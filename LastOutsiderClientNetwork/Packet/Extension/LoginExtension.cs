using LastOutsiderShared.Connection;
using LastOutsiderShared.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LastOutsiderClientNetwork.Packet.Extension.Login
{
    public static class LoginExtension
    {
        public static async Task GenerateAccount(this GameSocket socket, byte[] authToken, FinishListener<Account> finishListener)
        {
            await StaticPackets.GenerateAccount.SendPacketAsync(socket, authToken, finishListener);
        }

        public static async Task LoginAccount(this GameSocket socket, int id, byte[] authToken, FinishListener finishListener)
        {
            await StaticPackets.LoginAccount.SendPacketAsync(socket, id, authToken, finishListener);
        }

        public static async Task Handshake(this GameSocket socket, FinishListener finishListener)
        {
            await StaticPackets.Handshake.SendPacketAsync(socket, finishListener);
        }

        public static async Task FetchData(this GameSocket socket, FinishListener<FetchData> finishListener)
        {
            await StaticPackets.FetchData.SendPacketAsync(socket, finishListener);
        }
    }
}
