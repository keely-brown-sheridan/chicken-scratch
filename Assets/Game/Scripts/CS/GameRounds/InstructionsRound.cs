
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ChickenScratch.RoleData;

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

        public TutorialSticky choicesSticky2, choicesSticky3, modifierSticky, caseCabinetSticky;
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
            Cursor.visible = true;
            if (SettingsManager.Instance.isHost)
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

            BirdData playerBird = GameDataManager.Instance.GetBird(SettingsManager.Instance.birdName);
            if(playerBird == null)
            {
                Debug.LogError("Could not set the colour of the desk renderer because the playerBird["+SettingsManager.Instance.birdName.ToString()+"] has not been mapped to the ColourManager.");
            }
            else
            {
                GameManager.Instance.playerFlowManager.drawingRound.deskRenderer.color = playerBird.bgColour;
            }

            string currentDay = SettingsManager.Instance.GetCurrentDayName();
            int currentGoal = GameManager.Instance.playerFlowManager.GetCurrentGoal();
            dayInstructions.Show(currentDay, currentGoal);
        }

        private void initializeInstructionsRound()
        {
            active = true;
        }

        public void InitializePlayer(ColourManager.BirdName birdName, string playerName, RoleData.RoleType roleType)
        {
            SettingsManager.Instance.playerRole = GameDataManager.Instance.GetRole(roleType);
            string roleName = SettingsManager.Instance.playerRole.roleName;
            Color roleColour = alignmentText.color = SettingsManager.Instance.playerRole.roleColour;
            lanyardCard.Initialize(playerName, birdName, roleName, roleColour);
            lanyardCard.transform.parent.gameObject.SetActive(true);
            GameManager.Instance.playerFlowManager.drawingRound.playerBirdArm.Initialize();
            AudioManager.Instance.PlaySound("sfx_game_env_worker_card_start");
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