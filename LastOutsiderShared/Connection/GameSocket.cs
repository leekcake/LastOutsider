using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace LastOutsiderShared.Connection
{
    /*
    GameSocket {
        DATA HEADER - 2 BYTE (0x39, 0xbe)
        IS PING - 1 BYTE (BOOL)
        if(!IS PING) {
            DATA LENGTH - 4 BYTE
            DATA CONTENT - (DATA LENGTH) BYTE
        }
    }
    */
    /// <summary>
    /// 클라이언트와 서버간 통신에 사용하는 소켓
    /// </summary>
    public class GameSocket
    {
        private EncryptHelper encryptHelper = new EncryptHelper();

        private Socket socket;
    }
}
