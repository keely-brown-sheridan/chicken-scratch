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
        public GuessData guessData = new GuessData();
        public Dictionary<int, string> correctWordIdentifierMap = new Dictionary<int, string>();
        public float scoreModifier;
        public float maxScoreModifier;
        public int pointsForBonus;
        public int pointsPerCorrectWord;
        public int penalty;
        public string caseTypeName = "";
        public Color caseTypeColour = Color.white;
        public CaseScoringData scoringData = new CaseScoringData();

        public EndgameCaseData()
        {
            //scoringData = new CaseScoringData(this);
        }

        public EndgameCaseData(ChainData inChainData)
        {
            identifier = inChainData.identifier;
            correctPrompt = inChainData.correctPrompt;
            correctWordIdentifierMap = inChainData.correctWordIdentifierMap;
            guessData = inChainData.guessData;
            scoreModifier = inChainData.currentScoreModifier;
            maxScoreModifier = inChainData.maxScoreModifier;
            pointsForBonus = inChainData.pointsForBonus;
            pointsPerCorrectWord = inChainData.pointsPerCorrectWord;
            penalty = inChainData.penalty;
            caseTypeName = inChainData.caseTypeName;
            caseTypeColour = inChainData.caseTypeColour;

            foreach(TaskData gameTask in inChainData.taskQueue)
            {
                int taskRound = taskDataMap.Count + 1;
                float timeModifierDecrement = inChainData.taskQueue[taskRound - 1].timeModifierDecrement;
                EndgameTaskData endgameTaskData = new EndgameTaskData(gameTask, taskRound, inChainData.playerOrder[taskRound], timeModifierDecrement);
                switch(gameTask.taskType)
                {
                    case TaskData.TaskType.prompt_drawing:
                    case TaskData.TaskType.base_drawing:
                    case TaskData.TaskType.copy_drawing:
                    case TaskData.TaskType.add_drawing:
                    case TaskData.TaskType.compile_drawing:
                        if (!inChainData.drawings.ContainsKey(taskRound))
                        {
                            endgameTaskData.isComplete = false;
                            continue;
                        }
                        else
                        {
                            endgameTaskData.drawingData = inChainData.drawings[taskRound];
                        }
                        
                        break;
                    case TaskData.TaskType.prompting:
                        if(!inChainData.prompts.ContainsKey(taskRound))
                        {
                            endgameTaskData.isComplete = false;
                            continue;
                        }
                        else
                        {
                            endgameTaskData.promptData = inChainData.prompts[taskRound];
                        }
                        
                        break;
                    case TaskData.TaskType.base_guessing:
                        if(inChainData.guessData == null || !inChainData.IsComplete())
                        {
                            endgameTaskData.isComplete = false;
                            continue;
                        }
                        break;
                }
                taskDataMap.Add(taskRound, endgameTaskData);
            }
            scoringData = new CaseScoringData(this);
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
            guessData = netData.guessData;
            correctWordIdentifierMap = new Dictionary<int, string>();
            for (int i = 0; i < netData.correctWordsKeys.Count; i++)
            {
                correctWordIdentifierMap.Add(netData.correctWordsKeys[i], netData.correctWordsValues[i]);
            }
            scoreModifier = netData.scoreModifier;
            pointsForBonus = netData.pointsForBonus;
            pointsPerCorrectWord = netData.pointsPerCorrectWord;
            penalty = netData.penalty;
            caseTypeName = netData.caseTypeName;
            caseTypeColour = netData.caseTypeColour;
            scoringData = netData.scoringData;
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

        public bool WasCorrect()
        {
            foreach (KeyValuePair<int, string> correctWordIdentifier in correctWordIdentifierMap)
            {
                CaseWordData correctWord = GameDataManager.Instance.GetWord(correctWordIdentifier.Value);
                bool wordIsCorrect = (correctWordIdentifier.Key == 1 && correctWord.value == guessData.prefix) || (correctWordIdentifier.Key == 2 && correctWord.value == guessData.noun);
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
            foreach (KeyValuePair<int, string> correctWordIdentifier in correctWordIdentifierMap)
            {   
                CaseWordData correctWord = GameDataManager.Instance.GetWord(correctWordIdentifier.Value);
                totalDifficulty += correctWord.difficulty;
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

        public bool ContainsBird(ColourManager.BirdName bird)
        {
            
            foreach(KeyValuePair<int,EndgameTaskData> taskData in taskDataMap)
            {
                if(taskData.Value.assignedPlayer == bird)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
