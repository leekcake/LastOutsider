using LastOutsiderShared.Connection;
using System;
using System.Collections.Generic;
using System.Text;

namespace LastOutsiderClientNetwork.Packet
{
    public abstract class ClientResponseReceiver : ResponseReceiver
    {
        protected GameSocket socket;
        protected FinishListener finishListener;

        public ClientResponseReceiver(GameSocket socket, FinishListener finishAction)
        {
            this.socket = socket;
            this.finishListener = finishAction;
        }

        public abstract void OnResponse(byte[] response);
        public abstract void OnResponseError(string message);
    }
}
