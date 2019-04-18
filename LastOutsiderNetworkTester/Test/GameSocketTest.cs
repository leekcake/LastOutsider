using LastOutsiderShared.Connection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestBase;

namespace LastOutsiderNetworkTester.Test
{
    public class GameSocketTest : BaseTest
    {
        private static readonly byte[] needMoreTime = Encoding.UTF8.GetBytes("시간이 조금더 필요합니다");

        private class MessageReceiver : RequestReceiver
        {
            public string Key => "message";

            public Stream OnRequest(byte[] requestData)
            {
                return new MemoryStream(requestData);
            }
        }

        private class DevReceiver : RequestReceiver
        {
            public string Key => "dev";

            public Stream OnRequest(byte[] requestData)
            {
                return new MemoryStream(needMoreTime);
            }
        }

        private class FlagReceiver : ResponseReceiver
        {
            public bool IsReceived = false;
            public byte[] ReceivedData = null;

            public void OnResponse(byte[] response)
            {
                IsReceived = true;
                ReceivedData = response;
            }
        }

        protected override string Name => "Game Socket Test";

        protected override void TestInternal()
        {
            var listener = new TcpListener(IPAddress.Loopback, 8039);
            listener.Start();
            var serverTask = listener.AcceptTcpClientAsync();
            var clientTcp = new TcpClient();
            var clientTask = clientTcp.ConnectAsync(IPAddress.Loopback, 8039);

            Console.WriteLine("Waiting for Connect...");
            Task.WaitAll(serverTask, clientTask);

            Console.WriteLine("Connected! Wrapping with GameSocket");

            var server = new GameSocket();
            server.AttachNetworkStream(serverTask.GetAwaiter().GetResult().GetStream());
            var client = new GameSocket();
            client.AttachNetworkStream(clientTcp.GetStream());

            server.SendPingAsync();
            client.SendPingAsync();

            int tryCount = 0;
            while (server.LastPongTime == -1 || client.LastPongTime == -1)
            {
                Console.WriteLine($"서버와 클라이언트가 상대방의 핑을 기다리는중... {tryCount}00밀리초 지남");
                Thread.Sleep(100);
                tryCount++;
            }

            Console.WriteLine("서로 핑을 주고 받았습니다!");

            var random = new Random();

            server.registerRequestReceiver(new MessageReceiver());
            server.registerRequestReceiver(new DevReceiver());

            var messageFlag = new FlagReceiver();
            var devFlag = new FlagReceiver();

            var messageBuffer = new byte[1024 * 16];
            random.NextBytes(messageBuffer);
            client.SendRequestAsync("message", messageBuffer, messageFlag);

            client.SendRequestAsync("dev", new byte[] { 0x0 }, devFlag);

            tryCount = 0;
            while (!messageFlag.IsReceived || !devFlag.IsReceived)
            {
                Console.WriteLine($"클라이언트가 요청 2개의 응답을 기다리는중 {tryCount}00밀리초 지남");
                Thread.Sleep(100);
                tryCount++;
            }

            Console.WriteLine("클라이언트가 응답을 수신함");
            Assert(messageFlag.ReceivedData, messageBuffer);
            Assert(devFlag.ReceivedData, needMoreTime);

            Console.WriteLine("요청을 검증했습니다. 정상입니다.");
        }
    }
}
