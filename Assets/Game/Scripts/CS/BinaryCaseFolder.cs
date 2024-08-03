using ChickenScratch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ChickenScratch.CaseChoiceData;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class BinaryCaseFolder : CaseFolder
    {
        [SerializeField]
        TMPro.TMP_Text guessText;

        [SerializeField]
        private Image yesButtonImage;

        [SerializeField]
        private Image noButtonImage;

        [SerializeField]
        private float drawingScalingFactor;

        [SerializeField]
        private DrawingsContainer guessingContainer;

        [SerializeField]
        private Vector3 drawingOffset;

        [SerializeField]
        private CaseWordCategoryVisual caseWordCategoryVisual;

        private int currentCaseIndex = -1;
        private CaseChoiceData.PromptFormat promptFormat;

        private string possiblePrefix, possibleNoun;

        private enum CurrentSelection
        {
            yes, no, none
        }
        private CurrentSelection currentSelection = CurrentSelection.none;

        public void Initialize(int round, int caseID, string inPossiblePrefix, string inPossibleNoun, DrawingData drawingData, List<TaskData.TaskModifier> taskModifiers, UnityAction inTimeCompleteAction)
        {
            possiblePrefix = inPossiblePrefix;
            possibleNoun = inPossibleNoun;
            casePlayerTabs.Initialize(round, caseID);
            currentCaseIndex = caseID;
            SetCaseTypeVisuals(caseID);
            SetCertificationSlots(caseID);

            currentSelection = CurrentSelection.none;
            yesButtonImage.color = Color.white;
            noButtonImage.color = Color.white;

            ChainData currentCase = GameManager.Instance.playerFlowManager.drawingRound.caseMap[caseID];
            CaseChoiceData caseChoiceData = GameDataManager.Instance.GetCaseChoice(currentCase.caseTypeName);
            promptFormat = caseChoiceData.promptFormat;

            guessingContainer.Show(drawingData, drawingScalingFactor, taskModifiers);
            guessText.text = SettingsManager.Instance.CreatePrefixText(possiblePrefix) + " " + SettingsManager.Instance.CreateNounText(possibleNoun);

            timeCompleteAction = inTimeCompleteAction;
            RegisterToTimer(inTimeCompleteAction);
        }

        public override void Show(Color inFolderColour, float taskTime, float currentModifier, float maxModifierValue, float modifierDecrement)
        {
            base.Show(inFolderColour, taskTime, currentModifier, maxModifierValue, modifierDecrement);
        }

        public override void Hide()
        {
            base.Hide();
            guessingContainer.HidePreviousDrawings();
            caseWordCategoryVisual.Hide();
        }

        public override bool HasStarted()
        {
            return currentSelection != CurrentSelection.none;
        }

        public bool ChooseGuess(int caseID)
        {
            AudioManager.Instance.PlaySoundVariant("sfx_game_int_answer_select");

            if (currentSelection == CurrentSelection.none)
            {
                return false;
            }
            StatTracker.Instance.wordIsGuessed = true;

            GameManager.Instance.playerFlowManager.drawingRound.stampIsActive = false;
            float timeTaken = GameManager.Instance.playerFlowManager.drawingRound.timeInCurrentCase;
            StatTracker.Instance.timeInGuessingRound += timeTaken;
            GuessData guessData = new GuessData();
            guessData.timeTaken = timeTaken;
            guessData.author = SettingsManager.Instance.birdName;
            switch (currentSelection)
            {
                case CurrentSelection.yes:
                    guessData.prefix = possiblePrefix;
                    guessData.noun = possibleNoun;
                    break;
                case CurrentSelection.no:
                    guessData.prefix = "no";
                    guessData.noun = "no";
                    break;
                default:
                    guessData.prefix = "";
                    guessData.noun = "";
                    break;
            }
            guessData.round = GameManager.Instance.playerFlowManager.drawingRound.queuedFolderMap[caseID].round;
            GameManager.Instance.gameDataHandler.CmdPromptGuess(guessData, currentCaseIndex, timeTaken);

            return true;
        }

        public void ForceGuess(int caseID)
        {
            AudioManager.Instance.PlaySound("ButtonPress");

            float timeTaken = GameManager.Instance.playerFlowManager.drawingRound.timeInCurrentCase;
            StatTracker.Instance.timeInGuessingRound += timeTaken;
            GuessData guessData = new GuessData();
            guessData.timeTaken = timeTaken;
            guessData.author = SettingsManager.Instance.birdName;
            switch(currentSelection)
            {
                case CurrentSelection.yes:
                    guessData.prefix = possiblePrefix;
                    guessData.noun = possibleNoun;
                    break;
                case CurrentSelection.no:
                    guessData.prefix = "no";
                    guessData.noun = "no";
                    break;
                default:
                    guessData.prefix = "";
                    guessData.noun = "";
                    break;
            }

            if (!GameManager.Instance.playerFlowManager.drawingRound.queuedFolderMap.ContainsKey(caseID))
            {
                Debug.LogError("ERROR[ForceGuess]: Queued folder map does not contain caseID[" + caseID.ToString() + "]");
                return;
            }
            guessData.round = GameManager.Instance.playerFlowManager.drawingRound.queuedFolderMap[caseID].round;
            GameManager.Instance.gameDataHandler.CmdPromptGuess(guessData, currentCaseIndex, timeTaken);

            StatTracker.Instance.wordIsGuessed = true;
        }

        public void OnYesPress()
        {
            AudioManager.Instance.PlaySoundVariant("sfx_game_int_answer_select");
            currentSelection = CurrentSelection.yes;
            yesButtonImage.color = Color.yellow;
            noButtonImage.color = Color.white;
        }

        public void OnNoPress()
        {
            AudioManager.Instance.PlaySoundVariant("sfx_game_int_answer_select");
            currentSelection = CurrentSelection.no;
            yesButtonImage.color = Color.white;
            noButtonImage.color = Color.yellow;
        }

        public void ShowCategory(WordCategoryData wordCategoryData)
        {
            caseWordCategoryVisual.Initialize(wordCategoryData);
            caseWordCategoryVisual.Show();
        }
    }
}


