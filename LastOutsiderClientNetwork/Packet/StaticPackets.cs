using LastOutsiderClientNetwork.Packet.Login;
using LastOutsiderClientNetwork.Packet.Resource;
using System;
using System.Collections.Generic;
using System.Text;

namespace LastOutsiderClientNetwork.Packet.Extension
{
    public static class StaticPackets
    {
        #region Login
        public readonly static GenerateAccountPacket GenerateAccount = new GenerateAccountPacket();
        public readonly static LoginAccountPacket LoginAccount = new LoginAccountPacket();
        public readonly static HandshakePacket Handshake = new HandshakePacket();
        public readonly static FetchDataPacket FetchData = new FetchDataPacket();
        #endregion

        #region Resource
        public readonly static GetResourceStatusPacket GetResourceStatus = new GetResourceStatusPacket();
        #endregion
    }
}
