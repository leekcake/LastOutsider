using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LastOutsiderShared.Connection
{
    /// <summary>
    /// 이 클래스는 명령에서 즉석에서 생성후 사용하는것을 전제로 만들어짐
    /// 
    /// 데이터를 저장한뒤 한꺼번에 보냄
    /// 자료의 암호화등을 할때 한꺼번에 압축할 수 있도록 도움
    /// </summary>
    public class PacketContainer
    {
        private const int BUFFER_CAPACITY = 1024;
        private EncryptHelper encryptHelper;

        private MemoryStream pending = new MemoryStream(BUFFER_CAPACITY);
        private MemoryStream header = new MemoryStream(9);

        public PacketContainer(GameSocket.DataType type, EncryptHelper helper)
        {
            header.Write(GameSocket.HEADER, 0, GameSocket.HEADER.Length);
            header.Write(new byte[] { (byte)type, helper.UseAES ? (byte) 1 : (byte) 0, 0}, 0, 3);
            encryptHelper = helper;
        }
        
        public async Task WriteAsync(int i)
        {
            await pending.WriteAsync(i);
        }

        public async Task WriteAsync(uint i)
        {
            await pending.WriteAsync(i);
        }

        public async Task WriteAsync(long l)
        {
            await pending.WriteAsync(l);
        }

        public async Task WriteAsync(string str)
        {
            await pending.WriteAsync(str);
        }

        public async Task WriteAsync(byte[] data)
        {
            await pending.WriteByteArrayAsync(data);
        }

        public async Task WriteAsync(Stream stream, int length)
        {
            await pending.WriteAsync(stream, length);
        }

        public async Task Flush(NetworkStream into)
        {
            byte[] result;
            int resultLen;

            //Need Compress
            if( pending.Length > (1024 * 32) )
            {
                result = CompressPending();
                resultLen = result.Length;
            }
            else
            {
                result = pending.GetBuffer();
                resultLen = (int) pending.Length;
            }

            //Need Encrypt
            if( encryptHelper.UseAES )
            {
                header.GetBuffer()[5] = 1; //Encrypt Flag
                result = encryptHelper.EncryptAES(result);
                resultLen = result.Length;
            }

            header.Position = 5;
            await header.WriteAsync(resultLen);

            await into.WriteAsync(header.GetBuffer(), 0, 9);
            await into.WriteAsync(result, 0, resultLen);
            pending = new MemoryStream(BUFFER_CAPACITY);
        }

        private byte[] CompressPending()
        {
            header.GetBuffer()[4] = 1; //COMPRESSED flag
            using (var result = new MemoryStream())
            {
                using (var compressionStream = new GZipStream(result,
                    CompressionMode.Compress))
                {
                    compressionStream.Write(pending.GetBuffer(), 0, (int) pending.Length);
                    compressionStream.Flush();
                }
                return result.ToArray();
            }
        }
    }
}
