using LastOutsiderServer.Container;
using LastOutsiderServer.Database;
using LastOutsiderServer.Receiver.Base;
using LastOutsiderShared;
using LastOutsiderShared.Connection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LastOutsiderServer.Receiver.Resource
{
    public class GetResourceStatusReceiver : AfterLoginRequestReceiver
    {
        public GetResourceStatusReceiver(GameSocket socket) : base(socket)
        {
        }

        public override string Key => "Resource/Get Status";

        public async override Task<Stream> OnRequest(byte[] requestData)
        {
            var resource = ServerDataBase.Instance.GetResource(LoginedAccount.Id);

            var result = new MemoryStream();
            FormatterHolder.binaryFormatter.Serialize(result, resource);

            return result;
        }
    }
}
