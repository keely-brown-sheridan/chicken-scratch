
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ChickenScratch.ColourManager;
using static ChickenScratch.GameFlowManager;

namespace ChickenScratch
{
    public class ResultsRound : PlayerRound
    {
        public enum EndGameState
        {
            worker_early_win, worker_win, traitor_win, incomplete, worker_early_loss
        }

        public List<ResultVoteRow> allResultVoteRows;
        public Transform emailButtonContainer;
        public GameObject currentOpenEmail;
        public WorkersOutcomeContents workerWinWindow, workerWinWindow2;
        public SummaryEmailContents gameSummaryWindow;
        public FileSummaryEmailContents fileSummaryWindow;
        public List<BirdTag> emailWindows;
        public GameObject workerWinButtonPrefab, gameSummaryButtonPrefab, folderSummaryButtonPrefab;
        public GameObject containerObject;
        public EndGameState endgameState = EndGameState.incomplete;
        public Image lastSelectedButtonImage;
        public HonkManager honkManager;

        public Color selectedEmailButtonColour, unselectedEmailButtonColour;

        public Transform fileSummaryEmailsHolder;
        public GameObject fileSummaryEmailPrefab;

        [SerializeField]
        private GameObject playerStatRolePrefab;

        [SerializeField]
        private Transform playerStatRolesHolder;
        [SerializeField]
        private Button lobbyButton;

        [SerializeField]
        private UnityEngine.UI.Text returnToLobbyText;
        [SerializeField]
        private GameObject quitPromptObject;
        [SerializeField]
        private GameObject hostHasReturnedToLobbyPromptObject;



        private void Start()
        {

        }



        public override void StartRound()
        {
            base.StartRound();
            initializeResultsRound();

            if (SettingsManager.Instance.isHost)
            {
                returnToLobbyText.text = "Back";
                lobbyButton.interactable = true;
            }

            foreach (EndgameCaseData caseData in GameManager.Instance.playerFlowManager.slidesRound.caseDataMap.Values)
            {
                initializeCaseEmailContents(caseData);
            }
            fileSummaryWindow.enableFirstCase();
        }

        private void initializeResultsRound()
        {
            //Update endgame sheet
            ShowResults();
        }

        public void SetPlayerStatRoles(Dictionary<BirdName, AccoladesStatManager.StatRole> statRoleMap)
        {
            foreach (KeyValuePair<BirdName, AccoladesStatManager.StatRole> statRole in statRoleMap)
            {
                GameObject statRoleVisualizationObject = Instantiate(playerStatRolePrefab, playerStatRolesHolder);
                StatRoleCard statRoleCard = statRoleVisualizationObject.GetComponent<StatRoleCard>();
                statRoleCard.SetValues(statRole.Key, statRole.Value.name);
            }
        }

        public void ShowResults()
        {
            GameManager.Instance.playerFlowManager.instructionRound.lanyardCard.gameObject.SetActive(false);

            GameObject temp;
            EmailButton tempButton;

            temp = Instantiate(gameSummaryButtonPrefab, emailButtonContainer);
            tempButton = temp.GetComponent<EmailButton>();


            if (tempButton)
            {
                tempButton.window = gameSummaryWindow.gameObject;
                currentOpenEmail = gameSummaryWindow.gameObject;
                gameSummaryWindow.gameObject.SetActive(true);
                tempButton.unreadImage.color = selectedEmailButtonColour;
                lastSelectedButtonImage = tempButton.unreadImage;
            }

            temp = Instantiate(folderSummaryButtonPrefab, emailButtonContainer);
            tempButton = temp.GetComponent<EmailButton>();
            if (tempButton)
            {
                tempButton.window = fileSummaryWindow.gameObject;
            }
            string outcomeTextValue = "";
            Color outcomeTextColour = Color.black;
            int totalPoints = 0;
            SettingsManager.EndgameResult highestResult = null;
            foreach (EndgameCaseData caseData in GameManager.Instance.playerFlowManager.slidesRound.caseDataMap.Values)
            {
                totalPoints += caseData.GetTotalPoints();
            }

            foreach (SettingsManager.EndgameResult result in SettingsManager.Instance.resultPossibilities)
            {
                int requiredPointThreshold = (int)(result.getRequiredPointThreshold(SettingsManager.Instance.gameMode.name) * GameManager.Instance.playerFlowManager.playerNameMap.Count);
                if (highestResult == null && result.getRequiredPointThreshold(SettingsManager.Instance.gameMode.name) == 0)
                {
                    highestResult = result;
                }
                else if (requiredPointThreshold <= totalPoints)
                {
                    highestResult = result;
                }
            }

            if (highestResult != null)
            {
                outcomeTextValue = highestResult.resultName;
                outcomeTextColour = highestResult.resultTextColour;
                gameSummaryWindow.outcomeText.text = outcomeTextValue;
                gameSummaryWindow.outcomeText.color = outcomeTextColour;
            }

            gameSummaryWindow.setSummaryContents();
        }

        public void initializeCaseEmailContents(EndgameCaseData caseData)
        {
            if (!fileSummaryWindow.isInitialized)
            {
                fileSummaryWindow.initialize();
            }

            GameObject emailContentsObject = Instantiate(fileSummaryEmailPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity, fileSummaryEmailsHolder);
            emailContentsObject.transform.localPosition = Vector3.zero;
            CaseEmail emailContents = emailContentsObject.GetComponent<CaseEmail>();
            fileSummaryWindow.addCase(emailContentsObject, caseData.identifier);
            emailContents.initialize(caseData);
        }

        public void HostHasReturnedToLobby()
        {
            hostHasReturnedToLobbyPromptObject.SetActive(true);
            returnToLobbyText.text = "Back";
            lobbyButton.interactable = true;
        }

        public void OpenQuitPrompt()
        {
            AudioManager.Instance.PlaySoundVariant("sfx_ui_int_gen_sel");
            quitPromptObject.SetActive(true);
        }

        public void CloseQuitPrompt()
        {
            AudioManager.Instance.PlaySoundVariant("sfx_ui_int_cancel_back");
            quitPromptObject.SetActive(false);
        }

        public void CloseHostHasReturnedPrompt()
        {
            AudioManager.Instance.PlaySoundVariant("sfx_ui_int_cancel_back");
            hostHasReturnedToLobbyPromptObject.SetActive(false);
        }
    }
}