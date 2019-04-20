using LastOutsiderServer.Container;
using LastOutsiderServer.Database;
using LastOutsiderShared.Connection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LastOutsiderServer.Receiver.Login
{
    public class LoginAccountReceiver : RequestReceiver
    {
        public GameSocket socket;
        public LoginAccountReceiver(GameSocket socket)
        {
            this.socket = socket;
        }

        public string Key => "Login/Login Account";

        public async Task<Stream> OnRequest(byte[] requestData)
        {
            var stream = new MemoryStream(requestData);

            var id = await stream.ReceiveInt();
            var token = await stream.ReceiveByteArray();

            var account = ServerDataBase.Instance.GetAccount(id);
            if(account == null)
            {
                throw new Exception("계정 ID가 유효하지 않습니다");
            }

            if(!Enumerable.SequenceEqual(token, account.authToken) )
            {
                throw new Exception("계정 ID가 유효하지 않습니다"); //아이디는 있지만 Auth 토큰이 틀렸어도 알려주지 않음
                //TODO: 인증에 실패했지만 계속 연결을 시도하는 경우(brute force) 차단조치
            }

            ConnectedClient.RegisterLogin(socket, account);

            return new MemoryStream();
        }
    }
}
