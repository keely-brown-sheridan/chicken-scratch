
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

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

        public void SteamPlayerJoinedRoom(CSteamID lobbyID, CSteamID playerID, string nickName)
        {
            this.lobbyID = lobbyID;
            PlayerJoinedRoom(playerID.m_SteamID.ToString(), nickName);
        }

        public void PlayerJoinedRoom(string playerID, string nickName, ColourManager.BirdName selectedBird = ColourManager.BirdName.none)
        {
            PlayerLeftRoom(playerID, nickName);

            Debug.Log("Instantiating a player listing for player[" + nickName + "]");
            GameObject playerListingObj = GameObject.Instantiate(PlayerListingPrefab);
            playerListingObj.transform.SetParent(transform, false);

            PlayerListing playerListing = playerListingObj.GetComponent<PlayerListing>();
            playerListing.ApplyPlayer(playerID, nickName, false);
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
                if (PlayerListings[index].gameObject)
                {
                    Debug.Log("Destroying player listing.");
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
                    Debug.Log("Destroying player listing.");
                    Destroy(PlayerListings[i].gameObject);
                }
            }
            foreach (PlayerListingNetData playerListing in playerListingData)
            {
                PlayerJoinedRoom(playerListing.playerID.ToString(), playerListing.playerName, playerListing.selectedBird);
            }
        }


    }
}