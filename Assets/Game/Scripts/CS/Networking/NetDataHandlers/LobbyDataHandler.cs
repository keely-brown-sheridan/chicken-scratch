using ChickenScratch;
using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ChickenScratch.ColourManager;

public class LobbyDataHandler : NetworkBehaviour
{
    [ClientRpc]
    public void RpcSetPlayerListings(string hostID, List<PlayerListingNetData> playerListingData)
    {
        SettingsManager.Instance.hostID = hostID;
        LobbyNetwork.Instance.UpdateClientPlayerListings(playerListingData);
        if(SettingsManager.Instance.isHost)
        {
            MenuLobbyButtons.Instance.lobbyNotReadyManager.playerAllHaveCardsSelected = SettingsManager.Instance.GetPlayerNameCount() == NetworkServer.connections.Count();
        }
    }


    [Command(requiresAuthority = false)]
    public void CmdSelectPlayerBird(string playerName, ColourManager.BirdName selectedBird, NetworkConnectionToClient sender = null)
    {
        if(MenuLobbyButtons.Instance.IsBirdSelected(selectedBird))
        {
            return;
        }
        //If the player already has a bird selected then broadcast to have it deselected
        ColourManager.BirdName previousBird = MenuLobbyButtons.Instance.GetPreviouslySelectedBird(playerName);
        if (previousBird == selectedBird)
        {
            return;
        }
        else if(previousBird != ColourManager.BirdName.none)
        {
            RpcDeselectBirdBird(previousBird);
        }

        SettingsManager.Instance.SetBirdForPlayerID(sender, selectedBird);
        SettingsManager.Instance.ServerRefreshBirds();
        
    }

    [Command(requiresAuthority = false)]
    public void CmdSetSteamID(string steamID, NetworkConnectionToClient sender = null)
    {
        SettingsManager.Instance.SetConnectionSteamName(steamID, sender);
    }

    [ClientRpc]
    public void RpcSelectPlayerBird(string playerName, ColourManager.BirdName selectedBird)
    {
        
        
    }

    [ClientRpc]
    public void RpcSetPlayerBird(string playerID, BirdName birdName )
    {
        SettingsManager.Instance.AssignBirdToPlayer(birdName, playerID);
    }

    [ClientRpc]
    public void RpcDeselectBirdBird(ColourManager.BirdName birdName)
    {
        MenuLobbyButtons.Instance.DeselectPlayerBird(birdName);
    }

    [TargetRpc]
    public void TargetSetPlayerID(NetworkConnectionToClient target, string playerID)
    {
        SettingsManager.Instance.playerID = playerID;
    }

    [TargetRpc]
    public void TargetRequestSteamID(NetworkConnectionToClient target)
    {
        LobbyNetwork.Instance.lobbyDataHandler.CmdSetSteamID(Steamworks.SteamFriends.GetPersonaName());
    }

    [TargetRpc]
    public void TargetOpenLobby(NetworkConnectionToClient target)
    {
        LobbyNetwork.Instance.OnJoinedLobby();
    }

    [ClientRpc]
    public void RpcUpdateWordGroups(List<WordGroupData> wordGroups)
    {
        MenuLobbyButtons.Instance.wordGroupsController.UpdateWordGroups(wordGroups);
    }

    [TargetRpc]
    public void TargetReturnToLobby(NetworkConnectionToClient target)
    {
        MenuLobbyButtons.Instance.LoadLobbyFromGame();
        SettingsManager.Instance.currentSceneTransitionState = SettingsManager.SceneTransitionState.return_to_lobby_room;
        Cursor.visible = true;
    }

    [TargetRpc]
    public void TargetKickPlayer(NetworkConnectionToClient target)
    {
        NetworkManager.singleton.StopClient();
    }

    [ClientRpc]
    public void RpcCloseLobbyLoadingPage()
    {
        MenuLobbyButtons.Instance.LoadingPageObject.SetActive(false);
    }


}