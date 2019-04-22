using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LastOutsiderShared
{
    public static class AsyncUtil
    {
        public static async Task<byte[]> ReadBytesAsync(string path)
        {
            using(FileStream stream = new FileStream(path, FileMode.Open))
            {
                int read, off = 0, left = (int)stream.Length;
                var result = new byte[left];
                while (left > 0 &&
                       (read = await stream.ReadAsync(result, off, left)) > 0)
                {
                    off += read;
                    left -= read;
                }
                return result;
            }
        }

        public static async Task WriteBytesAsync(string path, byte[] data)
        {
            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                await stream.WriteAsync(data, 0, data.Length);
            }
        }
    }
}
