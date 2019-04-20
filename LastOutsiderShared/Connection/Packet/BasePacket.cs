using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LastOutsiderShared.Connection.Packet
{
    public abstract class BasePacket
    {
        public abstract string Key {
            get;
        }
    }
}
