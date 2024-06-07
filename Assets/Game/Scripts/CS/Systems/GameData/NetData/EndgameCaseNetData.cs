using ChickenScratch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChickenScratch
{
    [Serializable]
    public class EndgameCaseNetData
    {
        public int identifier;
        public List<int> taskDataKeys = new List<int>();
        public List<EndgameTaskData> taskDataValues = new List<EndgameTaskData>();
        public string correctPrompt;
        public List<int> guessesKeys = new List<int>();
        public List<string> guessesValues = new List<string>();
        public List<int> correctWordsKeys = new List<int>();
        public List<CaseWordData> correctWordsValues = new List<CaseWordData>();
        public float scoreModifier;
        public int pointsForBonus;
        public int pointsPerCorrectWord;
        public int penalty;
        public EndgameCaseNetData()
        {

        }
        public EndgameCaseNetData(EndgameCaseData inCaseData)
        {
            identifier = inCaseData.identifier;
            taskDataKeys = inCaseData.taskDataMap.Keys.ToList();
            taskDataValues = inCaseData.taskDataMap.Values.ToList();
            correctPrompt = inCaseData.correctPrompt;
            guessesKeys = inCaseData.guessesMap.Keys.ToList();
            guessesValues = inCaseData.guessesMap.Values.ToList();
            correctWordsKeys = inCaseData.correctWordsMap.Keys.ToList();
            correctWordsValues = inCaseData.correctWordsMap.Values.ToList();
            scoreModifier = inCaseData.scoreModifier;
            pointsForBonus = inCaseData.pointsForBonus;
            pointsPerCorrectWord = inCaseData.pointsPerCorrectWord;
            penalty = inCaseData.penalty;
        }
    }
}
