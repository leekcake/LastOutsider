using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
public class InvokeManager : MonoBehaviour
{
    public static InvokeManager Instance {
        get; private set;
    }

    private ConcurrentStack<Action> actions = new ConcurrentStack<Action>();

    public void Invoke(Action action)
    {
        actions.Push(action);
    }

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        while (!actions.IsEmpty)
        {
            Action action;
            if (actions.TryPop(out action))
            {
                action();
            }
        }

        if( Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.G) && Application.isEditor )
        {
            try
            {
                File.Delete(Consts.ACCOUNT_FILE);
            }
            catch { }
            UnityEditor.EditorApplication.isPlaying = false;
        }
    }
}
