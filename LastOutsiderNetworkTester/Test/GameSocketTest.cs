using LastOutsiderShared.Connection;
using System;
using System.Collections.Generic;
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
            while(server.LastPongTime == -1 || client.LastPongTime == -1)
            {
                Console.WriteLine($"서버와 클라이언트가 상대방의 핑을 기다리는중... {tryCount}00밀리초 지남");
                Thread.Sleep(100);
                tryCount++;
            }

            Console.WriteLine("서로 핑을 주고 받았습니다!");
        }
    }
}
