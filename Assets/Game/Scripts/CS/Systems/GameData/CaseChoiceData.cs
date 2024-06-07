using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChickenScratch
{
    [System.Serializable]
    public class CaseChoiceData
    {
        public string name;
        public CaseTemplateData.CaseFormat caseFormat;
        public int numberOfTasks;
        public int reward;
        public int cost;
        public CaseWordData.DifficultyDescriptor difficulty;
        public int penalty;
        public float startingScoreModifier;

        public List<CaseWordTemplateData> startingWords = new List<CaseWordTemplateData>();
        public List<List<string>> possibleWordsMap = new List<List<string>>();
        public List<CaseWordData> correctWordsMap = new List<CaseWordData>();
        public string correctPrompt;
    }
}
