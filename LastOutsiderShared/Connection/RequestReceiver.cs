using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LastOutsiderShared.Connection
{
    public interface RequestReceiver
    {
        Stream OnRequest(byte[] requestData);

        string Key {
            get;
        }
    }
}
