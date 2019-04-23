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
            var account = ServerDataBase.Instance.GetAccount(1);
            if(account == null)
            {
                account = ServerDataBase.Instance.CreateAccount(new byte[128]);
            }

            Console.WriteLine("Test 3 Minute");
            var resource = ServerDataBase.Instance.GetResource(account.Id);
            resource.Money = 5000;
            resource.Food = 5000;
            resource.Electric = 5000;
            resource.Time = 5000;
            resource.ResourceRecoverMax = 15000;
            resource.NextRecoverTime = DateTime.UtcNow;
            ServerDataBase.Instance.UpdateResource(resource);
            var recovery = ServerDataBase.Instance.GetResource(account.Id);

            Assert(resource.Electric + resource.ElectricRecoveryAmount, recovery.Electric);
            Assert(resource.Food + resource.FoodRecoveryAmount, recovery.Food);
            Assert(resource.Time + resource.TimeRecoveryAmount, recovery.Time);
            Assert(resource.Money + resource.MoneyRecoveryAmount, recovery.Money);
            Console.WriteLine("Test 3 Minute OK");

            Console.WriteLine("Test 6 Minute");
            resource = ServerDataBase.Instance.GetResource(account.Id);
            resource.NextRecoverTime = DateTime.UtcNow.AddMinutes(-3);
            ServerDataBase.Instance.UpdateResource(resource);
            recovery = ServerDataBase.Instance.GetResource(account.Id);

            Assert(resource.Electric + (resource.ElectricRecoveryAmount * 2), recovery.Electric);
            Assert(resource.Food + (resource.FoodRecoveryAmount * 2), recovery.Food);
            Assert(resource.Time + (resource.TimeRecoveryAmount * 2), recovery.Time);
            Assert(resource.Money + (resource.MoneyRecoveryAmount * 2), recovery.Money);
            Console.WriteLine("Test 6 Minute OK");

            Console.WriteLine("Test Recovery Limit");
            resource = ServerDataBase.Instance.GetResource(account.Id);
            resource.Food = resource.ResourceRecoverMax - 4;
            resource.Money = resource.ResourceRecoverMax + 5; //과금 이나 원정등으로 자동회복치 초과, 이 경우 리셋되면 안됨
            resource.NextRecoverTime = DateTime.UtcNow.AddMinutes(-3);
            ServerDataBase.Instance.UpdateResource(resource);
            recovery = ServerDataBase.Instance.GetResource(account.Id);

            Assert(resource.Electric + (resource.ElectricRecoveryAmount * 2), recovery.Electric);
            Assert(resource.ResourceRecoverMax, recovery.Food);
            Assert(resource.Time + (resource.TimeRecoveryAmount * 2), recovery.Time);
            Assert(resource.ResourceRecoverMax + 5, recovery.Money);

            Console.WriteLine("Test Recovery Limit OK");
        }
    }
}
