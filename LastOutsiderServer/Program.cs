using LastOutsiderServer.Container;
using LastOutsiderServer.Receiver;
using LastOutsiderShared;
using LastOutsiderShared.Connection;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace LastOutsiderServer
{
    class Program
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

        static void Main(string[] args)
        {
            var print = new Printer();

            var listener = new TcpListener(IPAddress.Any, 8039);
            listener.Start();
            print.Printline("서버가 8039 포트에서 클라이언트 대기");
            new Task(async () =>
            {
                while (true)
                {
                    var serverClient = await listener.AcceptTcpClientAsync();
                    var socket = new GameSocket();
                    socket.printHelper = print;
                    Receivers.RegisterReceivers(socket);
                    socket.AttachNetworkStream(serverClient.GetStream(), () =>
                    {
                        try
                        {
                            ConnectedClient.UnregisterLogin(socket);
                            serverClient.Close();
                        }
                        catch
                        {

                        }
                    });
                }
            }).Start();

            while(true)
            {
                //코더님, 역활이 없는 메인 스레드는 뭘 할수 있죠?
                //너는 쓸모가 없다, 팝콘이나 가져와라 메인 스레드
                Thread.Sleep(int.MaxValue);
            }
        }
    }
}
