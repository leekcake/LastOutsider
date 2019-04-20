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

        public async Task SendPacketAsync(GameSocket socket, int id, byte[] authToken, FinishListener finishAction)
        {
            var message = new MemoryStream();
            await message.WriteAsync(id);
            await message.WriteByteArrayAsync(authToken);

            await socket.SendRequestAsync(Key, message, (int) message.Length, new LoginAccountReceiver(socket, finishAction));
        }
    }
}
