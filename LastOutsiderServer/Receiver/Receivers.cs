using LastOutsiderServer.Receiver.Login;
using LastOutsiderShared.Connection;
using System;
using System.Collections.Generic;
using System.Text;

namespace LastOutsiderServer.Receiver
{
    public static class Receivers
    {
        public static void RegisterReceivers(GameSocket socket)
        {
            socket.registerRequestReceiver(new HandshakeReceiver(socket));
            socket.registerRequestReceiver(new LoginAccountReceiver(socket));
            socket.registerRequestReceiver(new GenerateAccountReceiver());
            socket.registerRequestReceiver(new FetchDataReceiver(socket));
        }
    }
}
