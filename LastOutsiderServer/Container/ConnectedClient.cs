using LastOutsiderShared.Connection;
using LastOutsiderShared.Data;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace LastOutsiderServer.Container
{
    public static class ConnectedClient
    {
        private static Dictionary<GameSocket, Account> Connections;

        public static void RegisterLogin(GameSocket socket, Account account)
        {
            foreach(KeyValuePair<GameSocket, Account> connection in Connections)
            {
                if(connection.Value.Id == account.Id)
                {
                    Connections.Remove(connection.Key);
                    break;
                }
            }
            Connections[socket] = account;

        }

        public static Account GetLogin(GameSocket socket, bool autoException = true)
        {
            if(Connections.ContainsKey(socket))
            {
                return Connections[socket];
            }
            else
            {
                if(autoException)
                {
                    throw new Exception("세션이 만료되었습니다");
                }
                return null;
            }
        }

        public static void UnregisterLogin(GameSocket socket)
        {
            Connections.Remove(socket);
        }
    }
}
