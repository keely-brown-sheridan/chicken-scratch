using ChickenScratch;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ChickenScratch
{
    public class GuessingCaseFolder : CaseFolder
    {
        [SerializeField]
        TMPro.TMP_Text guessText;

        [SerializeField]
        private List<PossiblePrompt> allPossibleWords;

        [SerializeField]
        private float drawingScalingFactor;

        [SerializeField]
        private DrawingsContainer guessingContainer;

        [SerializeField]
        private Vector3 drawingOffset;

        [SerializeField]
        private CaseWordCategoryVisual caseWordCategoryVisual;

        private Dictionary<int, Dictionary<string, PossiblePrompt>> possibleWordMap;
        private Dictionary<int, string> correctWords = new Dictionary<int, string>();
        private Dictionary<int, string> guessWords = new Dictionary<int, string>();
        private int currentCaseIndex = -1;

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

        public void Initialize(int caseID, Dictionary<int,List<string>> inPossibleWords, DrawingData drawingData, UnityAction inTimeCompleteAction)
        {
            currentCaseIndex = caseID;
            guessingContainer.Show(drawingData, drawingScalingFactor, drawingOffset);
            PossiblePrompt currentPossibleWord;
            int iterator = 1;
            Color guessButtonColour = Color.white;
            Color guessButtonTextColour = Color.black;
            foreach (KeyValuePair<int, List<string>> possibleWordGroup in inPossibleWords)
            {
                if(possibleWordGroup.Key == 1)
                {
                    guessButtonColour = SettingsManager.Instance.prefixBGColour;
                    guessButtonTextColour = SettingsManager.Instance.prefixFontColour;
                }
                else if(possibleWordGroup.Key == 2)
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
            guessingContainer.HidePreviousDrawings();
            caseWordCategoryVisual.Hide();
        }

        public override bool HasStarted()
        {
            return guessWords.Count == 2;
        }

        public void ChooseGuess(int caseID)
        {
            AudioManager.Instance.PlaySoundVariant("sfx_game_int_answer_select");

            if (guessWords.Count < 2)
            {
                return;
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

            AudioManager.Instance.PlaySound("Stamp");
            GameManager.Instance.playerFlowManager.drawingRound.stampIsActive = false;
            float timeTaken = GameManager.Instance.playerFlowManager.drawingRound.timeInCurrentCase;
            StatTracker.Instance.timeInGuessingRound += timeTaken;
            GuessData guessData = new GuessData();
            guessData.timeTaken = timeTaken;
            guessData.author = SettingsManager.Instance.birdName;
            guessData.prefix = guessWords.ContainsKey(1) ? guessWords[1] : "";
            guessData.noun = guessWords.ContainsKey(2) ? guessWords[2] : "";
            guessData.round = GameManager.Instance.playerFlowManager.drawingRound.queuedFolderMap[caseID].round;
            GameManager.Instance.gameDataHandler.CmdPromptGuess(guessData, currentCaseIndex, timeTaken);
            ClearGuess(1);
            ClearGuess(2);
            guessWords.Clear();
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
                if(word.Value.wordIndex == 1)
                {
                    wordBackgroundColour = SettingsManager.Instance.prefixBGColour;
                }
                else if(word.Value.wordIndex == 2)
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
            string prefixText = guessWords.ContainsKey(1) ? SettingsManager.Instance.CreatePrefixText(guessWords[1]) : "";
            string nounText = guessWords.ContainsKey(2) ? SettingsManager.Instance.CreateNounText(guessWords[2]) : "";
            guessText.text = prefixText + " " + nounText;
        }

        public void ShowCategory(WordCategoryData wordCategoryData)
        {
            caseWordCategoryVisual.Initialize(wordCategoryData);
            caseWordCategoryVisual.Show();
        }
    }
}

