using LastOutsiderServer.Receiver;
using LastOutsiderShared.Connection;
using System;
using System.Net;
using System.Net.Sockets;
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

        }
    }
}
