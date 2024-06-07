using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChickenScratch
{
    [System.Serializable]
    public class CaseWordTemplateData
    {
        public enum CaseWordType
        {
            descriptor, noun, invalid
        }
        public CaseWordType type = CaseWordType.invalid;

        public int difficultyMinimum = 1;
        public int difficultyMaximum = 5;
        public int cost = 0;
        public int reward = 0;
        public int numberOfOptionsForGuessing = 4;
    }
}
