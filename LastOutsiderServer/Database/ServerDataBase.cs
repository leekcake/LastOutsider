using LastOutsiderServer.Data;
using LastOutsiderShared.Data;
using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
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

            mapper.Entity<ResourceInServer>()
                .Id(x => x.UserId);

            //Directory.CreateDirectory("Accounts");
        }

        private LiteDatabase database = new LiteDatabase(@"filename=Server.db;utc=true;");

        private const string ACCOUNT_COLLECTION = "Accounts";
        private const string RESOURCE_COLLECTION = "Resources";

        public Account CreateAccount(byte[] authToken)
        {
            var account = new Account();
            account.AuthToken = authToken;
            var accounts = database.GetCollection<Account>(ACCOUNT_COLLECTION);
            accounts.Insert(account);
            accounts.EnsureIndex(x => x.Id);

            var resource = new ResourceInServer();
            resource.Electric = 5000;
            resource.Food = 5000;
            resource.Money = 5000;
            resource.Time = 5000;
            resource.NextRecoverTime = DateTime.UtcNow.AddMinutes(3);
            resource.UserId = account.Id;

            var resources = database.GetCollection<ResourceInServer>(RESOURCE_COLLECTION);
            resources.Insert(resource);
            resources.EnsureIndex(x => x.UserId);

            return account;
        }

        public Account GetAccount(int id)
        {
            var collections = database.GetCollection<Account>(ACCOUNT_COLLECTION);
            return collections.FindOne(x => x.Id == id);
        }

        public ResourceInServer GetResource(int id)
        {
            var collections = database.GetCollection<ResourceInServer>(RESOURCE_COLLECTION);
            
            var resource = collections.FindOne(x => x.UserId == id);
            //TODO: 더 나은 자연회복 처리시점
            //TODO: 더 나은 자연회복 처리코드 (while 대신 곱하기 사용)
            bool updated = false;
            int compare;
            while ( ( compare = DateTime.Compare(DateTime.UtcNow, resource.NextRecoverTime) ) > 0 )
            {
                resource.Money += resource.CalcRecover(resource.Money, resource.ResourceRecoverMax, resource.MoneyRecoveryAmount);
                resource.Food += resource.CalcRecover(resource.Money, resource.ResourceRecoverMax, resource.FoodRecoveryAmount);
                resource.Electric += resource.CalcRecover(resource.Money, resource.ResourceRecoverMax, resource.ElectricRecoveryAmount);
                resource.Time += resource.CalcRecover(resource.Money, resource.ResourceRecoverMax, resource.TimeRecoveryAmount);
                resource.NextRecoverTime = resource.NextRecoverTime.AddMinutes(3);
                updated = true;
            }
            if (updated)
            {
                UpdateResource(resource);
            }
            return resource;
        }        

        public void UpdateResource(ResourceInServer resource)
        {
            var collections = database.GetCollection<ResourceInServer>(RESOURCE_COLLECTION);
            collections.Update(resource);
        }
    }
}
