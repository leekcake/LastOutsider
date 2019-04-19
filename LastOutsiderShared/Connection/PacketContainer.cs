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
    /// 멀티 스레드 환경에서 사용될 수 있는 GameSocket의 스레드 안정성을 보장하고
    /// 자료의 암호화등을 할때 한꺼번에 압축할 수 있도록 도움
    /// </summary>
    public class PacketContainer
    {
        private const int BUFFER_CAPACITY = 1024;
        private EncryptHelper encryptHelper;

        private MemoryStream pending = new MemoryStream(BUFFER_CAPACITY);

        public PacketContainer(GameSocket.DataType type, EncryptHelper helper)
        {
            pending.Write(GameSocket.HEADER, 0, GameSocket.HEADER.Length);
            pending.Write(new byte[] { (byte)type, helper.UseAES ? (byte) 1 : (byte) 0 }, 0, 2);
            encryptHelper = helper;
        }
        
        public async Task WriteAsync(int i)
        {
            var lenByte = BitConverter.GetBytes(i);
            await pending.WriteAsync(lenByte, 0, lenByte.Length);
        }

        public async Task WriteAsync(uint i)
        {
            var lenByte = BitConverter.GetBytes(i);
            await pending.WriteAsync(lenByte, 0, lenByte.Length);
        }

        public async Task WriteAsync(long l)
        {
            var lenByte = BitConverter.GetBytes(l);
            await pending.WriteAsync(lenByte, 0, lenByte.Length);
        }

        public async Task WriteAsync(string str)
        {
            await WriteAsync(Encoding.UTF8.GetBytes(str));
        }

        public Task WriteAsync(byte[] data)
        {
            return WriteAsync(new MemoryStream(data), data.Length);
        }

        public async Task WriteAsync(Stream stream, int length)
        {
            WriteAsync(length);
            byte[] buffer = new byte[32768];
            int read, left = length;
            while (left > 0 &&
                   (read = await stream.ReadAsync(buffer, 0, Math.Min(buffer.Length, left))) > 0)
            {
                await stream.WriteAsync(buffer, 0, read);
                left -= read;
            }
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
                result = encryptHelper.EncryptAES(result, 0, resultLen);
                resultLen = result.Length;
            }

            await into.WriteAsync(result, 0, resultLen);

            pending = new MemoryStream(BUFFER_CAPACITY);
        }

        private byte[] CompressPending()
        {
            using (var result = new MemoryStream())
            {
                var buf = pending.GetBuffer();
                buf[4] = 1; //COMPRESSED flag
                result.Write(buf, 0, 5);
                using (var compressionStream = new GZipStream(result,
                    CompressionMode.Compress))
                {
                    compressionStream.Write(buf, 5, buf.Length - 5);
                    compressionStream.Flush();
                }
                return result.ToArray();
            }
        }
    }
}
