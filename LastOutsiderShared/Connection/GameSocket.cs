using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

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
        #region Pre-defined Head of Packet
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

        private static readonly byte[] REQUEST_HEAD = new byte[]
        {
            //HEADER
            0x39, 0xbe,
            //TYPE
            (byte) DataType.Request
        };

        private static readonly byte[] RESPONSE_HEAD = new byte[]
        {
            //HEADER
            0x39, 0xbe,
            //TYPE
            (byte) DataType.Response
        };
        #endregion

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
                           var header = await owner.Receive(HEADER.Length);
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

                           var type = await owner.Receive(1);
                           switch ((DataType)type[0])
                           {
                               case DataType.Ping:
                                   owner.SendPongAsync();
                                   break;
                               case DataType.Pong:
                                   owner.LastPongTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                                   //소켓이 아직 살아있음!
                                   break;
                               case DataType.Request:
                                   {
                                       var spaceInx = await owner.ReceiveUInt();
                                       var key = await owner.ReceiveString();
                                       var data = await owner.ReceiveByteArray();
                                       if (owner.requestReceivers.ContainsKey(key))
                                       {
                                           new Task(async () =>
                                           {
                                               var receiver = owner.requestReceivers[key];
                                               var response = receiver.OnRequest(data);
                                               await owner.SendResponseAsync(spaceInx, response, (int)response.Length);
                                           }).Start();
                                       }
                                       else
                                       {
                                           throw new Exception($"Unable to handle unknown request {key}");
                                       }
                                   }
                                   break;
                               case DataType.Response:
                                   {
                                       var spaceInx = await owner.ReceiveUInt();
                                       var data = await owner.ReceiveByteArray();
                                       new Task(() =>
                                       {
                                           owner.responseReceivers[spaceInx].OnResponse(data);
                                       }).Start();
                                   }
                                   break;
                           }
                       }
                   }
                   catch (Exception ex)
                   {
                       Debug.WriteLine(ex.Message);
                       //TODO: 더 나은 에러 체크
                   }
               });
            }

            private CancellationTokenSource readTaskCancellationToken = new CancellationTokenSource();

        }

        private EncryptHelper encryptHelper = new EncryptHelper();
        private NetworkStream networkStream;

        private SemaphoreSlim writeSemaphoreSlim = new SemaphoreSlim(1);

        private uint currentSpaceInx = 0;

        public long LastPongTime = -1;

        private Dictionary<string, RequestReceiver> requestReceivers = new Dictionary<string, RequestReceiver>();
        public void registerRequestReceiver(RequestReceiver receiver)
        {
            requestReceivers[receiver.Key] = receiver;
        }
        private Dictionary<uint, ResponseReceiver> responseReceivers = new Dictionary<uint, ResponseReceiver>();

        private ReadTask readTask;
        public void AttachNetworkStream(NetworkStream stream)
        {
            networkStream = stream;

            readTask?.Stop();
            readTask = new ReadTask(this);
            readTask.Run();
        }

        #region Send/Receive Helper Method
        private async Task Send(int i)
        {
            var lenByte = BitConverter.GetBytes(i);
            await networkStream.WriteAsync(lenByte, 0, lenByte.Length);
        }

        private async Task Send(uint i)
        {
            var lenByte = BitConverter.GetBytes(i);
            await networkStream.WriteAsync(lenByte, 0, lenByte.Length);
        }

        private async Task Send(long l)
        {
            var lenByte = BitConverter.GetBytes(l);
            await networkStream.WriteAsync(lenByte, 0, lenByte.Length);
        }

        private async Task Send(string str)
        {
            await Send(Encoding.UTF8.GetBytes(str));
        }

        private Task Send(byte[] data)
        {
            return Send(new MemoryStream(data), data.Length);
        }

        private async Task Send(Stream stream, int length)
        {
            await Send(length);
            byte[] buffer = new byte[32768];
            int read, left = 0;
            while (left > 0 &&
                   (read = await stream.ReadAsync(buffer, 0, Math.Min(buffer.Length, left))) > 0)
            {
                await networkStream.WriteAsync(buffer, 0, read);
                left -= read;
            }
        }

        private async Task Receive(byte[] buffer, int off, int len)
        {
            int left = len;
            while (left > 0)
            {
                var read = await networkStream.ReadAsync(buffer, off, left);
                if (read == 0) //Stream closed?
                {
                    if (!networkStream.CanRead)
                    {
                        throw new Exception("Can't read from NetworkStream, Closed Connection?");
                    }
                    continue;
                }
                off += read;
                left -= read;
            }
        }

        private async Task<byte[]> Receive(int len)
        {
            var buf = new byte[len];
            await Receive(buf, 0, len);
            return buf;
        }

        private async Task<int> ReceiveInt()
        {
            return BitConverter.ToInt32(await Receive(sizeof(int)), 0);
        }

        private async Task<uint> ReceiveUInt()
        {
            return BitConverter.ToUInt32(await Receive(sizeof(uint)), 0);
        }

        private async Task<byte[]> ReceiveByteArray()
        {
            return await Receive(await ReceiveInt());
        }

        private async Task<string> ReceiveString()
        {
            return Encoding.UTF8.GetString(await ReceiveByteArray());
        }

        #endregion

        public async Task SendPingAsync()
        {
            writeSemaphoreSlim.Wait();
            await networkStream.WriteAsync(PING_DATA, 0, PING_DATA.Length);
            writeSemaphoreSlim.Release();
        }

        public async Task SendPongAsync()
        {
            writeSemaphoreSlim.Wait();
            await networkStream.WriteAsync(PONG_DATA, 0, PONG_DATA.Length);
            writeSemaphoreSlim.Release();
        }

        public Task SendRequestAsync(string key, byte[] data, ResponseReceiver receiver)
        {
            return SendRequestAsync(key, new MemoryStream(data), data.Length, receiver);
        }

        public async Task SendRequestAsync(string key, Stream stream, int length, ResponseReceiver receiver)
        {
            writeSemaphoreSlim.Wait();
            var spaceInx = currentSpaceInx++;
            responseReceivers[spaceInx] = receiver;
            await networkStream.WriteAsync(REQUEST_HEAD, 0, REQUEST_HEAD.Length);
            await Send(spaceInx);
            await Send(key);
            await Send(stream, length);
            writeSemaphoreSlim.Release();
        }

        public Task SendResponseAsync(uint spaceInx, byte[] data)
        {
            return SendResponseAsync(spaceInx, new MemoryStream(data), data.Length);
        }

        public async Task SendResponseAsync(uint spaceInx, Stream stream, int length)
        {
            writeSemaphoreSlim.Wait();
            await networkStream.WriteAsync(RESPONSE_HEAD, 0, REQUEST_HEAD.Length);
            await Send(spaceInx);
            await Send(stream, length);
            writeSemaphoreSlim.Release();
        }
    }
}
