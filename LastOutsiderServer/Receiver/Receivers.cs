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
        }
    }
}
