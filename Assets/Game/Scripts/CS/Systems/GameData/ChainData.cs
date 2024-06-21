using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ChickenScratch.ColourManager;
using static ChickenScratch.DrawingRound;

namespace ChickenScratch
{
    [Serializable]
    public class ChainData
    {
        public Dictionary<int, DrawingData> drawings = new Dictionary<int, DrawingData>();
        public Dictionary<int, PlayerTextInputData> prompts = new Dictionary<int, PlayerTextInputData>();
        public GuessData guessData = new GuessData();

        public Dictionary<int, BirdName> playerOrder = new Dictionary<int, BirdName>();
        public int currentRound
        {
            get
            {
                return _currentRound;
            }
            set
            {
                _currentRound = value;
            }
        }
        private int _currentRound = 1;
        public Dictionary<int, List<string>> possibleWordsMap = new Dictionary<int, List<string>>();
        public Dictionary<int, string> correctWordIdentifierMap = new Dictionary<int, string>();
        public string correctPrompt = "";
        public int pointsForBonus;
        public int pointsPerCorrectWord;
        public float currentScoreModifier;
        public float maxScoreModifier;
        public int penalty;
        public float currentTaskDuration;
        public List<TaskData.TaskModifier> currentTaskModifiers;
        public bool active => identifier != -1;
        public int identifier = -1;
        public string caseTypeName = "";
        public Color caseTypeColour = Color.white;

        public enum CaseModifier
        {

        }
        public List<CaseModifier> caseModifiers;

        public List<TaskData> taskQueue = new List<TaskData>();


        public ChainData()
        {

        }

        public void addDrawing(int round, DrawingData drawing)
        {
            //Debug.LogError("Adding drawing from author["+drawing.author+"] for round["+round+"].");
            if (drawings.ContainsKey(round))
            {
                drawings[round] = drawing;
            }
            else
            {
                drawings.Add(round, drawing);
            }
        }

        public void sendCaseDetailsToClient(BirdName clientName)
        {
            foreach (KeyValuePair<int, PlayerTextInputData> prompt in prompts)
            {
                prompt.Value.author = playerOrder[prompt.Key];
                if (clientName == SettingsManager.Instance.birdName)
                {
                    continue;
                }
                else
                {
                    GameManager.Instance.gameDataHandler.TargetChainPrompt(SettingsManager.Instance.GetConnection(clientName), identifier, prompt.Key, prompt.Value.author, prompt.Value.text, prompt.Value.timeTaken);
                }
            }
        }

        public void SetWordsFromChoice(CaseChoiceNetData choice)
        {
            possibleWordsMap = new Dictionary<int, List<string>>();
            int iterator = 1;
            foreach(List<string> possibleWord in choice.possibleWordsMap)
            {
                possibleWordsMap.Add(iterator, possibleWord);
                iterator++;
            }
            iterator = 1;
            foreach(string correctWordIdentifier in choice.correctWordIdentifiersMap)
            {
                correctWordIdentifierMap.Add(iterator, correctWordIdentifier);
                iterator++;
            }
            correctPrompt = choice.correctPrompt;
        }

        public CaseState GetCaseState()
        {
            if(taskQueue.Count < currentRound)
            {
                return CaseState.invalid;
            }
            switch(taskQueue[currentRound-1].taskType)
            {
                case TaskData.TaskType.base_drawing:
                case TaskData.TaskType.prompt_drawing:
                case TaskData.TaskType.compile_drawing:
                    return CaseState.drawing;
                case TaskData.TaskType.copy_drawing:
                    return CaseState.copy_drawing;
                case TaskData.TaskType.add_drawing:
                    return CaseState.add_drawing;
                case TaskData.TaskType.prompting:
                    return CaseState.prompting;
                case TaskData.TaskType.base_guessing:
                    return CaseState.guessing;
            }
            return CaseState.invalid;
        }

        public string GetFullGuess()
        {
            return guessData.prefix + " " + guessData.noun;
        }

        public bool IsComplete()
        {
            return guessData.prefix != "" || guessData.noun != "";
        }
    }
}
