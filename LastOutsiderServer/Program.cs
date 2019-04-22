using LastOutsiderServer.Receiver;
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
        static void Main(string[] args)
        {
            var listener = new TcpListener(IPAddress.Any, 8039);
            listener.Start();
            new Task(async () =>
            {
                while (true)
                {
                    var serverClient = await listener.AcceptTcpClientAsync();
                    var socket = new GameSocket();
                    Receivers.RegisterReceivers(socket);
                    socket.AttachNetworkStream(serverClient.GetStream());
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
