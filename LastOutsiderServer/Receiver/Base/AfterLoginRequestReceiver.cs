using LastOutsiderServer.Container;
using LastOutsiderShared.Connection;
using LastOutsiderShared.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LastOutsiderServer.Receiver.Base
{
    public abstract class AfterLoginRequestReceiver : RequestReceiver
    {
        protected GameSocket socket;
        public AfterLoginRequestReceiver(GameSocket socket)
        {
            this.socket = socket;
        }

        private Account account;
        public Account LoginedAccount {
            get {
                if(account == null)
                {
                    account = ConnectedClient.GetLogin(socket);
                }
                return account;
            }
        }

        public abstract string Key {
            get;
        }

        public abstract Task<Stream> OnRequest(byte[] requestData);
    }
}
