
using System.Linq;
using UnityEngine;
using static ChickenScratch.ColourManager;

namespace ChickenScratch
{
    public class DCManager : MonoBehaviour
    {
        public enum GameScene
        {
            lobby, game, theater
        }

        public GameScene currentGameScene;
        public GameObject DCPrompt;
        public DisconnectionNotification disconnectionNotification;
        private bool masterClientHasSwitched = false;

        public void OnLeftRoom()
        {
            switch (currentGameScene)
            {
                case GameScene.lobby:
                    MenuLobbyButtons.Instance.LeftRoom();
                    print("Left room.");
                    break;
                case GameScene.game:
                case GameScene.theater:
                    GameNetwork.Instance.Disconnected_ReturnToLobby();
                    break;

            }

        }

        public void OnDisconnected()
        {
            switch (currentGameScene)
            {
                case GameScene.lobby:
                    MenuLobbyButtons.Instance.OpenRoomsPage();
                    LobbyNetwork.Instance.LeaveRoom();
                    Debug.LogError("Disconnected from room.");
                    break;
                case GameScene.game:
                case GameScene.theater:
                    return;
                    //handleHostDisconnection();
                    //break;
            }

        }

        public void OnMasterClientSwitched()
        {
            masterClientHasSwitched = true;
            switch (currentGameScene)
            {
                case GameScene.theater:
                case GameScene.game:
                    DCPrompt.SetActive(true);
                    GameManager.Instance.playerFlowManager.active = false;
                    break;
            }

        }

        public void handleHostDisconnection()
        {
            switch (currentGameScene)
            {
                case GameScene.theater:
                case GameScene.game:
                    GameFlowManager.GamePhase currentPhase = GameManager.Instance.playerFlowManager.currentPhaseName;
                    if (currentPhase == GameFlowManager.GamePhase.results)
                    {
                        GameManager.Instance.playerFlowManager.resultsRound.HostHasReturnedToLobby();
                        return;
                    }
                    DCPrompt.SetActive(true);
                    GameManager.Instance.playerFlowManager.active = false;
                    break;
            }
        }

        public void OnPlayerLeftRoom(BirdName disconnectedPlayer)
        {
            switch (currentGameScene)
            {
                case GameScene.lobby:

                    break;
                case GameScene.game:

                    GameFlowManager.GamePhase currentPhase = GameManager.Instance.playerFlowManager.currentPhaseName;
                    
                    //GameManager.Instance.playerFlowManager.playerNameMap.Remove(disconnectingBird);
                    if (SettingsManager.Instance.isHost)
                    {
                        GameManager.Instance.gameFlowManager.disconnectedPlayers.Add(disconnectedPlayer);
                        SettingsManager.Instance.DeassignBirdToPlayer(disconnectedPlayer);
                        GameManager.Instance.gameFlowManager.clearPlayerTransitionConditions(disconnectedPlayer);

                        if (SettingsManager.Instance.GetPlayerNameCount() == 1)
                        {
                            DCPrompt.SetActive(true);
                            GameManager.Instance.playerFlowManager.active = false;
                            return;
                        }
                    }


                    //Show a notification saying that the player has disconnected
                    disconnectionNotification.QueueDisconnection(disconnectedPlayer, GameManager.Instance.playerFlowManager.playerNameMap[disconnectedPlayer]);

                    if (masterClientHasSwitched)
                    {
                        return;
                    }
                    switch (currentPhase)
                    {
                        case GameFlowManager.GamePhase.slides:
                        case GameFlowManager.GamePhase.slides_tutorial:
                        case GameFlowManager.GamePhase.results:
                        case GameFlowManager.GamePhase.accolades:
                            //Do nothing, these modes can handle losing a player

                            break;
                        case GameFlowManager.GamePhase.loading:
                        case GameFlowManager.GamePhase.game_tutorial:
                        case GameFlowManager.GamePhase.instructions:

                            if (SettingsManager.Instance.isHost)
                            {
                                switch (SettingsManager.Instance.gameMode.caseDeliveryMode)
                                {
                                    case GameModeData.CaseDeliveryMode.queue:
                                        //If chains have already been distributed, remake the player order
                                        if (GameManager.Instance.gameFlowManager.IsInitialized)
                                        {
                                            GameManager.Instance.gameFlowManager.reorderCasesOnDisconnect(disconnectedPlayer);
                                        }
                                        break;
                                }
                            }

                            break;
                        case GameFlowManager.GamePhase.drawing:

                            if (SettingsManager.Instance.isHost)
                            {
                                DrawingRound drawingRound = GameManager.Instance.playerFlowManager.drawingRound;
                                switch (SettingsManager.Instance.gameMode.caseDeliveryMode)
                                {
                                    case GameModeData.CaseDeliveryMode.queue:

                                        //Close their cabinet
                                        int disconnectedCabinetIndex = GameManager.Instance.gameFlowManager.playerCabinetMap[disconnectedPlayer];
                                        GameManager.Instance.playerFlowManager.drawingRound.cabinetDrawerMap[disconnectedCabinetIndex].close();
                                        GameManager.Instance.gameDataHandler.RpcCloseCabinetDrawer(disconnectedCabinetIndex);

                                        //If it's the first round and there's enough players to make enough rounds with 1 less player,
                                        //then remake the player order

                                        break;
                                }
                            }

                            break;
                    }


                    break;
            }

        }

    }
}