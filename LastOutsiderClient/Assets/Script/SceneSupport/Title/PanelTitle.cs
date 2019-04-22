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
using System;
using UnityEngine.SceneManagement;

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
        var socket = await NetworkManager.Instance.GetGameSocketAsync(loginIfMissing: false);
        if (account == null)
        {
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
            await ConnectCanvas.Instance.CreateConnectInformation("로그인", (listener) =>
            {
                if (account == null)
                {
                    throw new Exception("계정이 없음");
                }
                socket.LoginAccountAsync(account.Id, account.AuthToken, listener);
            }, null)
            .StartAfter(ConnectCanvas.Instance.CreateConnectInformation("시작 데이터 가져오는중", (listener) =>
            {
                socket.FetchDataAsync(listener);
            }, new FinishListener<FetchData>((data) =>
            {
                DataManager.Instance.ReadFetchData(data);
            }, (message) => { })
            )).WaitAsync();

            //SceneManager.LoadSceneAsync("MainScene");
        }
        TouchProgress = false;
    }
}
