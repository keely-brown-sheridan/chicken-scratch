
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class InstructionsRound : PlayerRound
    {
        public Text alignmentText;

        public List<TutorialSticky> allCabinetStickies;
        public TutorialSticky roleDeskSticky, traitorPromptsDeskSticky, removeDeskSticky, responsesDeskSticky, slidesRatingSticky, slidesChatSticky, accuseFolderSticky, accusePlayerSticky,
                                accuseRevealSticky, accusePresentButtonSticky, accusePresentMenuSticky, accuseFolderZoomSticky, accusePresentationViewerSticky, accusePresentationPresenterSticky,
                                evaluationsSticky, limitedDrawingBotcherSticky, addPromptBotcherSticky, evaluationBotcherSticky,
                                corporateScoreSticky, corporateTimerSticky,
                                drawingToolsSticky, drawingToolsSticky2, guessingSticky,
                                draftingSticky, draftingSticky2;
        public bool hasSeenFirstCabinet = false, hasGottenFirstCabinet = false, hasClickedFirstCabinet = false;
        public DeskCard lanyardCard;
        public Animator bossNoteWorkerAnimator, bossNoteBotcherAnimator;

        private Dictionary<int, TutorialSticky> cabinetStickyMap;
        public bool active = false;

        [SerializeField]
        private DayInstructions dayInstructions;

        public override void StartRound()
        {
            base.StartRound();
            if(SettingsManager.Instance.isHost)
            {
                GameManager.Instance.gameFlowManager.timeRemainingInPhase = timeInRound;
            }
            hasGottenFirstCabinet = false;
            hasSeenFirstCabinet = false;

            cabinetStickyMap = new Dictionary<int, TutorialSticky>();
            foreach (TutorialSticky cabinetSticky in allCabinetStickies)
            {
                cabinetStickyMap.Add(cabinetSticky.gameObject.GetComponent<IndexMap>().index, cabinetSticky);
            }

            if (SettingsManager.Instance.isHost)
            {
                initializeInstructionsRound();
            }
            if (SettingsManager.Instance.GetSetting("stickies"))
            {
                PlaceStartingStickies(false);
            }

            GameManager.Instance.playerFlowManager.drawingRound.deskRenderer.color = ColourManager.Instance.birdMap[SettingsManager.Instance.birdName].bgColour;

            int currentDay = GameManager.Instance.playerFlowManager.currentDay;
            int currentGoal = SettingsManager.Instance.GetCurrentGoal();
            dayInstructions.Show(currentDay, currentGoal);
        }

        private void initializeInstructionsRound()
        {

            //Distribute the alignments
            foreach (KeyValuePair<ColourManager.BirdName, PlayerData> player in GameManager.Instance.gameFlowManager.gamePlayers)
            {
                if (GameManager.Instance.gameFlowManager.disconnectedPlayers.Contains(player.Key))
                {
                    continue;
                }
                
                if (SettingsManager.Instance.birdName == player.Key)
                {
                    InitializePlayer(player.Key, player.Value);
                }
                else
                {
                    GameManager.Instance.gameDataHandler.TargetInitializePlayer(SettingsManager.Instance.GetConnection(player.Key), player.Key, player.Value);
                    //GameNetwork.Instance.addToPlayerQueue(player.Key.ToString() + GameDelim.BASE + "role_distribution" + GameDelim.BASE + player.Value.playerRole.ToString());
                }
            }
            active = true;
        }

        public void InitializePlayer(ColourManager.BirdName playerName, PlayerData player)
        {
            string roleName = "WORKER";
            Color roleColour = alignmentText.color = GameManager.Instance.workerColour;
            lanyardCard.Initialize(player.playerName, playerName, roleName, roleColour);
            lanyardCard.transform.parent.gameObject.SetActive(true);
            GameManager.Instance.playerFlowManager.drawingRound.playerBirdArm.Initialize();
            AudioManager.Instance.PlaySound("sfx_game_env_worker_card_start");

            GameManager.Instance.playerFlowManager.playerRole = player.playerRole;
        }

        private void PlaceStartingStickies(bool playerIsBotcher)
        {
            if(!removeDeskSticky.hasBeenPlaced)
            {
                removeDeskSticky.Queue(true);
            }
            
        }

        private void Update()
        {
        }

        public void showCabinetSticky(int index, bool messageToShow)
        {
            cabinetStickyMap[index].Queue(messageToShow);
            if (messageToShow)
            {
                hasSeenFirstCabinet = true;
            }
            else
            {
                hasGottenFirstCabinet = true;
            }
        }

        public void hideCabinetStickies()
        {
            hasClickedFirstCabinet = true;
            hasGottenFirstCabinet = true;
            hasSeenFirstCabinet = true;
            removeDeskSticky.Click();

            foreach (KeyValuePair<int, TutorialSticky> cabinetSticky in cabinetStickyMap)
            {
                cabinetSticky.Value.Click();
            }
        }

        public void handleCabinetOpening(int id, bool isCurrentPlayer)
        {
            if (!hasGottenFirstCabinet)
            {
                if (isCurrentPlayer)
                {
                    showCabinetSticky(id, false);
                }
                else if (!hasSeenFirstCabinet)
                {
                    //showCabinetSticky(id, true);
                }
            }
            else if (!hasSeenFirstCabinet)
            {
                if (!isCurrentPlayer)
                {
                    //showCabinetSticky(id, true);
                }
            }
        }
    }
}