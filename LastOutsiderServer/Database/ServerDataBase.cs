using LastOutsiderShared.Data;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LastOutsiderServer.Database
{
    public class ServerDataBase
    {
        public static ServerDataBase Instance {
            get; private set;
        } = new ServerDataBase();

        private LiteDatabase database = new LiteDatabase(@"Server.db");

        private const string ACCOUNT_COLLECTION = "Accounts";

        public Account CreateAccount(byte[] authToken)
        {
            var account = new Account();
            account.authToken = authToken;
            var accounts = database.GetCollection<Account>(ACCOUNT_COLLECTION);
            accounts.Insert(account);

            return account;
        }

        public Account GetAccount(int id)
        {
            var collections = database.GetCollection<Account>(ACCOUNT_COLLECTION);
            var find = collections.Find(x => x.Id == id);
            if(find.Count() == 0)
            {
                return null;
            }
            return find.First();
        }
    }
}
