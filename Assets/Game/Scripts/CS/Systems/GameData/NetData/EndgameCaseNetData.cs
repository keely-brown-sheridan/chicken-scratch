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
        public List<int> taskDataKeys = new List<int>();
        public List<EndgameTaskData> taskDataValues = new List<EndgameTaskData>();
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
            taskDataKeys = inCaseData.taskDataMap.Keys.ToList();
            taskDataValues = inCaseData.taskDataMap.Values.ToList();
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
