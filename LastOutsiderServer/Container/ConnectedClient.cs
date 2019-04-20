using LastOutsiderShared.Connection;
using LastOutsiderShared.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace LastOutsiderServer.Container
{
    public static class ConnectedClient
    {
        private static Dictionary<GameSocket, Account> Connections;

        public static void RegisterLogin(GameSocket socket, Account account)
        {
            Connections[socket] = account;
        }

        public static Account GetLogin(GameSocket socket)
        {
            if(Connections.ContainsKey(socket))
            {
                return Connections[socket];
            }
            else
            {
                return null;
            }
        }

        public static void UnregisterLogin(GameSocket socket)
        {
            Connections.Remove(socket);
        }
    }
}
