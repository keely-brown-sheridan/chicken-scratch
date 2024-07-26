﻿using System;
using System.Collections.Generic;
using UnityEngine;
using static ChickenScratch.TaskData;

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
        public bool hasBeenShown = false;
        public string dayName = "";

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
            dayName = SettingsManager.Instance.GetCurrentDayName();

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
                    case TaskData.TaskType.blender_drawing:
                        if (!inChainData.drawings.ContainsKey(taskRound))
                        {
                            endgameTaskData.isComplete = false;
                            continue;
                        }
                        else
                        {
                            endgameTaskData.drawingData = inChainData.drawings[taskRound];
                            endgameTaskData.expectingDrawing = false;
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
                    case TaskData.TaskType.morph_guessing:
                    case TaskData.TaskType.base_guessing:
                    case TaskData.TaskType.competition_guessing:
                        if (inChainData.guessData == null || !inChainData.IsComplete())
                        {
                            endgameTaskData.isComplete = false;
                            continue;
                        }
                        break;
                }
                taskDataMap.Add(taskRound, endgameTaskData);
            }
            scoringData = new CaseScoringData(this, inChainData.pointsPerCorrectWord, inChainData.pointsForBonus);
        }

        public EndgameCaseData(EndgameCaseNetData netData)
        {
            identifier = netData.identifier;
            taskDataMap = new Dictionary<int, EndgameTaskData>();
            for (int i = 0; i < netData.taskData.Count; i++)
            {
                int taskIndex = netData.taskData[i].round;
                taskDataMap.Add(taskIndex, new EndgameTaskData(netData.taskData[i]));
                TaskType taskType = taskDataMap[taskIndex].taskType;
                if (taskType == TaskData.TaskType.base_drawing ||
                    taskType == TaskType.compile_drawing ||
                    taskType == TaskType.prompt_drawing ||
                    taskType == TaskType.add_drawing ||
                    taskType == TaskType.copy_drawing ||
                    taskType == TaskType.blender_drawing)
                {
                    taskDataMap[taskIndex].expectingDrawing = true;
                }
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
            dayName = netData.dayName;
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
                if(taskData.taskType == TaskData.TaskType.base_guessing || taskData.taskType == TaskType.morph_guessing || taskData.taskType == TaskType.competition_guessing)
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

        public int GetPointsForPlayerOnTask(ColourManager.BirdName assignedPlayer)
        {
            int split = 1;
            CaseChoiceData caseChoice = GameDataManager.Instance.GetCaseChoice(caseTypeName);
            if(caseChoice == null)
            {
                return 0;
            }
            if(caseChoice.caseFormat == CaseTemplateData.CaseFormat.competition)
            {
                split = 2;
            }
            else if(taskDataMap.Count != 0)
            {
                split = taskDataMap.Count;
            }
            else
            {
                return 0;
            }
            return (int)((float)scoringData.GetTotalPointsForPlayer(assignedPlayer) / (float)split);
        }
    }
}
