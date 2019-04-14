using LastOutsiderShared.Connection;
using System;
using System.Collections.Generic;
using System.Text;

namespace LastOutsiderSharedTester.Test
{
    public class EncryptHelperTest : BaseTest
    {
        protected override string Name => "Encrypt Helper Test";

        protected override void TestInternal()
        {
            var data = new byte[1000];
            Random random = new Random();
            random.NextBytes(data);

            var server = new EncryptHelper();
            var client = new EncryptHelper();
            client.GenerateRandomAESKey();

            server.GenerateNewRSA();
            client.RSAPublicKey = server.RSAPublicKey;

            var encryptAESKeyRSA = client.EncryptRSA(client.AESKey);
            var decryptAESKeyRSA = server.DecryptRSA(encryptAESKeyRSA);
            Assert(client.AESKey, decryptAESKeyRSA);
            
            server.AESKey = decryptAESKeyRSA;

            var encryptDataAES = server.EncryptAES(data);
            var decryptDataAES = client.DecryptAES(encryptDataAES);

            Assert(data, decryptDataAES);
        }
    }
}
