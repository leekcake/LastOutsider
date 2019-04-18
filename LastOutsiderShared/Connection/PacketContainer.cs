using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LastOutsiderShared.Connection
{
    /// <summary>
    /// 백업본
    /// 나중에 전송 명령어가 다른 전송을 기다릴 여유도 없을 정도가 되었을때 사용
    /// </summary>
    public class PacketContainer
    {
        private MemoryStream pending = new MemoryStream(1024);

        public void Send(int i)
        {
            var lenByte = BitConverter.GetBytes(i);
            pending.Write(lenByte, 0, lenByte.Length);
        }

        public void Send(uint i)
        {
            var lenByte = BitConverter.GetBytes(i);
            pending.Write(lenByte, 0, lenByte.Length);
        }

        public void Send(long l)
        {
            var lenByte = BitConverter.GetBytes(l);
            pending.Write(lenByte, 0, lenByte.Length);
        }

        public async Task Send(string str)
        {
            await Send(Encoding.UTF8.GetBytes(str));
        }

        public Task Send(byte[] data)
        {
            return Send(new MemoryStream(data), data.Length);
        }

        public async Task Send(Stream stream, int length)
        {
            Send(length);
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
            pending.Position = 0;
            byte[] buffer = new byte[32768];
            int read, left = (int) pending.Length;
            while (left > 0 &&
                   (read = await pending.ReadAsync(buffer, 0, Math.Min(buffer.Length, left))) > 0)
            {
                await into.WriteAsync(buffer, 0, read);
                left -= read;
            }
        }
    }
}
