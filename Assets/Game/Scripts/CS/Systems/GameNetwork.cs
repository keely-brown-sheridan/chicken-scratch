
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ChickenScratch.ColourManager;

namespace ChickenScratch
{
    public class GameNetwork : Singleton<GameNetwork>
    {
        public static BirdName getBirdNameFromCode(string inCode)
        {
            switch (inCode)
            {
                case "red":
                    return ColourManager.BirdName.red;
                case "blue":
                    return ColourManager.BirdName.blue;
                case "green":
                    return ColourManager.BirdName.green;
                case "grey":
                    return ColourManager.BirdName.grey;
                case "purple":
                    return ColourManager.BirdName.purple;
                case "maroon":
                    return ColourManager.BirdName.maroon;
                case "black":
                    return ColourManager.BirdName.black;
                case "orange":
                    return ColourManager.BirdName.orange;
                case "teal":
                    return ColourManager.BirdName.teal;
                case "yellow":
                    return ColourManager.BirdName.yellow;
                case "brown":
                    return ColourManager.BirdName.brown;
                case "pink":
                    return ColourManager.BirdName.pink;
            }

            return ColourManager.BirdName.none;
        }

        public void Disconnected_ReturnToLobby()
        {
            AudioManager.Instance.StopSound("EndMusic");
            AudioManager.Instance.StopSound("GameMusic");
            AudioManager.Instance.StopSound("SlidesMusic");
            AudioManager.Instance.StopSound("AccoladesMusic");
            Cursor.visible = true;
            LobbyNetwork.Instance.lobbyDataHandler.CloseGame();
            if (SettingsManager.Instance.isHost)
            {
                SettingsManager.Instance.waitingForPlayers = true;
                SettingsManager.Instance.currentSceneTransitionState = SettingsManager.SceneTransitionState.return_to_lobby_room;
                Steamworks.SteamMatchmaking.SetLobbyJoinable(SettingsManager.Instance.currentRoomID, true);
                
                //NetworkManager.singleton.ServerChangeScene("MainMenu");
            }
        }

        public void Disconnected_ReturnToRooms()
        {
            SettingsManager.Instance.disconnected = true;
            if (NetworkServer.active || NetworkClient.isConnected)
            {
                if (SettingsManager.Instance.isHost)
                {
                    NetworkManager.singleton.StopHost();
                }
                else
                {
                    NetworkManager.singleton.StopClient();
                }
            }
            AudioManager.Instance.StopSound("EndMusic");
            AudioManager.Instance.StopSound("GameMusic");
            AudioManager.Instance.StopSound("SlidesMusic");
            AudioManager.Instance.StopSound("AccoladesMusic");

            SettingsManager.Instance.ClearAllPlayers();
            SettingsManager.Instance.birdName = BirdName.none;
            SettingsManager.Instance.currentSceneTransitionState = SettingsManager.SceneTransitionState.return_to_room_listings;
            LobbyNetwork.Instance.lobbyDataHandler.CloseGame();
            MenuLobbyButtons.Instance.CloseLobbyPageFromDisconnect();
            
            Cursor.visible = true;
        }
    }
}