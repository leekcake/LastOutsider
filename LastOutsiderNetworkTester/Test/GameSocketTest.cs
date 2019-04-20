﻿using LastOutsiderClientNetwork.Packet.Login;
using LastOutsiderServer.Receiver;
using LastOutsiderShared;
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

        private static T GetResultOnTask<T>(Task<T> task)
        {
            task.Wait();
            return task.GetAwaiter().GetResult();
        }

        #region Data Transfer
        private class MessageReceiver : RequestReceiver
        {
            public string Key => "message";

            public async Task<Stream> OnRequest(byte[] requestData)
            {
                return new MemoryStream(requestData);
            }
        }

        private class DevReceiver : RequestReceiver
        {
            public string Key => "dev";

            public async Task<Stream> OnRequest(byte[] requestData)
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
        #endregion

        protected override string Name => "Game Socket Test";

        private void WaitForFlag(Func<bool> flag, string messageFormat)
        {
            int tryCount = 0;
            bool pureFalse = true;
            while (!flag.Invoke())
            {
                pureFalse = false;
                Console.WriteLine( string.Format(messageFormat, tryCount==0 ? "" : $"{tryCount}00밀리초 남음" ) );
                Thread.Sleep(100);
                tryCount++;
            }

            if(pureFalse)
            {
                Console.WriteLine(string.Format(messageFormat, "이미 처리됨"));
            }
            else
            {
                Console.WriteLine(string.Format(messageFormat, "처리됨"));
            }
        }

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

            Receivers.RegisterReceivers(server);

            server.SendPingAsync();
            client.SendPingAsync();

            WaitForFlag( () => { return server.LastPongTime == -1; } , "서버가 클라이언트의 핑을 기다리는중... {0}");
            WaitForFlag( () => { return client.LastPongTime == -1; } , "클라이언트가 서버의 핑을 기다리는중... {0}");

            Console.WriteLine("서로 핑을 주고 받았습니다!");

            Console.WriteLine("암호화를 활성화 합니다");

            var handshake = new HandshakePacket();
            handshake.SendPacketAsync(client, null);
            WaitForFlag(() => { return client.encryptHelper.UseAES; }, "연결의 암호화를 기다리는중...");

            var random = new Random();

            server.registerRequestReceiver(new MessageReceiver());
            server.registerRequestReceiver(new DevReceiver());

            var messageFlag = new FlagReceiver();
            var devFlag = new FlagReceiver();

            var messageBuffer = new byte[1024 * 16];
            random.NextBytes(messageBuffer);
            client.SendRequestAsync("message", messageBuffer, messageFlag);

            client.SendRequestAsync("dev", new byte[] { 0x0 }, devFlag);

            WaitForFlag(() => { return messageFlag.IsReceived; }, "클라이언트가 랜덤 바이트 테스트의 응답을 기다리는중... {0}");
            WaitForFlag(() => { return devFlag.IsReceived; }, "클라이언트가 고정 메시지 테스트의 응답을 기다리는중... {0}");

            Console.WriteLine("클라이언트가 응답을 수신함");
            Assert(messageFlag.ReceivedData, messageBuffer);
            Assert(devFlag.ReceivedData, needMoreTime);

            Console.WriteLine("요청을 검증했습니다. 정상입니다.");

            listener.Stop();
        }
    }
}
