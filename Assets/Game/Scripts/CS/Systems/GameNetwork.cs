
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

            SceneManager.LoadScene(0);
            Cursor.visible = true;
        }

        public void Disconnected_ReturnToLobby()
        {
            if (SettingsManager.Instance.isHost)
            {
                GameManager.Instance.gameDataHandler.RpcHostJoiningLobby();
            }
            SettingsManager.Instance.disconnected = true;
            SceneManager.LoadScene(0);
            Cursor.visible = true;
        }

        public void Disconnected_ReturnToRooms()
        {
            SettingsManager.Instance.currentSceneTransitionState = SettingsManager.SceneTransitionState.return_to_room_listings;
            SettingsManager.Instance.playerQuit = true;
            SceneManager.LoadScene(0);
            Cursor.visible = true;
        }





        public abstract class NetworkMessage
        {
            public enum SendType
            {
                broadcast, to_player, to_server
            }

            public string[] messageSegments;

            public NetworkMessage(string[] inMessageSegments)
            {
                messageSegments = inMessageSegments;
            }

            public void printMessageSegments()
            {
                string messageToPrint = "";
                foreach (string messageSegment in messageSegments)
                {
                    messageToPrint += messageSegment + ",";
                }

                Debug.LogError(messageToPrint);
            }

            public bool validateSegmentCount(int expectedCount, string messageName)
            {
                if (messageSegments.Length != expectedCount)
                {
                    Debug.LogError("Invalid number of segments in message[" + messageSegments.Length + "] for " + messageName + " message.");
                    printMessageSegments();
                    return false;
                }
                return true;
            }

            public abstract bool resolve();
        }







    }
}