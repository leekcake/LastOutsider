using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MessagePack;
using LastOutsiderShared.Data;
using System.Threading.Tasks;
using LastOutsiderClientNetwork.Packet.Extension.Login;
using System.Security.Cryptography;
using LastOutsiderClientNetwork.Packet;
using LastOutsiderShared;

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

    private bool TouchProgress = false;

    private void Update()
    {
        if(Input.GetMouseButtonUp(0) && !ConnectCanvas.Instance.gameObject.activeSelf )
        {
            CallbackTouch();
        }
    }

    public async Task CallbackTouch()
    {
        if (TouchProgress) return; 
        TouchProgress = true;

        var account = Consts.GetAccountFromFile();
        if(account == null)
        {
            var socket = await NetworkManager.Instance.GetGameSocketAsync(loginIfMissing: false);
            var authToken = new byte[128];
            var rnd = new RNGCryptoServiceProvider();
            rnd.GetBytes(authToken);

            account = await ConnectCanvas.Instance.CreateConnectInformation<Account>("계정 생성", (listener) =>
            {
                socket.GenerateAccountAsync(authToken, listener);
            }, null).WaitAsync();
            if(account == null)
            {
                return;
            }
            await AsyncUtil.WriteBytesAsync(Consts.ACCOUNT_FILE, MessagePackSerializer.Serialize(account));
            beforeLogin.SetActive(false);
            afterLogin.SetActive(true);
        }
        else
        {
            //TODO: Login
        }
    }
}
