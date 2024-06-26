using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChickenScratch
{
    [System.Serializable]
    public class CaseChoiceNetData
    {
        
        public List<List<string>> possibleWordsMap = new List<List<string>>();
        public List<string> correctWordIdentifiersMap = new List<string>();
        public string correctPrompt;
        public string caseChoiceIdentifier;
        public float maxScoreModifier;
        public float scoreModifierDecrement;
        public float modifierIncreaseValue;
    }
}
