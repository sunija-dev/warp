using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using System;
using CompressString;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class WarpNetworkManager : NetworkManager
{
    public static WarpNetworkManager Instance;

    public event EventHandler<NetworkConnection> ePlayerConnected;
    public event EventHandler<NetworkConnection> ePlayerDisconnected;

    private int iReconnectAttempts = 0;

    private void Awake()
    {
        Instance = this;
        base.Awake();
    }

    // SERVER
    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkServer.RegisterHandler<CreatePlayerMessage>(OnCreatePlayer);
    }

    // SERVER 
    public override void OnStopServer()
    {
        print("OnStopServer");
        base.OnStopServer();
    }

    public struct CreatePlayerMessage : NetworkMessage {}

    // CLIENT
    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        conn.Send(new CreatePlayerMessage { });
        print("CLIENT: CreatePlayer message was sent");
    }

    // SERVER
    void OnCreatePlayer(NetworkConnection connection, CreatePlayerMessage createPlayerMessage)
    {
        Debug.Log("SERVER: Create player.");
        GameObject goPlayer = Instantiate(playerPrefab);
        NetworkServer.AddPlayerForConnection(connection, goPlayer);
    }

    // CLIENT
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        print("OnClientDisconnect");
        base.OnClientDisconnect(conn);
        ClientManager.Instance.bAccLoggedIn = false;
        ClientManager.Instance.bCharLoggedIn = false;
        ClientManager.Instance.ShowLoadingPage();

        // call StopClient to clean everything up properly (otherwise NetworkClient.active remains false after next login)
        StopClient();
        StartCoroutine(coTryReconnect());
    }

    // CLIENT
    public IEnumerator coTryReconnect()
    {
        iReconnectAttempts++;
        if (iReconnectAttempts > 2)
        {
            CategoryLoading categoryLoading = ClientManager.Instance.categoryLoading;
            LocalizationUtility.LocalizeTextAsync(categoryLoading.m_textTitle, "warp_cannot_connect_title");
            LocalizationUtility.LocalizeTextAsync(categoryLoading.m_textDescription, "warp_cannot_connect", new string[] { iReconnectAttempts.ToString() });
        }

        Debug.Log("Trying to reconnect in 10 sec");
        yield return new WaitForSeconds(10f);
        StartClient();
        if (!ClientManager.Instance.bAccLoggedIn)
            StartCoroutine(coTryReconnect());
        else
            iReconnectAttempts = 0;
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        ePlayerConnected?.Invoke(this, conn);
        base.OnServerAddPlayer(conn);
    }

    // SERVER
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        ePlayerDisconnected?.Invoke(this, conn);
        base.OnServerDisconnect(conn);
    }

    public override void OnServerError(NetworkConnection conn, Exception exception)
    {
        base.OnServerError(conn, exception);

        string strAccountName = "";
        if (ServerManager.s_dictPlayers.ContainsKey(conn))
        {
            try { strAccountName = ServerManager.s_dictPlayers[conn].account.accountInfo.name; } // try/catch necessary?
            catch { strAccountName = "Could not be found."; }
        }
        else
        {
            strAccountName = conn.connectionId.ToString();
        }

        Debug.Log($"{strAccountName} - {exception.Source}: {exception.Message}\n{exception.StackTrace}");
    }
}
