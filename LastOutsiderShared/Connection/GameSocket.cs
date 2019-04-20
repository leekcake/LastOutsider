using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.IO.Compression;

namespace LastOutsiderShared.Connection
{
    /*
    GameSocket {
	    DATA HEADER - 2 BYTE (0x39, 0xbe)
	    DATA TYPE - 1 BYTE
	    if(DATA TYPE != PING) {
            ENCRYPTED? - 1 BYTE
            COMPRESSED? - 1 BYTE
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
        public static readonly byte[] HEADER = new byte[]
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

            private byte[] Decompress(byte[] input)
            {
                using (var source = new MemoryStream(input))
                {
                    byte[] lengthBytes = new byte[4];
                    source.Read(lengthBytes, 0, 4);

                    var length = BitConverter.ToInt32(lengthBytes, 0);
                    using (var decompressionStream = new GZipStream(source,
                        CompressionMode.Decompress))
                    {
                        var result = new byte[length];
                        decompressionStream.Read(result, 0, length);
                        return result;
                    }
                }
            }

            public void Run()
            {
                Task.Factory.StartNew(async () =>
               {
                   try
                   {
                       byte[] headerBuffer = new byte[2];
                       START:
                       while (true)
                       {
                           Stream stream = owner.networkStream;

                           var header = await stream.Receive(HEADER.Length);
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

                           var type = await stream.Receive(1);

                           switch ((DataType)type[0])
                           {
                               case DataType.Ping:
                                   owner.SendPongAsync();
                                   goto START;
                               case DataType.Pong:
                                   owner.LastPongTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                                   //소켓이 아직 살아있음!
                                   goto START;
                           }

                           var encrypted = (await stream.Receive(1))[0] == 1;
                           var compressed = (await stream.Receive(1))[0] == 1;

                           if(encrypted || compressed)
                           {
                               byte[] data = await stream.ReceiveByteArray();
                               if(compressed)
                               {
                                   data = Decompress(data);
                               }

                               if(encrypted)
                               {
                                   data = owner.encryptHelper.DecryptAES(data, 0, data.Length);
                               }

                               stream = new MemoryStream(data);
                           }

                           switch((DataType)type[1])
                           {
                               case DataType.Request:
                                   {
                                       var spaceInx = await stream.ReceiveUInt();
                                       var key = await stream.ReceiveString();
                                       var data = await stream.ReceiveByteArray();
                                       if (owner.requestReceivers.ContainsKey(key))
                                       {
                                           new Task(async () =>
                                           {
                                               var receiver = owner.requestReceivers[key];
                                               var response = await receiver.OnRequest(data);
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
                                       var spaceInx = await owner.networkStream.ReceiveUInt();
                                       var data = await owner.networkStream.ReceiveByteArray();
                                       new Task(() =>
                                       {
                                           owner.responseReceivers[spaceInx].OnResponse(data);
                                           owner.responseReceivers.Remove(spaceInx);
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

        public readonly EncryptHelper encryptHelper = new EncryptHelper();
        private NetworkStream networkStream;

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

        public async Task SendPingAsync()
        {
            await networkStream.WriteAsync(PING_DATA, 0, PING_DATA.Length);
        }

        public async Task SendPongAsync()
        {
            await networkStream.WriteAsync(PONG_DATA, 0, PONG_DATA.Length);
        }

        public Task SendRequestAsync(string key, byte[] data, ResponseReceiver receiver)
        {
            return SendRequestAsync(key, new MemoryStream(data), data.Length, receiver);
        }

        public async Task SendRequestAsync(string key, Stream stream, int length, ResponseReceiver receiver)
        {
            PacketContainer packetContainer = new PacketContainer(DataType.Request, encryptHelper);

            var spaceInx = currentSpaceInx++;
            responseReceivers[spaceInx] = receiver;

            await packetContainer.WriteAsync(spaceInx);
            await packetContainer.WriteAsync(key);
            await packetContainer.WriteAsync(stream, length);

            await packetContainer.Flush(networkStream);
        }

        public Task SendResponseAsync(uint spaceInx, byte[] data)
        {
            return SendResponseAsync(spaceInx, new MemoryStream(data), data.Length);
        }

        public async Task SendResponseAsync(uint spaceInx, Stream stream, int length)
        {
            PacketContainer packetContainer = new PacketContainer(DataType.Response, encryptHelper);
            await packetContainer.WriteAsync(spaceInx);
            await packetContainer.WriteAsync(stream, length);

            await packetContainer.Flush(networkStream);
        }
    }
}
