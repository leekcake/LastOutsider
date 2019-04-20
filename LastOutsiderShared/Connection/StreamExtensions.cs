using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LastOutsiderShared.Connection
{
    public static class StreamExtensions
    {
        public static async Task WriteAsync(this Stream stream, int i)
        {
            var lenByte = BitConverter.GetBytes(i);
            await stream.WriteAsync(lenByte, 0, lenByte.Length);
        }

        public static async Task WriteAsync(this Stream stream, uint i)
        {
            var lenByte = BitConverter.GetBytes(i);
            await stream.WriteAsync(lenByte, 0, lenByte.Length);
        }

        public static async Task WriteAsync(this Stream stream, long l)
        {
            var lenByte = BitConverter.GetBytes(l);
            await stream.WriteAsync(lenByte, 0, lenByte.Length);
        }

        public static async Task WriteAsync(this Stream stream, string str)
        {
            await WriteAsync(stream, Encoding.UTF8.GetBytes(str));
        }

        public static async Task WriteAsync(this Stream stream, byte[] data)
        {
            await WriteAsync(stream, new MemoryStream(data), data.Length);
        }

        public static async Task WriteAsync(this Stream dest, Stream source, int length)
        {
            await WriteAsync(dest, length);
            byte[] buffer = new byte[32768];
            int read, left = length;
            while (left > 0 &&
                   (read = await source.ReadAsync(buffer, 0, Math.Min(buffer.Length, left))) > 0)
            {
                await dest.WriteAsync(buffer, 0, read);
                left -= read;
            }
        }

        public static async Task Receive(this Stream stream, byte[] buffer, int off, int len)
        {
            int left = len;
            while (left > 0)
            {
                var read = await stream.ReadAsync(buffer, off, left);
                if (read == 0) //Stream closed?
                {
                    if (!stream.CanRead)
                    {
                        throw new Exception("Can't read from NetworkStream, Closed Connection?");
                    }
                    continue;
                }
                off += read;
                left -= read;
            }
        }

        public static async Task<byte[]> Receive(this Stream stream, int len)
        {
            var buf = new byte[len];
            await Receive(stream, buf, 0, len);
            return buf;
        }

        public static async Task<int> ReceiveInt(this Stream stream)
        {
            return BitConverter.ToInt32(await Receive(stream, sizeof(int)), 0);
        }

        public static async Task<uint> ReceiveUInt(this Stream stream)
        {
            return BitConverter.ToUInt32(await Receive(stream, sizeof(uint)), 0);
        }

        public static async Task<byte[]> ReceiveByteArray(this Stream stream)
        {
            return await Receive(stream, await ReceiveInt(stream));
        }

        public static async Task<string> ReceiveString(this Stream stream)
        {
            return Encoding.UTF8.GetString(await ReceiveByteArray(stream));
        }
    }
}
