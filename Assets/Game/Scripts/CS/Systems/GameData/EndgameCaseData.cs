using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    [Serializable]
    public class EndgameCaseData
    {
        public int identifier;
        public Dictionary<int, EndgameTaskData> taskDataMap = new Dictionary<int, EndgameTaskData>();
        public string correctPrompt;
        public Dictionary<int, string> guessesMap = new Dictionary<int, string>();
        public Dictionary<int, CaseWordData> correctWordsMap = new Dictionary<int, CaseWordData>();
        public float scoreModifier;
        public int pointsForBonus;
        public int pointsPerCorrectWord;
        public int penalty;



        public EndgameCaseData()
        {

        }

        public EndgameCaseData(ChainData inChainData)
        {
            identifier = inChainData.identifier;
            correctPrompt = inChainData.correctPrompt;
            correctWordsMap = inChainData.correctWordsMap;
            guessesMap = inChainData.guessesMap;
            scoreModifier = inChainData.currentScoreModifier;
            pointsForBonus = inChainData.pointsForBonus;
            pointsPerCorrectWord = inChainData.pointsPerCorrectWord;
            penalty = inChainData.penalty;

            foreach(TaskData gameTask in inChainData.taskQueue)
            {
                int taskRound = taskDataMap.Count + 1;
                EndgameTaskData endgameTaskData = new EndgameTaskData(gameTask, taskRound, inChainData.playerOrder[taskRound]);
                switch(gameTask.taskType)
                {
                    case TaskData.TaskType.prompt_drawing:
                    case TaskData.TaskType.base_drawing:
                    case TaskData.TaskType.copy_drawing:
                    case TaskData.TaskType.add_drawing:
                        endgameTaskData.drawingData = inChainData.drawings[taskRound];
                        break;
                    case TaskData.TaskType.prompting:
                        endgameTaskData.promptData = inChainData.prompts[taskRound];
                        break;
                }
                taskDataMap.Add(taskRound, endgameTaskData);
            }

        }

        public EndgameCaseData(EndgameCaseNetData netData)
        {
            identifier = netData.identifier;
            taskDataMap = new Dictionary<int, EndgameTaskData>();
            for (int i = 0; i < netData.taskDataKeys.Count; i++)
            {
                taskDataMap.Add(netData.taskDataKeys[i], netData.taskDataValues[i]);
            }
            correctPrompt = netData.correctPrompt;
            guessesMap = new Dictionary<int, string>();
            for (int i = 0; i < netData.guessesKeys.Count; i++)
            {
                guessesMap.Add(netData.guessesKeys[i], netData.guessesValues[i]);
            }
            correctWordsMap = new Dictionary<int, CaseWordData>();
            for (int i = 0; i < netData.correctWordsKeys.Count; i++)
            {
                correctWordsMap.Add(netData.correctWordsKeys[i], netData.correctWordsValues[i]);
            }
            scoreModifier = netData.scoreModifier;
            pointsForBonus = netData.pointsForBonus;
            pointsPerCorrectWord = netData.pointsPerCorrectWord;
            penalty = netData.penalty;
        }

        public void IncreaseRating(int round, ColourManager.BirdName target)
        {
            if(!taskDataMap.ContainsKey(round))
            {
                Debug.LogError("ERROR[IncreaseRating]: Could not increase rating for round["+round.ToString()+"] because it was missing.");
            }
            taskDataMap[round].ratingData.likeCount++;
            taskDataMap[round].ratingData.target = target;
        }

        public int GetTotalPoints()
        {
            int totalPoints = 0;
            bool allCorrect = true;
            foreach(KeyValuePair<int,CaseWordData> correctWord in correctWordsMap)
            {
                if(guessesMap.ContainsKey(correctWord.Key) && guessesMap[correctWord.Key] == correctWord.Value.value)
                {
                    totalPoints += pointsPerCorrectWord;
                }
                else
                {
                    allCorrect = false;
                }
            }
            if(allCorrect)
            {
                totalPoints += pointsForBonus;
            }
            return totalPoints;
        }

        public bool WasCorrect()
        {
            foreach (KeyValuePair<int, CaseWordData> correctWord in correctWordsMap)
            {
                bool wordIsCorrect = guessesMap.ContainsKey(correctWord.Key) && guessesMap[correctWord.Key] == correctWord.Value.value;
                if(!wordIsCorrect)
                {
                    return false;
                }
            }
            return true;
        }

        public int GetDifficulty()
        {
            int totalDifficulty = 0;
            foreach (KeyValuePair<int, CaseWordData> correctWord in correctWordsMap)
            {
                totalDifficulty += correctWord.Value.difficulty;
            }
            return totalDifficulty;
        }

        public ColourManager.BirdName GetGuesser()
        {
            foreach(EndgameTaskData taskData in taskDataMap.Values)
            {
                if(taskData.taskType == TaskData.TaskType.base_guessing)
                {
                    return taskData.assignedPlayer;
                }
            }
            return ColourManager.BirdName.none;
        }
    }
}
