using LastOutsiderServer.Database;
using LastOutsiderShared;
using LastOutsiderShared.Connection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace LastOutsiderServer.Receiver.Login
{
    public class GenerateAccountReceiver : RequestReceiver
    {
        public string Key => "Login/Generate Account";

        public async Task<Stream> OnRequest(byte[] requestData)
        {
            if(requestData.Length != 128)
            {
                throw new Exception("계정 생성 오류: 요청이 손상되었습니다");
            }

            var account = ServerDataBase.Instance.CreateAccount(requestData);
            var result = new MemoryStream();
            FormatterHolder.binaryFormatter.Serialize(result, account);
            result.Position = 0;
            return result;
        }

    }
}
