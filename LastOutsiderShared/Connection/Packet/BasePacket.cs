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

        public abstract byte[] Data {
            get;
        }

        public abstract ResponseReceiver ResponseReceiver {
            get;
        }

        protected async Task Send(GameSocket socket)
        {
            await socket.SendRequestAsync(Key, Data, ResponseReceiver);
        }
    }
}
