
using UnityEngine;
using Mirror;

namespace ChickenScratch
{
    public class DrawingTestManager : MonoBehaviour
    {

        [SerializeField]
        private GameObject serverButtonHolder, birdButtonHolder, startGameButton;



        private bool hasConfirmedReadyToStart = false;



        void Start()
        {
            SettingsManager.Instance.UpdateSetting("tutorials", false);
            SettingsManager.Instance.UpdateSetting("music", false);
            SettingsManager.Instance.UpdateSetting("stickies", false);
        }



        public void createTestRoom()
        {
            if (!NetworkClient.isConnected)
            {
                print("Connecting to server..");
                //PhotonNetwork.ConnectToRegion("us");
                CSNetworkManager.singleton.StartHost();
                serverButtonHolder.SetActive(false);
                birdButtonHolder.SetActive(true);
            }
        }

        public void joinTestRoom()
        {
            if (!NetworkClient.isConnected)
            {
                print("Connecting to server..");
                CSNetworkManager.singleton.StartClient();
                serverButtonHolder.SetActive(false);
                birdButtonHolder.SetActive(true);
            }
        }

        public void chooseBird(string birdName)
        {
            switch (birdName)
            {
                case "Red":
                    SettingsManager.Instance.birdName = ColourManager.BirdName.red;
                    break;
                case "Blue":
                    SettingsManager.Instance.birdName = ColourManager.BirdName.blue;
                    break;
                case "Green":
                    SettingsManager.Instance.birdName = ColourManager.BirdName.green;
                    break;
                case "Purple":
                    SettingsManager.Instance.birdName = ColourManager.BirdName.purple;
                    break;
                case "Orange":
                    SettingsManager.Instance.birdName = ColourManager.BirdName.orange;
                    break;
                case "Grey":
                    SettingsManager.Instance.birdName = ColourManager.BirdName.grey;
                    break;
                case "Black":
                    SettingsManager.Instance.birdName = ColourManager.BirdName.black;
                    break;
                case "Maroon":
                    SettingsManager.Instance.birdName = ColourManager.BirdName.maroon;
                    break;
            }

            tryToSelectBird(SettingsManager.Instance.playerName, SettingsManager.Instance.birdName);
            birdButtonHolder.SetActive(false);
            if(SettingsManager.Instance.isHost)
            {
                startGameButton.SetActive(true);
            }
        }

        public void tryToSelectBird(string playerName, ColourManager.BirdName birdName)
        {
            GameManager.Instance.gameDataHandler.CmdSetPlayerBird(playerName, birdName);
        }

        public void startGame()
        {
            GameManager.Instance.gameFlowManager.gameObject.SetActive(true);
            GameManager.Instance.gameDataHandler.RpcStartTestGame();
            gameObject.SetActive(false);
        }

        public void confirmReadyToStart()
        {
            if (!hasConfirmedReadyToStart)
            { 
                GameManager.Instance.gameDataHandler.CmdPlayerLoadedGame(SettingsManager.Instance.birdName);
                hasConfirmedReadyToStart = true;
            }

        }
    }
}