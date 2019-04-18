using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace LastOutsiderShared.Connection
{
    /*
    GameSocket {
	    DATA HEADER - 2 BYTE (0x39, 0xbe)
	    DATA TYPE - 1 BYTE
	    if(DATA TYPE != PING) {
		    SPACE INX - 4 BYTE
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
        private static readonly byte[] HEADER = new byte[]
        {
            0x39, 0xbe
        };

        private static readonly byte[] PING_DATA = new byte[]
        {
            //HEADER
            0x39, 0xbe,
            //TYPE
            (byte) DataType.Ping
        };

        private static readonly byte[] PONG_DATA = new byte[]
        {
            //HEADER
            0x39, 0xbe,
            //TYPE
            (byte) DataType.Pong
        };

        public enum DataType : byte
        {
            Ping = 0,
            Pong = 1,
            Request = 2,
            Response = 3,
            Check = 4
        }

        public class ReadTask
        {
            private GameSocket owner;

            public ReadTask(GameSocket owner)
            {
                this.owner = owner;
            }

            private async Task readFullyOnNetwork(byte[] buffer, int off, int len)
            {
                int left = len;
                while (left > 0)
                {
                    var read = await owner.networkStream.ReadAsync(buffer, off, left);
                    if (read == 0) //Stream closed?
                    {
                        if (!owner.networkStream.CanRead)
                        {
                            throw new Exception("Can't read from NetworkStream, Closed Connection?");
                        }
                        continue;
                    }
                    off += read;
                    left -= read;
                }
            }

            private async Task<byte[]> readFullyOnNetwork(int len)
            {
                var buf = new byte[len];
                readFullyOnNetwork(buf, 0, len);
                return buf;
            }

            public void Stop()
            {
                readTaskCancellationToken.Cancel();
            }

            public void Run()
            {
                Task.Factory.StartNew(async () =>
               {
                   try
                   {
                       byte[] headerBuffer = new byte[2];
                       while (true)
                       {
                           readTaskCancellationToken.Token.ThrowIfCancellationRequested();
                           var header = await readFullyOnNetwork(HEADER.Length);
                           readTaskCancellationToken.Token.ThrowIfCancellationRequested();
                           if (!Enumerable.SequenceEqual(header, HEADER))
                           {
                               //다른 통신 주체가 이 패킷 규약을 따르지 않는 다른 프로그램인 경우
                               //드물게 TCP 통신이 오염되었고 그 오염된 데이터를 TCP가 살리지 못한경우
                               try
                               {
                                   owner.networkStream.Close(); //말이 안통하는게 당연하기 때문에 일단 연결을 닫음
                               }
                               catch { }
                               break;
                           }


                           var type = await readFullyOnNetwork(1);
                           switch ((DataType)type[0])
                           {
                               case DataType.Ping:
                                   owner.SendPongAsync();
                                   break;
                               case DataType.Pong:
                                   owner.LastPongTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                                   //소켓이 아직 살아있음!
                                   break;
                           }
                       }
                   }
                   catch (Exception ex)
                   {
                       //TODO: 더 나은 에러 체크
                   }
               });
            }

            private CancellationTokenSource readTaskCancellationToken = new CancellationTokenSource();

        }

        private EncryptHelper encryptHelper = new EncryptHelper();
        private NetworkStream networkStream;

        private uint currentSpaceInx = 0;

        public long LastPongTime = -1;

        private ReadTask readTask;
        public void AttachNetworkStream(NetworkStream stream)
        {
            networkStream = stream;

            readTask?.Stop();
            readTask = new ReadTask(this);
            readTask.Run();
        }

        public Task SendPingAsync()
        {
            return networkStream.WriteAsync(PING_DATA, 0, PING_DATA.Length);
        }

        public Task SendPongAsync()
        {
            return networkStream.WriteAsync(PONG_DATA, 0, PONG_DATA.Length);
        }
    }
}
