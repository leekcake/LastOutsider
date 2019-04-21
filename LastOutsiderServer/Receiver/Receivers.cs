using LastOutsiderServer.Receiver.Login;
using LastOutsiderServer.Receiver.Resource;
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
            #region Login
            socket.registerRequestReceiver(new HandshakeReceiver(socket));
            socket.registerRequestReceiver(new LoginAccountReceiver(socket));
            socket.registerRequestReceiver(new GenerateAccountReceiver());
            socket.registerRequestReceiver(new FetchDataReceiver(socket));
            #endregion

            #region Resource
            socket.registerRequestReceiver(new GetResourceStatusReceiver(socket));
            #endregion
        }
    }
}
