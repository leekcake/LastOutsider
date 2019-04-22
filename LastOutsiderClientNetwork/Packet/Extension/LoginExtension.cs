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
        public static async Task<Account> GenerateAccountAsync(this GameSocket socket, byte[] authToken, FinishListener<Account> finishListener)
        {
            await StaticPackets.GenerateAccount.SendPacketAsync(socket, authToken, finishListener);
            return await finishListener.WaitAsync();
        }

        public static async Task LoginAccountAsync(this GameSocket socket, int id, byte[] authToken, FinishListener finishListener)
        {
            await StaticPackets.LoginAccount.SendPacketAsync(socket, id, authToken, finishListener);
            await finishListener.WaitAsync();
        }

        public static async Task HandshakeAsync(this GameSocket socket, FinishListener finishListener)
        {
            await StaticPackets.Handshake.SendPacketAsync(socket, finishListener);
            await finishListener.WaitAsync();
        }

        public static async Task<FetchData> FetchDataAsync(this GameSocket socket, FinishListener<FetchData> finishListener)
        {
            await StaticPackets.FetchData.SendPacketAsync(socket, finishListener);
            return await finishListener.WaitAsync();
        }
    }
}
