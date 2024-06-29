using ChickenScratch;
using Mirror;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ChickenScratch.ColourManager;

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
        currentState = NetworkState.disconnected;
        base.OnStopHost();
    }

    public override void OnClientConnect()
    {
        switch(currentState)
        {
            case NetworkState.lobby:
                break;
            case NetworkState.ingame:
                break;
        }
        
        base.OnClientConnect();
    }


    public override void OnClientDisconnect()
    {
        Debug.LogError("Client disconnect.");
        base.OnClientDisconnect();
        switch(currentState)
        {
            case NetworkState.lobby:
                break;
            case NetworkState.ingame:
                if (dcManager == null)
                {
                    dcManager = FindObjectOfType<DCManager>();
                }
                dcManager.handleHostDisconnection();
                return;
                currentState = NetworkState.disconnected;
                SettingsManager.Instance.currentSceneTransitionState = SettingsManager.SceneTransitionState.return_to_room_listings;
                SettingsManager.Instance.playerQuit = true;
                Cursor.visible = true;
                SceneManager.LoadScene(0);
                break;
        }
        

    }

    public override void OnStopClient()
    {
        switch(currentState)
        {
            case NetworkState.ingame:
                return;
        }
        //intentionalDisconnection = true;
        SettingsManager.Instance.DisconnectFromLobby();
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
        switch(currentState)
        {
            case NetworkState.lobby:
                SettingsManager.Instance.AddConnection(conn);
                if(conn.connectionId == NetworkConnection.LocalConnectionId)
                {
                    SettingsManager.Instance.SetConnectionSteamName(Steamworks.SteamFriends.GetPersonaName(), conn);
                }
                MenuLobbyButtons.Instance.UpdatePlayerCount();
                break;
        }
        
        base.OnServerConnect(conn);
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        switch(currentState)
        {
            case NetworkState.lobby:
                

                if(SettingsManager.Instance.waitingForPlayers)
                {
                    List<BirdName> allPlayers = SettingsManager.Instance.GetAllActiveBirds();
                    foreach (BirdName player in allPlayers)
                    {
                        
                        SettingsManager.Instance.SetBirdForPlayerID(SettingsManager.Instance.GetConnection(player), player);
                    }

                    LobbyNetwork.Instance.lobbyDataHandler.TargetReturnToLobby(conn);
                    LobbyNetwork.Instance.loadedPlayers++;
                    if(LobbyNetwork.Instance.loadedPlayers == NetworkServer.connections.Count)
                    {
                        //Load the lobby page
                        LobbyNetwork.Instance.lobbyDataHandler.RpcCloseLobbyLoadingPage();

                        SettingsManager.Instance.ServerRefreshBirds();
                        SettingsManager.Instance.waitingForPlayers = false;
                    }
                    
                }
                else
                {
                    SettingsManager.Instance.BroadcastBirdAssignmentInLobby();
                    LobbyNetwork.Instance.lobbyDataHandler.TargetOpenLobby(conn);
                }
                LobbyNetwork.Instance.lobbyDataHandler.TargetRequestSteamID(conn);
                LobbyNetwork.Instance.lobbyDataHandler.TargetSetPlayerID(conn, conn.connectionId.ToString());
                break;
        }

    }

    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        switch(currentState)
        {
            case NetworkState.lobby:
                
                break;
            case NetworkState.ingame:
                if (!GameManager.Instance.gameFlowManager.connectedPlayers.Contains(conn))
                {
                    GameManager.Instance.gameFlowManager.connectedPlayers.Add(conn);
                }
                break;
        }

        
        base.OnServerReady(conn);
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        
        switch (currentState)
        {
            case NetworkState.lobby:
                SettingsManager.Instance.RemoveConnection(conn);
                MenuLobbyButtons.Instance.UpdatePlayerCount();
                SettingsManager.Instance.ServerRefreshBirds();
                break;
            case NetworkState.ingame:
                if (dcManager == null)
                {
                    dcManager = FindObjectOfType<DCManager>();
                }
                if (dcManager)
                {
                    BirdName disconnectedPlayer = SettingsManager.Instance.GetDisconnectedPlayerBird(conn);
                    if(disconnectedPlayer != BirdName.none)
                    {
                        GameManager.Instance.gameDataHandler.RpcSendDisconnectionNotification(disconnectedPlayer);
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
                Debug.LogError("Changing scene and setting currentState for network manager.");
                SettingsManager.Instance.isHostInLobby = false;
                currentState = NetworkState.ingame;
                break;
        }
        
        base.ServerChangeScene(newSceneName);
    }

    
}
