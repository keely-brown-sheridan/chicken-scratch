using ChickenScratch;
using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class LobbyDataHandler : NetworkBehaviour
{
    [ClientRpc]
    public void RpcSetPlayerListings(List<PlayerListingNetData> playerListingData)
    {
        LobbyNetwork.Instance.UpdateClientPlayerListings(playerListingData);
    }


    [Command(requiresAuthority = false)]
    public void CmdSelectPlayerBird(string playerID, ColourManager.BirdName selectedBird)
    {
        if(MenuLobbyButtons.Instance.IsBirdSelected(selectedBird))
        {
            return;
        }
        //If the player already has a bird selected then broadcast to have it deselected
        ColourManager.BirdName previousBird = MenuLobbyButtons.Instance.GetPreviouslySelectedBird(playerID);
        if (previousBird == selectedBird)
        {
            return;
        }
        else if(previousBird != ColourManager.BirdName.none)
        {
            RpcDeselectBirdBird(previousBird);
        }
        LobbyNetwork.Instance.SetBirdForPlayerID(playerID, selectedBird);
        RpcSelectPlayerBird(playerID, selectedBird);
    }

    [ClientRpc]
    public void RpcSelectPlayerBird(string playerID, ColourManager.BirdName selectedBird)
    {
        Debug.Log("RpcSelectPlayerBird");
        MenuLobbyButtons.Instance.SelectPlayerBird(selectedBird, playerID);
        
    }

    [ClientRpc]
    public void RpcDeselectBirdBird(ColourManager.BirdName birdName)
    {
        Debug.Log("RpcDeselectPlayerBird");
        MenuLobbyButtons.Instance.DeselectPlayerBird(birdName);
    }

    [TargetRpc]
    public void TargetSetPlayerID(NetworkConnectionToClient target, string playerID)
    {
        Debug.LogError("Setting player ID["+playerID+"]");
        SettingsManager.Instance.playerID = playerID;
    }

    [ClientRpc]
    public void RpcUpdateWordGroups(List<WordGroupData> wordGroups)
    {
        Debug.Log("Updating word groups["+wordGroups.Count.ToString()+"].");
        MenuLobbyButtons.Instance.wordGroupsController.UpdateWordGroups(wordGroups);
    }

}