using LastOutsiderShared;
using LastOutsiderShared.Connection;
using LastOutsiderShared.Connection.Packet;
using LastOutsiderShared.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LastOutsiderClientNetwork.Packet.Login
{
    public class FetchDataPacket : BasePacket
    {
        public override string Key => "Login/Fetch Data";

        private class GenerateAccountReceiver : ClientResponseReceiver<FetchData>
        {
            public GenerateAccountReceiver(GameSocket socket, FinishListener<FetchData> finishAction) : base(socket, finishAction)
            {
            }

            public override void OnResponse(byte[] response)
            {
                var fetchData = (FetchData)FormatterHolder.binaryFormatter.Deserialize(new MemoryStream(response));
                finishListener.OnFinish(fetchData);
            }

            public override void OnResponseError(string message)
            {
                finishListener?.OnError?.Invoke(message);
            }
        }

        public async Task SendPacketAsync(GameSocket socket, FinishListener<FetchData> finishAction)
        {
            await socket.SendRequestAsync(Key, new GenerateAccountReceiver(socket, finishAction));
        }
    }
}
