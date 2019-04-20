using LastOutsiderShared.Connection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LastOutsiderServer.Receiver.Login
{
    public class HandshakeReceiver : RequestReceiver
    {
        public GameSocket socket;
        public HandshakeReceiver(GameSocket socket)
        {
            this.socket = socket;
        }

        public string Key => "handshake";

        public async Task<Stream> OnRequest(byte[] requestData)
        {
            var result = new MemoryStream();

            var stream = new MemoryStream(requestData);
            var req = await stream.ReceiveString();

            if (req == "requestRSA")
            {
                socket.encryptHelper.GenerateNewRSA();

                await result.WriteAsync(socket.encryptHelper.RSAPublicKey);
            }
            else if (req == "responseAES")
            {
                socket.encryptHelper.AESKey = socket.encryptHelper.DecryptRSA(await stream.ReceiveByteArray());
                socket.encryptHelper.UseAES = true;
                await result.WriteAsync("OK");
            }
            result.Position = 0;
            return result;
        }
    }
}
