using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MessagePack;
using LastOutsiderShared.Data;

public class PanelTitle : MonoBehaviour
{
    public GameObject beforeLogin, afterLogin;

    // Start is called before the first frame update
    void Start()
    {
        if (File.Exists(Consts.ACCOUNT_FILE))
        {
            afterLogin.SetActive(true);
        }
        else
        {
            beforeLogin.SetActive(true);
        }

        NetworkManager.Instance.GetGameSocketAsync(loginIfMissing: false);
    }

    public async void CallbackTouch()
    {
        if( !File.Exists(Consts.ACCOUNT_FILE) )
        {

            beforeLogin.SetActive(false);
            afterLogin.SetActive(true);
            return;
        }

        var account = MessagePackSerializer.Deserialize<Account>(File.ReadAllBytes(Consts.ACCOUNT_FILE));
        
        
    }
}
