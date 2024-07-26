using ChickenScratch;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static ChickenScratch.CaseChoiceData;
using static ChickenScratch.TaskData;
using UnityEngine.UI;
using System.Linq;
using System;

namespace ChickenScratch
{
    public class CompetingCaseFolder : CaseFolder
    {
        [SerializeField]
        TMPro.TMP_Text guessText;

        [SerializeField]
        private List<PossiblePrompt> allPossibleWords;

        [SerializeField]
        private float drawingScalingFactor;

        [SerializeField]
        private DrawingsContainer choiceAContainer, choiceBContainer, choiceCContainer;

        [SerializeField]
        private GameObject choiceAVisualization, choiceBVisualization, choiceCVisualization;

        [SerializeField]
        private Vector3 drawingOffset;

        [SerializeField]
        private CaseWordCategoryVisual caseWordCategoryVisual;

        [SerializeField]
        private float choosingIndicatorShowDuration;

        [SerializeField]
        private List<Image> choosingIndicators;

        [SerializeField]
        private List<GameObject> chooseButtonObjects;

        [SerializeField]
        private GameObject choiceCChoiceObject;

        [SerializeField]
        private GameObject choiceCDrawingBoxObject;

        private Dictionary<int, Dictionary<string, PossiblePrompt>> possibleWordMap;
        private Dictionary<int, string> correctWords = new Dictionary<int, string>();
        private Dictionary<int, string> guessWords = new Dictionary<int, string>();
        private int currentCaseIndex = -1;
        private CaseChoiceData.PromptFormat promptFormat;
        private ColourManager.BirdName chosenDrawingAuthor = ColourManager.BirdName.none;
        private ColourManager.BirdName choiceAAuthor = ColourManager.BirdName.none, choiceBAuthor = ColourManager.BirdName.none, choiceCAuthor = ColourManager.BirdName.none;
        private float timeShowingChooseIndicators = 0f;

        private void Start()
        {
            possibleWordMap = new Dictionary<int, Dictionary<string, PossiblePrompt>>();

            foreach (PossiblePrompt possibleWord in allPossibleWords)
            {
                if (!possibleWordMap.ContainsKey(possibleWord.wordIndex))
                {
                    possibleWordMap.Add(possibleWord.wordIndex, new Dictionary<string, PossiblePrompt>());
                }
                if (possibleWordMap[possibleWord.wordIndex].ContainsKey(possibleWord.identifier))
                {
                    Debug.LogError("Identifier[" + possibleWord.identifier + "] repeated multiple times for possible word with word index[" + possibleWord.wordIndex.ToString() + "].");
                    continue;
                }
                possibleWordMap[possibleWord.wordIndex].Add(possibleWord.identifier, possibleWord);
            }
        }

        protected override void Update()
        {
            base.Update();
            if(timeShowingChooseIndicators > 0f)
            {
                timeShowingChooseIndicators += Time.deltaTime;
                float timeRatio = timeShowingChooseIndicators / choosingIndicatorShowDuration;
                foreach(Image chooseIndicator in choosingIndicators)
                {
                    chooseIndicator.color = new Color(chooseIndicator.color.r, chooseIndicator.color.g, chooseIndicator.color.b, 0.5f - timeRatio/2);
                }
                if(timeShowingChooseIndicators > choosingIndicatorShowDuration)
                {
                    timeShowingChooseIndicators = 0f;
                }
            }
        }

        public void Initialize(int round, int caseID, Dictionary<int, List<string>> inPossibleWords, List<DrawingData> drawings, List<TaskData.TaskModifier> taskModifiers, UnityAction inTimeCompleteAction)
        {
            foreach (GameObject chooseButtonObject in chooseButtonObjects)
            {
                chooseButtonObject.SetActive(true);
            }
            casePlayerTabs.Initialize(round, caseID);
            currentCaseIndex = caseID;
            SetCaseTypeVisuals(caseID);
            SetCertificationSlots(caseID);

            ChainData currentCase = GameManager.Instance.playerFlowManager.drawingRound.caseMap[caseID];
            CaseChoiceData caseChoiceData = GameDataManager.Instance.GetCaseChoice(currentCase.caseTypeName);
            promptFormat = caseChoiceData.promptFormat;
            chosenDrawingAuthor = ColourManager.BirdName.none;
            choiceAVisualization.SetActive(false);
            choiceBVisualization.SetActive(false);
            choiceCVisualization.SetActive(false);
            choiceAAuthor = drawings[0].author;
            choiceBAuthor = drawings[1].author;
            
            choiceAContainer.Show(drawings[0], drawingScalingFactor, taskModifiers);
            choiceBContainer.Show(drawings[1], drawingScalingFactor, taskModifiers);
            if(drawings.Count > 2)
            {
                choiceCChoiceObject.SetActive(true);
                choiceCDrawingBoxObject.SetActive(true);
                choiceCAuthor = drawings[2].author;
                choiceCContainer.Show(drawings[2], drawingScalingFactor, taskModifiers);
            }
            else
            {
                choiceCAuthor = ColourManager.BirdName.none;
                choiceCChoiceObject.SetActive(false);
                choiceCDrawingBoxObject.SetActive(false);
            }
            
            PossiblePrompt currentPossibleWord;
            int iterator = 1;
            Color guessButtonColour = Color.white;
            Color guessButtonTextColour = Color.black;
            foreach (KeyValuePair<int, List<string>> possibleWordGroup in inPossibleWords)
            {
                if (possibleWordGroup.Key == 1)
                {
                    guessButtonColour = SettingsManager.Instance.prefixBGColour;
                    guessButtonTextColour = SettingsManager.Instance.prefixFontColour;
                }
                else if (possibleWordGroup.Key == 2)
                {
                    guessButtonColour = SettingsManager.Instance.nounBGColour;
                    guessButtonTextColour = SettingsManager.Instance.nounFontColour;
                }
                iterator = 1;
                foreach (string possibleWord in possibleWordGroup.Value)
                {
                    currentPossibleWord = possibleWordMap[possibleWordGroup.Key][iterator.ToString()];
                    currentPossibleWord.displayText.text = possibleWord;
                    currentPossibleWord.displayText.color = guessButtonTextColour;
                    currentPossibleWord.backgroundImage.color = guessButtonColour;
                    currentPossibleWord.gameObject.SetActive(true);
                    iterator++;
                }
            }
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
            choiceAContainer.HidePreviousDrawings();
            choiceBContainer.HidePreviousDrawings();
            choiceCContainer.HidePreviousDrawings();
            caseWordCategoryVisual.Hide();
        }

        public void ChooseA()
        {
            chosenDrawingAuthor = choiceAAuthor;
            choiceAVisualization.SetActive(true);
            choiceBVisualization.SetActive(false);
            choiceCVisualization.SetActive(false);
            foreach (GameObject chooseButtonObject in chooseButtonObjects)
            {
                chooseButtonObject.SetActive(false);
            }
        }

        public void ChooseB()
        {
            chosenDrawingAuthor = choiceBAuthor;
            choiceAVisualization.SetActive(false);
            choiceBVisualization.SetActive(true);
            choiceCVisualization.SetActive(false);
            foreach(GameObject chooseButtonObject in chooseButtonObjects)
            {
                chooseButtonObject.SetActive(false);
            }
        }

        public void ChooseC()
        {
            chosenDrawingAuthor = choiceCAuthor;
            choiceAVisualization.SetActive(false);
            choiceBVisualization.SetActive(false);
            choiceCVisualization.SetActive(true);
            foreach (GameObject chooseButtonObject in chooseButtonObjects)
            {
                chooseButtonObject.SetActive(false);
            }
        }

        public override bool HasStarted()
        {
            return guessWords.Count == 2;
        }

        public bool ChooseGuess(int caseID)
        {
            AudioManager.Instance.PlaySoundVariant("sfx_game_int_answer_select");

            if(chosenDrawingAuthor == ColourManager.BirdName.none)
            {
                //Turn on choose flash
                timeShowingChooseIndicators = Time.deltaTime;
                return false;
            }
            if (guessWords.Count < 2)
            {
                return false;
            }

            StatTracker.Instance.wordIsGuessed = true;
            Dictionary<int, string> tempGuessWords = new Dictionary<int, string>();
            foreach (KeyValuePair<int, string> guessWord in guessWords)
            {
                tempGuessWords.Add(guessWord.Key, guessWord.Value);
            }

            foreach (PossiblePrompt possibleWord in allPossibleWords)
            {
                possibleWord.gameObject.SetActive(false);
            }

            GameManager.Instance.playerFlowManager.drawingRound.stampIsActive = false;
            float timeTaken = GameManager.Instance.playerFlowManager.drawingRound.timeInCurrentCase;
            StatTracker.Instance.timeInGuessingRound += timeTaken;
            GuessData guessData = new GuessData();
            guessData.timeTaken = timeTaken;
            guessData.author = SettingsManager.Instance.birdName;
            guessData.prefix = guessWords.ContainsKey(1) ? guessWords[1] : "";
            guessData.noun = guessWords.ContainsKey(2) ? guessWords[2] : "";
            guessData.round = GameManager.Instance.playerFlowManager.drawingRound.queuedFolderMap[caseID].round;
            GameManager.Instance.gameDataHandler.CmdSetCompetitionChoice(caseID, chosenDrawingAuthor);
            GameManager.Instance.gameDataHandler.CmdPromptGuess(guessData, currentCaseIndex, timeTaken);
            ClearGuess(1);
            ClearGuess(2);
            guessWords.Clear();

            return true;
        }

        public void ForceGuess(int caseID)
        {
            AudioManager.Instance.PlaySound("ButtonPress");
            foreach (PossiblePrompt possiblePrompt in allPossibleWords)
            {
                possiblePrompt.gameObject.SetActive(false);
            }
            float timeTaken = GameManager.Instance.playerFlowManager.drawingRound.timeInCurrentCase;
            StatTracker.Instance.timeInGuessingRound += timeTaken;
            GuessData guessData = new GuessData();
            guessData.timeTaken = timeTaken;
            guessData.author = SettingsManager.Instance.birdName;
            guessData.prefix = guessWords.ContainsKey(1) ? guessWords[1] : "";
            guessData.noun = guessWords.ContainsKey(2) ? guessWords[2] : "";

            if (chosenDrawingAuthor == ColourManager.BirdName.none)
            {
                //Choose one randomly
                List<ColourManager.BirdName> randomChoices = (new List<ColourManager.BirdName>() { choiceAAuthor, choiceBAuthor });
                if(choiceCAuthor != ColourManager.BirdName.none)
                {
                    randomChoices.Add(choiceCAuthor);
                }
                randomChoices = randomChoices.OrderBy(x => Guid.NewGuid()).ToList();
                chosenDrawingAuthor = randomChoices[0];
            }
            GameManager.Instance.gameDataHandler.CmdSetCompetitionChoice(caseID, chosenDrawingAuthor);
            if (!GameManager.Instance.playerFlowManager.drawingRound.queuedFolderMap.ContainsKey(caseID))
            {
                Debug.LogError("ERROR[ForceGuess]: Queued folder map does not contain caseID[" + caseID.ToString() + "]");
                return;
            }
            guessData.round = GameManager.Instance.playerFlowManager.drawingRound.queuedFolderMap[caseID].round;
            GameManager.Instance.gameDataHandler.CmdPromptGuess(guessData, currentCaseIndex, timeTaken);

            StatTracker.Instance.wordIsGuessed = true;
        }

        private void ClearGuess(int wordIndex)
        {
            guessText.text = "";
            Color wordBackgroundColour = Color.white;
            //Unhighlight other prefix buttons
            foreach (KeyValuePair<string, PossiblePrompt> word in possibleWordMap[wordIndex])
            {
                if (word.Value.wordIndex == 1)
                {
                    wordBackgroundColour = SettingsManager.Instance.prefixBGColour;
                }
                else if (word.Value.wordIndex == 2)
                {
                    wordBackgroundColour = SettingsManager.Instance.nounBGColour;
                }
                word.Value.backgroundImage.color = wordBackgroundColour;
            }
        }

        public void SetGuess(string wordIndexCombo)
        {
            AudioManager.Instance.PlaySoundVariant("sfx_game_int_answer_select");
            string[] wordIndexSegments = wordIndexCombo.Split(new string[] { "-" }, System.StringSplitOptions.None);
            if (wordIndexSegments.Length != 2)
            {
                Debug.LogError("Could not set word guess, word index combo only had one index.");
                return;
            }
            int wordIndex = -1;
            wordIndex = int.TryParse(wordIndexSegments[0], out wordIndex) ? wordIndex : -1;
            if (wordIndex == -1)
            {
                Debug.LogError("Could not set word guess, word index[" + wordIndexSegments[0] + "] wasn't a valid number.");
            }

            if (!guessWords.ContainsKey(wordIndex))
            {
                guessWords.Add(wordIndex, "");
            }
            guessWords[wordIndex] = possibleWordMap[wordIndex][wordIndexSegments[1]].displayText.text;


            ClearGuess(wordIndex);

            //Highlight the button
            possibleWordMap[wordIndex][wordIndexSegments[1]].backgroundImage.color = Color.yellow;
            string value1, value2;
            switch (promptFormat)
            {
                case PromptFormat.standard:
                    value1 = (guessWords.ContainsKey(1) ? SettingsManager.Instance.CreatePrefixText(guessWords[1]) : "");
                    value2 = (guessWords.ContainsKey(2) ? SettingsManager.Instance.CreateNounText(guessWords[2]) : "");
                    guessText.text = value1 + " " + value2;
                    break;
                case PromptFormat.variant:
                    value1 = (guessWords.ContainsKey(1) ? SettingsManager.Instance.CreateVariantText(guessWords[1]) : "");
                    value2 = (guessWords.ContainsKey(2) ? SettingsManager.Instance.CreateNounText(guessWords[2]) : "");
                    guessText.text = value1 + " " + value2;
                    break;
                case PromptFormat.location:
                    value1 = (guessWords.ContainsKey(1) ? SettingsManager.Instance.CreateNounText(guessWords[1]) : "");
                    value2 = (guessWords.ContainsKey(2) ? SettingsManager.Instance.CreateLocationText(guessWords[2]) : "");
                    guessText.text = value1 + " in the " + value2;
                    break;
            }
        }

        public void ShowCategory(WordCategoryData wordCategoryData)
        {
            caseWordCategoryVisual.Initialize(wordCategoryData);
            caseWordCategoryVisual.Show();
        }
    }
}

