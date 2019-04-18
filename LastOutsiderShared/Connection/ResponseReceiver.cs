using System;
using System.Collections.Generic;
using System.Text;

namespace LastOutsiderShared.Connection
{
    public interface ResponseReceiver
    {
        void OnResponse(byte[] response);
    }
}
