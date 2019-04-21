using LastOutsiderShared;
using LastOutsiderShared.Connection;
using LastOutsiderShared.Connection.Packet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ResourceData = LastOutsiderShared.Data.Resource;

namespace LastOutsiderClientNetwork.Packet.Resource
{
    public class GetResourceStatusPacket : BasePacket
    {
        public override string Key => "Resource/Get Status";

        private class GenerateAccountReceiver : ClientResponseReceiver<ResourceData>
        {
            public GenerateAccountReceiver(GameSocket socket, FinishListener<ResourceData> finishAction) : base(socket, finishAction)
            {
            }

            public override void OnResponse(byte[] response)
            {
                var account = (ResourceData)FormatterHolder.binaryFormatter.Deserialize(new MemoryStream(response));
                finishListener.OnFinish(account);
            }

            public override void OnResponseError(string message)
            {
                finishListener?.OnError?.Invoke(message);
            }
        }

        public async Task SendPacketAsync(GameSocket socket, FinishListener<ResourceData> finishAction)
        {
            await socket.SendRequestAsync(Key, new GenerateAccountReceiver(socket, finishAction));
        }
    }
}
