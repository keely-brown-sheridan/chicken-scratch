using ChickenScratch;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ChickenScratch
{
    [Serializable]
    public class EndgameCaseNetData
    {
        public int identifier;
        public List<EndgameTaskNetData> taskData = new List<EndgameTaskNetData>();
        public string correctPrompt;
        public GuessData guessData;
        public List<int> correctWordsKeys = new List<int>();
        public List<string> correctWordsValues = new List<string>();
        public float scoreModifier;
        public int pointsForBonus;
        public int pointsPerCorrectWord;
        public int penalty;
        public string caseTypeName;
        public Color caseTypeColour;
        public CaseScoringData scoringData;
        public EndgameCaseNetData()
        {

        }
        public EndgameCaseNetData(EndgameCaseData inCaseData)
        {
            identifier = inCaseData.identifier;
            foreach(KeyValuePair<int,EndgameTaskData> task in inCaseData.taskDataMap)
            {
                taskData.Add(new EndgameTaskNetData(task.Value));
            }
            correctPrompt = inCaseData.correctPrompt;
            guessData = inCaseData.guessData;
            correctWordsKeys = inCaseData.correctWordIdentifierMap.Keys.ToList();
            correctWordsValues = inCaseData.correctWordIdentifierMap.Values.ToList();
            scoreModifier = inCaseData.scoreModifier;
            pointsForBonus = inCaseData.pointsForBonus;
            pointsPerCorrectWord = inCaseData.pointsPerCorrectWord;
            penalty = inCaseData.penalty;
            caseTypeName = inCaseData.caseTypeName;
            caseTypeColour = inCaseData.caseTypeColour;
            scoringData = inCaseData.scoringData;
        }
    }
}
