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
        protected override string Name => "Strees Test of Game Socket";

        private const int MAX_RESPONSE_TIME = 100;
        private List<int> responseTimes = new List<int>(MAX_RESPONSE_TIME);

        private void PushNewResponseTime(int time)
        {
            lock (this)
            {
                if (responseTimes.Count == MAX_RESPONSE_TIME)
                {
                    responseTimes.RemoveAt(0);
                }
                responseTimes.Add(time);
                responseCount++;
            }
        }

        private int responseCount = 0;

        private int Min {
            get {
                try
                {
                    return responseTimes.Min();
                }
                catch
                {
                    return -1;
                }
            }
        }

        private double Avg {
            get {
                try
                {
                    return Math.Round(responseTimes.Average(), 3);
                }
                catch
                {
                    return -1;
                }
            }
        }

        private int Max {
            get {
                try
                {

                    return responseTimes.Max();
                }
                catch
                {
                    return -1;
                }
            }
        }

        private class DummyRequestReceiver : RequestReceiver
        {
            public string Key => "dummy";

            public async Task<Stream> OnRequest(byte[] requestData)
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
            private static byte[] data = new byte[1024 * 32];
            static TestLauncher()
            {
                Random random = new Random();
                random.NextBytes(data);
            }

            private GameSocketStressTest owner;

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
                    
                    while (owner.TestAlive)
                    {
                        long start = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                        var DRR = new DummyResponseReceiver();
                        client.SendRequestAsync("dummy", data, DRR);

                        while(!DRR.Responsed)
                        {
                            await Task.Delay(10);
                        }

                        long end = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                        owner.PushNewResponseTime((int)(end - start));

                        await Task.Delay( 1000 );
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

            while (TestAlive)
            {
                for (int i = 0; i < 10; i++)
                {
                    var serverTask = listener.AcceptTcpClientAsync();
                    var clientTcp = new TcpClient();
                    var clientTask = clientTcp.ConnectAsync(IPAddress.Loopback, 8039);

                    Task.WaitAll(serverTask, clientTask);

                    new TestLauncher(this, serverTask.GetAwaiter().GetResult().GetStream(), clientTcp.GetStream()).Run();
                    clientCount++;
                }
                Thread.Sleep(1000);

                lock (this)
                {
                    if (Avg > 5000)
                    {
                        TestAlive = false;
                        Console.WriteLine($"End State(Client(ResponseCount)/Min/Avg/Max) : {clientCount}({responseCount}) / {Min}ms / {Avg}ms / {Max}ms");
                    }
                    else
                    {
                        Console.WriteLine($"Current State(Client(ResponseCount)/Min/Avg/Max) : {clientCount}({responseCount}) / {Min}ms / {Avg}ms / {Max}ms");
                    }
                    responseCount = 0;
                }
            }

            listener.Stop();
        }
    }
}
