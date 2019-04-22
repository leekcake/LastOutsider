using LastOutsiderServer.Database;
using System;
using System.Collections.Generic;
using System.Text;
using TestBase;

namespace LastOutsiderServerTester.Test
{
    public class ResourceRecoveryTest : BaseTest
    {
        protected override string Name => "Resource Recovery Test";

        protected override void TestInternal()
        {
            var account = ServerDataBase.Instance.GetAccount(0);
            if(account == null)
            {
                account = ServerDataBase.Instance.CreateAccount(new byte[128]);
            }

            var resource = ServerDataBase.Instance.GetResource(account.Id);
            resource.NextRecoverTime = DateTime.UtcNow;
            ServerDataBase.Instance.UpdateResource(resource);
            var recovery = ServerDataBase.Instance.GetResource(account.Id);

            Assert(resource.Electric + resource.ElectricRecoveryAmount, recovery.Electric);
            Assert(resource.Food + resource.FoodRecoveryAmount, recovery.Food);
            Assert(resource.Time + resource.TimeRecoveryAmount, recovery.Time);
            Assert(resource.Money + resource.MoneyRecoveryAmount, recovery.Money);

            resource = ServerDataBase.Instance.GetResource(account.Id);
            resource.NextRecoverTime = DateTime.UtcNow.AddMinutes(-3);
            ServerDataBase.Instance.UpdateResource(resource);
            recovery = ServerDataBase.Instance.GetResource(account.Id);

            Assert(resource.Electric + (resource.ElectricRecoveryAmount * 2), recovery.Electric);
            Assert(resource.Food + (resource.FoodRecoveryAmount * 2), recovery.Food);
            Assert(resource.Time + (resource.TimeRecoveryAmount * 2), recovery.Time);
            Assert(resource.Money + (resource.MoneyRecoveryAmount * 2), recovery.Money);
        }
    }
}
