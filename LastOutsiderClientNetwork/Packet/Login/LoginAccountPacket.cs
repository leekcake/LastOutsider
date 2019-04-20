using LastOutsiderShared.Connection;
using LastOutsiderShared.Connection.Packet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LastOutsiderClientNetwork.Packet.Login
{
    public class LoginAccountPacket : BasePacket
    {
        public override string Key => "Login/Login Account";

        private class LoginAccountReceiver : ClientResponseReceiver
        {
            public LoginAccountReceiver(GameSocket socket, FinishListener finishAction) : base(socket, finishAction)
            {
            }

            public override void OnResponse(byte[] response)
            {
                finishListener.OnFinish();
            }

            public override void OnResponseError(string message)
            {
                finishListener?.OnError?.Invoke(message);
            }
        }

        public async Task SendPacketAsync(GameSocket socket, byte[] authToken, FinishListener finishAction)
        {
            await socket.SendRequestAsync(Key, authToken, new LoginAccountReceiver(socket, finishAction));
        }
    }
}
