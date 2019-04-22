using LastOutsiderClientNetwork.Packet;
using LastOutsiderClientNetwork.Packet.Extension.Login;
using LastOutsiderClientNetwork.Packet.Login;
using LastOutsiderShared;
using LastOutsiderShared.Connection;
using LastOutsiderShared.Data;
using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class NetworkManager
{
    private class Printer : PrintHelper
    {
        public void Printline(string line)
        {
            Debug.Log(line);
        }
    }

    #region Singleton
    private static NetworkManager instance;
    public static NetworkManager Instance {
        get {
            if (instance == null)
            {
                instance = new NetworkManager();
            }

            return instance;
        }
    }
    #endregion

    private static readonly IPAddress serverIP = IPAddress.Loopback;
    private const int serverPort = 8039;

    private TcpClient tcpClient = null;
    private GameSocket gameSocket = null;

    private bool SocketInCreate = false;
    
    /// <summary>
    /// 저장되어 있는 게임 소켓을 반환합니다
    /// </summary>
    /// <param name="createIfMissing">참일시 소켓이 없는경우 서버와 연결합니다</param>
    /// <returns></returns>
    public async Task<GameSocket> GetGameSocketAsync(bool createIfMissing = true, bool loginIfMissing = true)
    {
        while(SocketInCreate)
        {
            await Task.Delay(1);
        }

        if (gameSocket == null && createIfMissing)
        {
            SocketInCreate = true;
            tcpClient = new TcpClient();
            gameSocket = new GameSocket();
            gameSocket.printHelper = new Printer();
            var handshakeCI = ConnectCanvas.Instance.CreateConnectInformation("서버에 연결", async (listener) =>
            {
                try
                {
                    await tcpClient.ConnectAsync(serverIP, serverPort);
                    gameSocket.AttachNetworkStream(tcpClient.GetStream());
                    listener.OnFinish();
                }
                catch (Exception ex)
                {
                    listener.OnError(ex.Message);
                }
            }, null, -1)
            .StartAfter(ConnectCanvas.Instance.CreateConnectInformation("연결을 암호화", (receiver) =>
            {
                gameSocket.HandshakeAsync(receiver);
                if(!loginIfMissing)
                {
                    SocketInCreate = false;
                }
            }, null, autoStart: false));

            if (loginIfMissing)
            {
                var account = Consts.GetAccountFromFile();
                await handshakeCI.StartAfter(ConnectCanvas.Instance.CreateConnectInformation("로그인", (listener) =>
                {
                    if (account == null)
                    {
                        throw new Exception("계정이 없음");
                    }
                    gameSocket.LoginAccountAsync(account.Id, account.AuthToken, listener);
                }, null, autoStart: false))
                .StartAfter(ConnectCanvas.Instance.CreateConnectInformation("데이터를 다시 가져오는중", (listener) =>
                {
                    gameSocket.FetchDataAsync(listener);
                }, new FinishListener<FetchData>((data) =>
                {
                    DataManager.Instance.ReadFetchData(data);
                    SocketInCreate = false;
                }, (message) => { })
                )).WaitAsync();
            }
            else
            {
                await handshakeCI.WaitAsync();
            }
        }
        return gameSocket;
    }
}
