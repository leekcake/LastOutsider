using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LastOutsiderClientNetwork.Packet
{
    public delegate Task AsyncEventHandler(object sender, EventArgs e);

    public class FinishListener
    {
        public Action OnFinish { get; private set; }

        public Action<string> OnError { get; private set; }

        public Action onFinish;
        public Action<string> onError;

        private bool IsEnd = false;

        private void GenerateProxy()
        {
            OnFinish = new Action(() =>
            {
                IsEnd = true;
                onFinish?.Invoke();
            });

            OnError = new Action<string>((message) =>
            {
                IsEnd = true;
                onError?.Invoke(message);
            });
        }

        public FinishListener(Action<string> onError)
        {
            this.onError = onError;
            GenerateProxy();
        }

        public FinishListener(Action onFinish, Action<string> onError)
        {
            this.onFinish = onFinish;
            this.onError = onError;
            GenerateProxy();
        }

        public async Task WaitAsync()
        {
            while (IsEnd) {
                await Task.Delay(1);
            }
        }
    }

    public class FinishListener<T>
    {
        public Action<T> OnFinish { get; private set; }

        public Action<string> OnError { get; private set; }

        public Action<T> onFinish;
        public Action<string> onError;

        private bool IsEnd = false;
        private T ReceivedData = default;

        private void GenerateProxy()
        {
            OnFinish = new Action<T>((data) =>
            {
                IsEnd = true;
                onFinish?.Invoke(data);
            });

            OnError = new Action<string>((message) =>
            {
                IsEnd = true;
                onError?.Invoke(message);
            });
        }

        public FinishListener(Action<string> onError)
        {
            this.onError = onError;
            GenerateProxy();
        }

        public FinishListener(Action<T> onFinish, Action<string> onError)
        {
            this.onFinish = onFinish;
            this.onError = onError;
            GenerateProxy();
        }

        public async Task<T> WaitAsync()
        {
            while (IsEnd)
            {
                await Task.Delay(1);
            }
            return ReceivedData;
        }
    }
}
