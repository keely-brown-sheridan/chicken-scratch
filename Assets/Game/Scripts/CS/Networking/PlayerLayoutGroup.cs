
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Mirror;

namespace ChickenScratch
{
    public class PlayerLayoutGroup : MonoBehaviour
    {
        [SerializeField]
        private GameObject _playerListingPrefab;
        private GameObject PlayerListingPrefab
        {
            get { return _playerListingPrefab; }
        }

        private List<PlayerListing> _playerListings = new List<PlayerListing>();
        public List<PlayerListing> PlayerListings
        {
            get { return _playerListings; }
        }

        public CSteamID lobbyID;

        public void PlayerJoinedRoom(string playerID, string nickName, ColourManager.BirdName selectedBird = ColourManager.BirdName.none)
        {
            
            PlayerLeftRoom(playerID, nickName);
            if (MenuLobbyButtons.Instance.PlayerListingMap.ContainsKey(playerID))
            {
                Debug.LogError("ERROR[PlayerJoinedRoom]: A player with identifier[" + playerID + "] has already joined the room.");
                return;
            }

            GameObject playerListingObj = GameObject.Instantiate(PlayerListingPrefab);
            playerListingObj.transform.SetParent(transform, false);

            PlayerListing playerListing = playerListingObj.GetComponent<PlayerListing>();
            playerListing.ApplyPlayer(playerID, nickName, playerID == SettingsManager.Instance.hostID);
            playerListing.ChangePlayerBird(selectedBird);

            PlayerListings.Add(playerListing);
            
            MenuLobbyButtons.Instance.PlayerListingMap.Add(playerID, playerListing);
        }

        public void PlayerLeftRoom(string playerID, string nickname)
        {
            int index = PlayerListings.FindIndex(x => x.PlayerID == playerID);

            if (MenuLobbyButtons.Instance.PlayerListingMap.ContainsKey(playerID.ToString()))
            {
                MenuLobbyButtons.Instance.PlayerListingMap.Remove(playerID.ToString());
            }

            if (index != -1)
            {
                if (PlayerListings[index] != null && PlayerListings[index].gameObject)
                {
                    GameObject.Destroy(PlayerListings[index].gameObject);
                }

                PlayerListings.RemoveAt(index);
            }
        }

        public void Set(List<PlayerListingNetData> playerListingData)
        {
            for (int i = PlayerListings.Count - 1; i >= 0; i--)
            {
                if (PlayerListings[i] != null && PlayerListings[i].gameObject != null)
                {
                    Destroy(PlayerListings[i].gameObject);
                }
            }
            foreach (PlayerListingNetData playerListing in playerListingData)
            {
                PlayerJoinedRoom(playerListing.playerName, playerListing.playerName, playerListing.selectedBird);
            }
        }


    }
}