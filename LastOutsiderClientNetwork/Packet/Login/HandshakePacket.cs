using LastOutsiderShared;
using LastOutsiderShared.Connection;
using LastOutsiderShared.Connection.Packet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LastOutsiderClientNetwork.Packet.Login
{
    public class HandshakePacket : BasePacket
    {
        private class HandshakeReceiver : ResponseReceiver
        {
            private GameSocket socket;
            private Action finishAction;

            public HandshakeReceiver(GameSocket socket, Action finishAction)
            {
                this.socket = socket;
                this.finishAction = finishAction;
            }

            public async void OnResponse(byte[] response)
            {
                try
                {
                    var stream = new MemoryStream(response);
                    var data = await stream.ReceiveString();

                    if (data == "OK")
                    {
                        socket.encryptHelper.UseAES = true;
                        finishAction?.Invoke();
                    }
                    else
                    {
                        socket.encryptHelper.RSAPublicKey = data;
                        socket.encryptHelper.GenerateRandomAESKey();

                        var result = new MemoryStream();
                        await result.WriteAsync("responseAES");
                        await result.WriteByteArrayAsync(socket.encryptHelper.EncryptRSA(socket.encryptHelper.AESKey));

                        await socket.SendRequestAsync("handshake", result.ToArray(), this);
                    }
                }
                catch (Exception ex)
                {
                    ExceptionUtils.OnException(ex);
                }
            }
        }

        public override string Key => "handshake";

        public async Task SendPacketAsync(GameSocket socket, Action finishAction)
        {
            await socket.SendRequestAsync(Key, "requestRSA", new HandshakeReceiver(socket, finishAction));
        }
    }
}
