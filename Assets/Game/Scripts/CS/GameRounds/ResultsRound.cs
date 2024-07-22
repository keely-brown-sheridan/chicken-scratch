
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

        public Transform emailButtonContainer;


        public SummaryEmailContents gameSummaryWindow;
        public GameObject containerObject;
        public EndGameState endgameState = EndGameState.incomplete;
        public Image lastSelectedButtonImage;

        public Color selectedEmailButtonColour, unselectedEmailButtonColour;

        [SerializeField]
        private Transform emailHolderTransform;

        [SerializeField]
        private GameObject dailyFileEmailPrefab;

        [SerializeField]
        private Button lobbyButton;

        [SerializeField]
        private GameObject creditsWindowObject;

        [SerializeField]
        private Text returnToLobbyText;

        [SerializeField]
        private GameObject quitPromptObject;

        [SerializeField]
        private GameObject hostHasReturnedToLobbyPromptObject;

        [SerializeField]
        private GameObject emailButtonPrefab;
        
        private GameObject currentOpenEmail;

        private void Start()
        { 
        }



        public override void StartRound()
        {
            base.StartRound();
            ShowResults();

            if (SettingsManager.Instance.isHost)
            {
                returnToLobbyText.text = "Back";
                lobbyButton.interactable = true;
            }
        }

        public void ShowResults()
        {
            GameManager.Instance.playerFlowManager.instructionRound.lanyardCard.gameObject.SetActive(false);

            GameObject temp;
            EmailButton tempButton;

            temp = Instantiate(emailButtonPrefab, emailButtonContainer);
            tempButton = temp.GetComponent<EmailButton>();

            if (tempButton)
            {
                tempButton.window = gameSummaryWindow.gameObject;
                currentOpenEmail = gameSummaryWindow.gameObject;
                gameSummaryWindow.gameObject.SetActive(true);
                tempButton.unreadImage.color = selectedEmailButtonColour;
                lastSelectedButtonImage = tempButton.unreadImage;
                tempButton.text.text = "WEEKLY REPORT";
            }
            gameSummaryWindow.setSummaryContents();

            //Create all of the daily emails
            List<string> dayNames = new List<string>();
            foreach(EndgameCaseData caseData in GameManager.Instance.playerFlowManager.slidesRound.caseDataMap.Values)
            {
                if(!dayNames.Contains(caseData.dayName))
                {
                    dayNames.Add(caseData.dayName);
                }
            }
            foreach(string dayName in dayNames)
            {
                GameObject dailyEmailContentsObject = Instantiate(dailyFileEmailPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity, emailHolderTransform);
                DailyEmailContents dailyEmail = dailyEmailContentsObject.GetComponent<DailyEmailContents>();
                dailyEmailContentsObject.transform.localPosition = Vector3.zero;
                dailyEmail.Initialize(dayName);
                GameObject dailyEmailButtonObject = Instantiate(emailButtonPrefab, emailButtonContainer);
                EmailButton dailyEmailButton = dailyEmailButtonObject.GetComponent<EmailButton>();
                dailyEmailButton.window = dailyEmailContentsObject;
                dailyEmailButton.text.text = dayName.ToUpper() + " REPORT";
            }

            //Create button for credits email
            temp = Instantiate(emailButtonPrefab, emailButtonContainer);
            tempButton = temp.GetComponent<EmailButton>();

            if (tempButton)
            {
                tempButton.window = creditsWindowObject.gameObject;
                tempButton.text.text = "MEET THE TEAM";
            }
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

        public void OpenEmail(GameObject emailObject, Image unreadImage)
        {
            if (currentOpenEmail)
            {
                currentOpenEmail.SetActive(false);
            }
            currentOpenEmail = emailObject;
            currentOpenEmail.SetActive(true);
            if (lastSelectedButtonImage)
            {
                lastSelectedButtonImage.color = unselectedEmailButtonColour;
            }
            unreadImage.color = selectedEmailButtonColour;
            lastSelectedButtonImage = unreadImage;
        }
    }
}