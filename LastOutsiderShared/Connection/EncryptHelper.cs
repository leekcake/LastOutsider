using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using static LastOutsiderShared.Connection.RSACryptoServiceProviderExtensions;

namespace LastOutsiderShared.Connection
{
    /// <summary>
    /// 패킷 통신간 암호화를 도움
    /// </summary>
    public class EncryptHelper
    {
        public bool UseRSA {
            get; set;
        } = false;
        public bool UseAES {
            get; set;
        } = false;

        public bool UseEncrypt {
            get {
                return UseAES || UseRSA;
            }
        }

        #region RSA
        private string RSAPrivateKey = null;
        public string RSAPublicKey {
            get;
            set;
        } = null;

        public void GenerateNewRSA()
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            RSAParameters privateKey = RSA.Create().ExportParameters(true);
            rsa.ImportParameters(privateKey);
            RSAPrivateKey = RSACryptoServiceProviderExtensions.ToXmlString(rsa);

            RSAParameters publicKey = new RSAParameters();
            publicKey.Modulus = privateKey.Modulus;
            publicKey.Exponent = privateKey.Exponent;
            RSAPublicKey = RSACryptoServiceProviderExtensions.ToXmlString(rsa);
        }

        public byte[] EncryptRSA(byte[] original)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            RSACryptoServiceProviderExtensions.FromXmlString(rsa, RSAPublicKey);
            return rsa.Encrypt(original, false);
        }

        public byte[] DecryptRSA(byte[] encrypted)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            RSACryptoServiceProviderExtensions.FromXmlString(rsa, RSAPrivateKey);
            return rsa.Decrypt(encrypted, false);
        }
        #endregion

        #region AES
        public byte[] AESKey {
            get;
            set;
        }
        
        public void GenerateRandomAESKey()
        {
            var rnd = new RNGCryptoServiceProvider();
            var b = new byte[16];
            rnd.GetBytes(b);
            AESKey = b;
        }

        private RijndaelManaged CreateAESCipher()
        {
            RijndaelManaged cipher = new RijndaelManaged();
            cipher.Mode = CipherMode.CBC;
            cipher.Padding = PaddingMode.PKCS7;

            cipher.KeySize = 128;
            cipher.BlockSize = 128;
            cipher.Key = AESKey;
            cipher.IV = AESKey;

            return cipher;
        }

        public byte[] EncryptAES(byte[] original, int offset = 0, int length = -1)
        {
            if(length == -1)
            {
                length = original.Length;
            }
            var cipher = CreateAESCipher();
            var encryptor = cipher.CreateEncryptor();
            var result = encryptor.TransformFinalBlock(original, offset, length);
            encryptor.Dispose();
            return result;
        }

        public byte[] DecryptAES(byte[] encrypted, int offset = 0, int length = -1)
        {
            if (length == -1)
            {
                length = encrypted.Length;
            }
            var cipher = CreateAESCipher();
            var decrypter = cipher.CreateDecryptor();
            var result = decrypter.TransformFinalBlock(encrypted, offset, length);
            decrypter.Dispose();
            return result;
        }

        #endregion
    }
}
