﻿using LastOutsiderClientNetwork.Packet;
using LastOutsiderClientNetwork.Packet.Login;
using LastOutsiderClientNetwork.Packet.Resource;
using LastOutsiderServer.Receiver;
using LastOutsiderShared;
using LastOutsiderShared.Connection;
using LastOutsiderShared.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestBase;

#pragma warning disable 1998,4014

namespace LastOutsiderNetworkTester.Test
{
    public class GameSocketTest : BaseTest
    {
        private class Printer : PrintHelper
        {

            public void Printline(string line)
            {
                lock (this)
                {
                    Console.WriteLine(line);
                }
            }
        }

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

            public void OnResponseError(string message)
            {
                throw new Exception(message);
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
            var printer = new Printer();

            #region Make Connection
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
            #endregion

            #region Alive Checker - No Response at all (half-open tcp connection)
            Console.WriteLine("Alive Checker - No Response at all (half-open tcp connection)");
            var aliveCheckServerTask = listener.AcceptTcpClientAsync();
            var aliveCheckClient = new TcpClient();
            var aliveCheckClientTask = aliveCheckClient.ConnectAsync(IPAddress.Loopback, 8039);

            Task.WaitAll(aliveCheckClientTask, aliveCheckServerTask);

            var isClosed = false;
            var aliveCheckServerSocket = new GameSocket();
            aliveCheckServerSocket.printHelper = printer;
            aliveCheckServerSocket.AttachNetworkStream(aliveCheckServerTask.GetAwaiter().GetResult().GetStream(), () =>
            {
                isClosed = true;
            });
            aliveCheckServerSocket.LastReceiveTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() - 30000;

            Thread.Sleep(2000);
            aliveCheckServerSocket.LastReceiveTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() - 60000;
            Thread.Sleep(2000);
            Assert(isClosed, true);
            aliveCheckClient.Close();

            Console.WriteLine("Alive Checker - No Response at all (half-open tcp connection) is PASS");
            #endregion

            #region Alive Checker - Client Closed Connection
            Console.WriteLine("Alive Checker - Client Closed Connection");
            aliveCheckServerTask = listener.AcceptTcpClientAsync();
            aliveCheckClient = new TcpClient();
            aliveCheckClientTask = aliveCheckClient.ConnectAsync(IPAddress.Loopback, 8039);

            Task.WaitAll(aliveCheckClientTask, aliveCheckServerTask);

            isClosed = false;
            aliveCheckServerSocket = new GameSocket();
            aliveCheckServerSocket.printHelper = printer;
            aliveCheckServerSocket.AttachNetworkStream(aliveCheckServerTask.GetAwaiter().GetResult().GetStream(), () =>
            {
                isClosed = true;
            });
            aliveCheckClient.Close();
            aliveCheckServerSocket.LastReceiveTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() - 30000;
            Thread.Sleep(2000);
            aliveCheckServerSocket.LastReceiveTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() - 60000;
            Thread.Sleep(2000);
            Assert(isClosed, true);
            Console.WriteLine("Alive Checker - Client Closed Connection is PASS");
            #endregion

            #region Handshake
            Console.WriteLine("암호화를 활성화 합니다");

            var handshake = new HandshakePacket();
            handshake.SendPacketAsync(client, null);
            WaitForFlag(() => { return client.encryptHelper.UseAES; }, "연결의 암호화를 기다리는중... {0}");

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
            #endregion

            #region Login Packet
            var authToken = new byte[128];
            var rnd = new RNGCryptoServiceProvider();
            rnd.GetBytes(authToken);

            string failMessage = null;
            Action throwIfFail = new Action(() =>
            {
                if(failMessage != null)
                {
                    throw new Exception(failMessage);
                }
            });

            Account generatedAccount = null;
            var generateAccountPacket = new GenerateAccountPacket();
            generateAccountPacket.SendPacketAsync(client, authToken, new FinishListener<Account>((account) =>
            {
                generatedAccount = account;
            }, (message) =>
            {
                failMessage = message;
            }));
            WaitForFlag(() => { return generatedAccount != null || failMessage != null; }, "클라이언트가 서버의 계정 생성을 기다리는중... {0}");
            throwIfFail();

            var loginAccount = new LoginAccountPacket();
            var badLoginPass = false;
            //Bad Token Test
            loginAccount.SendPacketAsync(client, generatedAccount.Id, new byte[128], new FinishListener(() =>
            {
                failMessage = "클라이언트가 틀린 토큰으로 로그인을 성공했습니다";
            }, (message) => {
                badLoginPass = true;
            }));
            WaitForFlag(() => { return badLoginPass || failMessage != null; }, "클라이언트가 틀린 토큰으로 로그인 시도중... {0}");
            throwIfFail();

            var goodLoginPass = false;
            loginAccount.SendPacketAsync(client, generatedAccount.Id, authToken, new FinishListener(() =>
            {
                goodLoginPass = true;
            }, (message) => {
                failMessage = message;
            }));
            WaitForFlag(() => { return goodLoginPass || failMessage != null; }, "클라이언트가 정상 토큰으로 로그인 시도중... {0}");
            throwIfFail();

            var fetchDataPacket = new FetchDataPacket();
            FetchData data = null;
            fetchDataPacket.SendPacketAsync(client, new FinishListener<FetchData>((fetchData) =>
            {
                data = fetchData;
            }, (message) =>
            {
                failMessage = message;
            }));
            WaitForFlag(() => { return data != null || failMessage != null; }, "클라이언트가 시작에 필요한 정보를 가져오는중... {0}");
            throwIfFail();
            #endregion

            #region Resource Packet
            var resourcePacket = new GetResourceStatusPacket();
            bool resourceReceived = false;
            resourcePacket.SendPacketAsync(client, new FinishListener<Resource>((resource) =>
            {
                Console.WriteLine($"자원정보 수신함(Mo/Fo/El/Ti): {resource.Money}/{resource.Food}/{resource.Electric}/{resource.Time}");
                resourceReceived = true;
            }, (message) =>
            {
                failMessage = message;
            }));
            WaitForFlag(() => { return resourceReceived || failMessage != null; }, "자원 정보를 서버로부터 받는중... {0}");
            throwIfFail();

            listener.Stop();
            #endregion
        }
    }
}
