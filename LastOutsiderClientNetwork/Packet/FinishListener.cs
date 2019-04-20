using System;
using System.Collections.Generic;
using System.Text;

namespace LastOutsiderClientNetwork.Packet
{
    public class FinishListener
    {
        public Action OnFinish;
        public Action<string> OnError;

        public FinishListener(Action onFinish, Action<string> onError)
        {
            OnFinish = onFinish;
            OnError = onError;
        }
    }

    public class FinishListener<T>
    {
        public Action<T> OnFinish;
        public Action<string> OnError;

        public FinishListener(Action<T> onFinish, Action<string> onError)
        {
            OnFinish = onFinish;
            OnError = onError;
        }
    }
}
