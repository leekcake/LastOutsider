using LastOutsiderClientNetwork.Packet;
using LastOutsiderShared.Connection;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 연결 상태를 표시하는데 사용하는 캔버스
/// </summary>
public partial class ConnectCanvas : MonoBehaviour
{
    #region Singleton
    private static ConnectCanvas instance;
    public static ConnectCanvas Instance {
        get {
            return instance;
        }
    }
    #endregion

    private ConcurrentStack<Action> invokeActions;

    #region BaseConnectInformation
    public abstract class BaseConnectInformation
    {
        public ConnectCanvas Owner;
        public string Message;
        public bool Verbose;

        public int MaxTry;

        public int CurrentTry = 0;
        public string LastErrorMessage = "";

        private BaseConnectInformation AfterRun;

        public bool IsStarted {
            get; private set;
        }

        /// <summary>
        /// 이 연결 정보가 현재 작업중인지의 여부
        /// </summary>
        public bool InOperation {
            get; private set;
        }

        /// <summary>
        /// 이 연결 정보가 성공적으로 마무리 되었는지의 여부
        /// </summary>
        public bool IsFinished {
            get; private set;
        } = false;

        /// <summary>
        /// 이 연결 정보가 파괴되었는지의 여부
        /// </summary>
        public bool IsDestroyed {
            get; private set;
        } = false;

        public BaseConnectInformation(ConnectCanvas owner, string message, int maxTry = 3, bool verbose = false)
        {
            Owner = owner;
            Message = message;
            MaxTry = maxTry;
            Verbose = verbose;
            if( Application.platform == RuntimePlatform.WindowsEditor )
            {
                Verbose = true;
            }
        }

        /// <summary>
        /// 이 연결을 지금 시작합니다
        /// </summary>
        /// <returns></returns>
        public BaseConnectInformation Start()
        {
            if (IsStarted) return this;
            IsStarted = true;
            InOperation = true;
            CallMakeConnection();
            InvokeManager.Instance.Invoke(() =>
            {
                Owner.gameObject.SetActive(true);
            });
            return this;
        }

        /// <summary>
        /// 이 연결을 인자로 받은 연결이 성공적으로 완료되면 시작합니다.
        /// 오류가 발생하는경우 시작되지 않습니다
        /// </summary>
        /// <param name="connectInformation">이 연결이 성공하면 실행할 연결</param>
        /// <returns>인자로 받은 연결</returns>
        public T StartAfter<T>(T connectInformation)
            where T : BaseConnectInformation
        {
            if(connectInformation.IsStarted)
            {
                Debug.LogError("Given ConnectInformation is already started, may need to disable auotStart");
                return default(T);
            }

            //이 연결정보가 벌써 임무를 마친경우 - 좀 늦게 StartAfter를 호출한경우
            if(IsFinished)
            {
                //시작 명령을 토스
                connectInformation.Start();
                return connectInformation;
            }
            AfterRun = connectInformation;           
            return connectInformation;
        }

        protected abstract void CallMakeConnection();

        protected void OnFinish()
        {
            IsFinished = true;
            InOperation = false;
            AfterRun?.Start();
            Destroy();
        }

        public void StopRetry()
        {
            CurrentTry = MaxTry - 1;
        }

        private void Destroy()
        {
            lock (Owner.RegisteredConnect)
            {
                Owner.RegisteredConnect.Remove(this);
            }
            IsDestroyed = true;
        }

        protected void OnError(string message)
        {
            LastErrorMessage = message;
            //StopRetry
            if (CurrentTry + 1 == MaxTry)
            {
                Destroy();
                return;
            }
            CurrentTry++;

            InOperation = false;
            new Task(async () =>
            {
                await Task.Delay(Mathf.Min(10000, CurrentTry * 1000));
                InOperation = true;
                CallMakeConnection();
            }).Start();
        }

        public bool IsInSlient {
            get {
                return !Verbose && CurrentTry == 0;
            }
        }

        public string DisplayText {
            get {
                if(IsInSlient)
                {
                    return "";
                }

                if(CurrentTry == 0)
                {
                    return Message;
                }

                if(InOperation)
                {
                    return $"{Message}\r\n오류 메시지: {LastErrorMessage}\r\n{CurrentTry}회 재시도중";
                }

                return $"{Message}\r\n오류 메시지: {LastErrorMessage}\r\n{CurrentTry}회 재시도중(다음 재시도를 대기)";
            }
        }
    }
    #endregion

    public class ConnectInformation : BaseConnectInformation
    {
        private FinishListener finishListener;
        private Action<FinishListener> MakeConnection;

        private FinishListener mirrorListener;

        public ConnectInformation(ConnectCanvas owner, Action<FinishListener> makeConnection, FinishListener listener, string message, int maxTry = 3, bool verbose = false) : base(owner, message, maxTry, verbose)
        {
            MakeConnection = makeConnection;
            finishListener = listener;

            mirrorListener = new FinishListener(() =>
            {
                finishListener?.OnFinish();
                OnFinish();
            }, (errorMessage) =>
            {
                finishListener?.OnError(errorMessage);
                OnError(errorMessage);
            });
        }

        public async Task WaitAsync()
        {
            while ( !IsDestroyed )
            {
                await Task.Delay(3);
            }
        }

        protected override void CallMakeConnection()
        {
            MakeConnection(mirrorListener);
        }
    }

    public class ConnectInformation<Data> : BaseConnectInformation
    {
        private FinishListener<Data> finishListener;
        private Action<FinishListener<Data>> MakeConnection;
        private FinishListener<Data> mirrorListener;

        private Data Result = default;

        public ConnectInformation(ConnectCanvas owner, Action<FinishListener<Data>> makeConnection, FinishListener<Data> listener, string message, int maxTry = 3, bool verbose = false) : base(owner, message, maxTry, verbose)
        {
            MakeConnection = makeConnection;
            finishListener = listener;

            mirrorListener = new FinishListener<Data>((data) =>
            {
                Result = data;
                finishListener?.OnFinish(data);
                OnFinish();
            }, (errorMessage) =>
            {
                finishListener?.OnError(errorMessage);
                OnError(errorMessage);
            });
        }

        public async Task<Data> WaitAsync()
        {
            while (!IsDestroyed)
            {
                await Task.Delay(3);
            }
            return Result;
        }

        protected override void CallMakeConnection()
        {
            MakeConnection(mirrorListener);
        }
    }

    private readonly List<BaseConnectInformation> RegisteredConnect = new List<BaseConnectInformation>();

    public ConnectInformation CreateConnectInformation(string message, Action<FinishListener> makeConnection, FinishListener listener, int maxTry = 3, bool verbose = false, bool autoStart = true)
    {
        var connect = new ConnectInformation(this, makeConnection, listener, message, maxTry, verbose);
        lock (RegisteredConnect)
        {
            RegisteredConnect.Add(connect);
        }
        if (autoStart)
        {
            connect.Start();
        }

        return connect;
    }

    public ConnectInformation<T> CreateConnectInformation<T>(string message, Action<FinishListener<T>> makeConnection, FinishListener<T> listener, int maxTry = 3, bool verbose = false, bool autoStart = true)
    {
        var connect = new ConnectInformation<T>(this, makeConnection, listener, message, maxTry, verbose);
        lock (RegisteredConnect)
        {
            RegisteredConnect.Add(connect);
        }
        if(autoStart)
        {
            connect.Start();
        }

        return connect;
    }

    public GameObject BG;
    public Text ProgressText;

    void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
        gameObject.SetActive(false); //Re-enabled in ConnectInformation.Start
    }

    private void Update()
    {
        BaseConnectInformation connectInformation;

        lock(RegisteredConnect)
        {
            if (RegisteredConnect.Count == 0)
            {
                gameObject.SetActive(false);
                return;
            }
            connectInformation = RegisteredConnect[0];
        }

        if(connectInformation.IsInSlient)
        {
            BG.SetActive(false);
            ProgressText.text = "";
        }
        else
        {
            BG.SetActive(true);
            ProgressText.text = connectInformation.DisplayText;
        }
    }
}
