using LastOutsiderShared.Connection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestBase;

namespace LastOutsiderNetworkTester.Test
{
    public class GameSocketStressTest : BaseTest
    {
        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            for (int i = 0; i < Console.WindowWidth; i++)
                Console.Write(" ");
            Console.SetCursorPosition(0, currentLineCursor);
        }

        protected override string Name => "Strees Test of Game Socket";

        private const int MAX_RESPONSE_TIME = 100;
        private List<int> responseTimes = new List<int>(MAX_RESPONSE_TIME);

        private void PushNewResponseTime(int time)
        {
            if(responseTimes.Count == MAX_RESPONSE_TIME)
            {
                responseTimes.RemoveAt(0);
            }
            responseTimes.Add(time);
        }

        private int Min {
            get {
                return responseTimes.Min();
            }
        }

        private double Avg {
            get {
                return Math.Round(responseTimes.Average(), 3);
            }
        }

        private int Max {
            get {
                return responseTimes.Max();
            }
        }

        private class DummyRequestReceiver : RequestReceiver
        {
            public string Key => "dummy";

            public Stream OnRequest(byte[] requestData)
            {
                return new MemoryStream(requestData);
            }
        }

        private class DummyResponseReceiver : ResponseReceiver
        {
            public ManualResetEvent ResponsedEvent = new ManualResetEvent(false);
            public bool Responsed = false;

            public void OnResponse(byte[] response)
            {
                Responsed = true;
                ResponsedEvent.Set();
            }
        }

        private class TestLauncher
        {
            private GameSocketStressTest owner;

            private Random random = new Random();

            private GameSocket server, client;

            public TestLauncher(GameSocketStressTest owner, NetworkStream server, NetworkStream client)
            {
                this.owner = owner;
                this.server = new GameSocket();
                this.server.registerRequestReceiver(new DummyRequestReceiver());
                this.server.AttachNetworkStream(server);
                this.client = new GameSocket();
                this.client.AttachNetworkStream(client);
            }

            public void Run()
            {
                new Task(async () =>
                {
                    while( owner.TestAlive )
                    {
                        long start = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                        await client.SendPingAsync();
                        await server.SendPingAsync();

                        byte[] data = new byte[ random.Next(1024, 1024*64) ];
                        random.NextBytes(data);

                        var DRR = new DummyResponseReceiver();
                        await client.SendRequestAsync("dummy", data, DRR);

                        DRR.ResponsedEvent.WaitOne();

                        long end = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                        owner.PushNewResponseTime((int)(end - start));

                        await Task.Delay( random.Next(100, 10000) );
                    }
                }).Start();
            }
        }

        public bool TestAlive = true;

        protected override void TestInternal()
        {
            var listener = new TcpListener(IPAddress.Loopback, 8039);
            listener.Start();

            int clientCount = 0;

            //Client Count Updater
            Console.WriteLine("Game Socket Stress Test");
            Console.WriteLine("Test unilt avg over 5000ms");
            Console.WriteLine("Waiting for Current State");
            new Task(async () =>
            {
                while (TestAlive)
                {
                    ClearCurrentConsoleLine();
                    if (Avg > 5000)
                    {
                        TestAlive = false;
                        Console.WriteLine($"End State(Client/Min/Avg/Max) : ${clientCount}C / {Min}ms / {Avg}ms {Max}ms");
                    }
                    else
                    {
                        Console.WriteLine($"Current State(Client/Min/Avg/Max) : ${clientCount}C / {Min}ms / {Avg}ms {Max}ms");
                    }
                    await Task.Delay(1000);
                }
            }).Start();

            while (TestAlive)
            {
                var serverTask = listener.AcceptTcpClientAsync();
                var clientTcp = new TcpClient();
                var clientTask = clientTcp.ConnectAsync(IPAddress.Loopback, 8039);

                Task.WaitAll(serverTask, clientTask);

                new TestLauncher(this, serverTask.GetAwaiter().GetResult().GetStream(), clientTcp.GetStream()).Run();

                Thread.Sleep(1000);
            }

            listener.Stop();
        }
    }
}
