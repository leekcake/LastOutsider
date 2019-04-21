using LastOutsiderServer.Container;
using LastOutsiderServer.Receiver.Base;
using LastOutsiderShared;
using LastOutsiderShared.Connection;
using LastOutsiderShared.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LastOutsiderServer.Receiver.Login
{
    /// <summary>
    /// 게임이 시작하기 전 필요한 데이터를 전부 전달
    /// </summary>
    public class FetchDataReceiver : AfterLoginRequestReceiver
    {
        public FetchDataReceiver(GameSocket socket) : base(socket)
        {
        }

        public override string Key => "Login/Fetch Data";

        public override async Task<Stream> OnRequest(byte[] requestData)
        {
            var fetchData = new FetchData();

            //TODO: Fill with real data

            var stream = new MemoryStream();
            FormatterHolder.binaryFormatter.Serialize(stream, fetchData);
            stream.Position = 0;
            return stream;
        }
    }
}
