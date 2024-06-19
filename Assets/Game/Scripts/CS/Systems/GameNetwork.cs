
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

        public void GameOver_ReturnToLobby()
        {

            SettingsManager.Instance.disconnected = false;
            if (SettingsManager.Instance.isHost)
            {
                GameManager.Instance.gameDataHandler.RpcHostJoiningLobby();
            }
            SettingsManager.Instance.currentSceneTransitionState = SettingsManager.SceneTransitionState.return_to_lobby_room;
            Cursor.visible = true;
            SceneManager.LoadScene(0);
            
        }

        public void Disconnected_ReturnToLobby()
        {
            if (SettingsManager.Instance.isHost)
            {
                SettingsManager.Instance.waitingForPlayers = true;
                SettingsManager.Instance.currentSceneTransitionState = SettingsManager.SceneTransitionState.return_to_lobby_room;
                NetworkManager.singleton.ServerChangeScene("MainMenu");
            }
        }

        public void Disconnected_ReturnToRooms()
        {
            SettingsManager.Instance.currentSceneTransitionState = SettingsManager.SceneTransitionState.return_to_room_listings;
            SettingsManager.Instance.disconnected = true;
            Cursor.visible = true;
            SceneManager.LoadScene(0);
            
        }
    }
}