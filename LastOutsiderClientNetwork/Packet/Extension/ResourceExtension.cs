using LastOutsiderShared.Connection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using ResourceData = LastOutsiderShared.Data.Resource;

namespace LastOutsiderClientNetwork.Packet.Extension
{
    public static class ResourceExtension
    {
        public static async Task GetResourceStatus(this GameSocket socket, FinishListener<ResourceData> finishListener)
        {
            await StaticPackets.GetResourceStatus.SendPacketAsync(socket, finishListener);
        }
    }
}
