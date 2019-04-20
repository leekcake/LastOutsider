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
        private class HandshakeReceiver : ClientResponseReceiver
        {
            public HandshakeReceiver(GameSocket socket, FinishListener finishAction) : base(socket, finishAction)
            {
            }

            public override async void OnResponse(byte[] response)
            {
                try
                {
                    var stream = new MemoryStream(response);
                    var data = await stream.ReceiveString();

                    if (data == "OK")
                    {
                        socket.encryptHelper.UseAES = true;
                        finishListener?.OnFinish?.Invoke();
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
                    finishListener?.OnError?.Invoke(ex.Message);
                }
            }

            public override void OnResponseError(string message)
            {
                finishListener?.OnError?.Invoke(message);
            }
        }

        public override string Key => "handshake";

        public async Task SendPacketAsync(GameSocket socket, FinishListener finishAction)
        {
            await socket.SendRequestAsync(Key, "requestRSA", new HandshakeReceiver(socket, finishAction));
        }
    }
}
