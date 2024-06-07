using ChickenScratch;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CSNetworkManager : NetworkManager
{
    public enum NetworkState
    {
        disconnected, lobby, ingame, invalid
    }
    public NetworkState currentState = NetworkState.disconnected;
    
    public static bool intentionalDisconnection = true;

    private DCManager dcManager;

    public override void Awake()
    {
        base.Awake();
    }

    public override void OnStartHost()
    {
        Debug.Log("OnStartHost was called.");
        if (SceneManager.GetActiveScene().name == "Game")
        {
            currentState = NetworkState.ingame;
        }
        else
        {
            currentState = NetworkState.lobby;
        }
        
        base.OnStartHost();
    }

    public override void OnStartClient()
    {
        Debug.Log("OnStartClient was called.");
        if (SceneManager.GetActiveScene().name == "Game")
        {
            currentState = NetworkState.ingame;
        }
        else
        {
            currentState = NetworkState.lobby;
        }
        intentionalDisconnection = false;
        base.OnStartClient();
    }

    public override void OnStopHost()
    {
        Debug.Log("OnStopHost was called.");
        currentState = NetworkState.disconnected;
        base.OnStopHost();
    }

    public override void OnClientConnect()
    {
        Debug.Log("OnClientConnect was called.");
        switch(currentState)
        {
            case NetworkState.lobby:
                LobbyNetwork.Instance.OnJoinedLobby();
                break;
            case NetworkState.ingame:
                break;
        }
        
        base.OnClientConnect();
    }

    public override void OnClientDisconnect()
    {
        Debug.LogError("OnClientDisconnect was called. DCManager?[" + (dcManager == null).ToString() + "]");
        base.OnClientDisconnect();
        currentState = NetworkState.disconnected;
    }

    public override void OnStopClient()
    {
        //intentionalDisconnection = true;
        Debug.LogError("OnStopClient was called.");
        
        if (dcManager == null)
        {
            dcManager = FindObjectOfType<DCManager>();
        }
        if (dcManager)
        {
            Debug.LogError("Intentional disconnection?:[" + intentionalDisconnection.ToString() + "]");
            if (!intentionalDisconnection)
            {
                dcManager.OnDisconnected();
            }
            else
            {
                dcManager.OnLeftRoom();
            }
            intentionalDisconnection = false;
        }
        base.OnStopClient();
    }



    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        Debug.Log("OnServerConnect was called.");
        switch(currentState)
        {
            case NetworkState.lobby:
                MenuLobbyButtons.Instance.UpdatePlayerCount();
                break;
        }
        
        base.OnServerConnect(conn);
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        Debug.Log("OnServerAddPlayer was called.");
        base.OnServerAddPlayer(conn);
        switch(currentState)
        {
            case NetworkState.lobby:
                LobbyNetwork.Instance.lobbyDataHandler.TargetSetPlayerID(conn, conn.connectionId.ToString());
                LobbyNetwork.Instance.ServerUpdatePlayerListings();
                break;
        }

    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        Debug.Log("OnServerDisconnect was called.");
        
        switch (currentState)
        {
            case NetworkState.lobby:
                MenuLobbyButtons.Instance.UpdatePlayerCount();
                LobbyNetwork.Instance.ServerUpdatePlayerListings();
                break;
            case NetworkState.ingame:
                if (dcManager == null)
                {
                    dcManager = FindObjectOfType<DCManager>();
                }
                if (dcManager)
                {
                    if(SettingsManager.Instance.birdConnectionMap.Any(bc => bc.Value == conn))
                    {
                        KeyValuePair<ColourManager.BirdName, NetworkConnectionToClient> playerConnection = SettingsManager.Instance.birdConnectionMap.Single(bc => bc.Value == conn);
                        GameManager.Instance.gameDataHandler.RpcSendDisconnectionNotification(playerConnection.Key);
                    }
                }
                break;
        }
        
        base.OnServerDisconnect(conn);
        
    }

    public override void ServerChangeScene(string newSceneName)
    {
        switch(newSceneName)
        {
            case "MainMenu":
                currentState = NetworkState.lobby;
                break;
            case "Game":
                SettingsManager.Instance.isHostInLobby = false;
                currentState = NetworkState.ingame;
                break;
        }
        
        base.ServerChangeScene(newSceneName);
    }

    
}
