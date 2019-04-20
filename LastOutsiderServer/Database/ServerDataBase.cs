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

        public ServerDataBase()
        {
            var mapper = BsonMapper.Global;

            mapper.Entity<Account>()
                .Id(x => x.Id);
        }

        private LiteDatabase database = new LiteDatabase(@"Server.db");

        private const string ACCOUNT_COLLECTION = "Accounts";

        public Account CreateAccount(byte[] authToken)
        {
            var account = new Account();
            account.authToken = authToken;
            var accounts = database.GetCollection<Account>(ACCOUNT_COLLECTION);
            accounts.Insert(account);
            accounts.EnsureIndex(x => x.Id);

            return account;
        }

        public Account GetAccount(int id)
        {
            var collections = database.GetCollection<Account>(ACCOUNT_COLLECTION);
            return collections.FindOne(x => x.Id == id);
        }
    }
}
