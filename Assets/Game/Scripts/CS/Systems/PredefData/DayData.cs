using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChickenScratch
{
    [System.Serializable]
    public class DayData
    {
        public int goalPerPlayer;
        public float casesPerPlayer;
        public ResultData winResult, loseResult;
        public List<string> caseTypesToAddToPool;
        public List<string> unlocksToAddToPool;
        public int numberOfCaseTypeUnlocks;
    }
}
