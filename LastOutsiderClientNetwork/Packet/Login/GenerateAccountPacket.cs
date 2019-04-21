using LastOutsiderShared;
using LastOutsiderShared.Connection;
using LastOutsiderShared.Connection.Packet;
using LastOutsiderShared.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MessagePack;

namespace LastOutsiderClientNetwork.Packet.Login
{
    public class GenerateAccountPacket : BasePacket
    {
        public override string Key => "Login/Generate Account";

        private class GenerateAccountReceiver : ClientResponseReceiver<Account>
        {
            public GenerateAccountReceiver(GameSocket socket, FinishListener<Account> finishAction) : base(socket, finishAction)
            {
            }

            public override void OnResponse(byte[] response)
            {
                var account = MessagePackSerializer.Deserialize<Account>(response);
                finishListener.OnFinish(account);
            }

            public override void OnResponseError(string message)
            {
                finishListener?.OnError?.Invoke(message);
            }
        }

        public async Task SendPacketAsync(GameSocket socket, byte[] authToken, FinishListener<Account> finishAction)
        {
            await socket.SendRequestAsync(Key, authToken, new GenerateAccountReceiver(socket, finishAction));
        }
    }
}
