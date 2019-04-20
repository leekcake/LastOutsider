using LastOutsiderShared.Data;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace LastOutsiderServer.Database
{
    public class ServerDataBase
    {
        private LiteDatabase database = new LiteDatabase(@"Server.db");

        private const string ACCOUNT_COLLECTION = "Accounts";

        public Account CreateAccount(byte[] guestToken)
        {
            var account = new Account();
            var accounts = database.GetCollection<Account>(ACCOUNT_COLLECTION);
            accounts.Insert(account);

            return account;
        }
    }
}
