using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LastOutsiderShared.Connection
{
    public interface RequestReceiver
    {
        Task<Stream> OnRequest(byte[] requestData);

        string Key {
            get;
        }
    }
}
