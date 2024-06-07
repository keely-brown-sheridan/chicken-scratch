using ChickenScratch;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        public void Initialize(int caseID, Dictionary<int,List<string>> inPossibleWords, DrawingData drawingData)
        {
            currentCaseIndex = caseID;
            guessingContainer.Show(drawingData, drawingScalingFactor, drawingOffset);
            PossiblePrompt currentPossibleWord;
            int iterator = 1;
            foreach (KeyValuePair<int, List<string>> possibleWordGroup in inPossibleWords)
            {
                iterator = 1;
                foreach (string possibleWord in possibleWordGroup.Value)
                {
                    currentPossibleWord = possibleWordMap[possibleWordGroup.Key][iterator.ToString()];
                    currentPossibleWord.displayText.text = possibleWord;
                    currentPossibleWord.gameObject.SetActive(true);
                    iterator++;
                }
            }
        }

        public override void Show(Color inFolderColour, float taskTime, float currentModifier, float modifierDecrement)
        {
            base.Show(inFolderColour, taskTime, currentModifier, modifierDecrement);
        }

        public override void Hide()
        {
            base.Hide();
            guessingContainer.HidePreviousDrawings();
        }

        public override bool HasStarted()
        {
            return guessWords.Count == 2;
        }

        public void ChooseGuess()
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

            GameManager.Instance.gameDataHandler.CmdPromptGuessWrapper(SettingsManager.Instance.birdName, tempGuessWords, currentCaseIndex);
            ClearGuess(1);
            ClearGuess(2);
            guessWords.Clear();
        }

        public void ForceGuess()
        {
            AudioManager.Instance.PlaySound("ButtonPress");
            foreach (PossiblePrompt possiblePrompt in allPossibleWords)
            {
                possiblePrompt.gameObject.SetActive(false);
            }

            if (SettingsManager.Instance.isHost)
            {
                GameManager.Instance.playerFlowManager.addGuessPrompt(SettingsManager.Instance.birdName, guessWords, currentCaseIndex);
            }
            else
            {
                GameManager.Instance.gameDataHandler.CmdPromptGuessWrapper(SettingsManager.Instance.birdName, guessWords, currentCaseIndex);
            }
            Hide();
            StatTracker.Instance.wordIsGuessed = true;
        }

        private void ClearGuess(int wordIndex)
        {
            guessText.text = "";
            //Unhighlight other prefix buttons
            foreach (KeyValuePair<string, PossiblePrompt> word in possibleWordMap[wordIndex])
            {
                if (word.Value.isCorrect)
                {
                    word.Value.backgroundImage.color = Color.green;
                }
                else
                {
                    word.Value.backgroundImage.color = Color.white;
                }
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
            guessText.color = ColourManager.Instance.birdMap[SettingsManager.Instance.birdName].colour;
            guessText.text = (guessWords.ContainsKey(1) ? guessWords[1] : "") + " " + (guessWords.ContainsKey(2) ? guessWords[2] : "");
        }
    }
}

