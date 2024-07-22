using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChickenScratch
{
    [Serializable]
    public class StoreChoiceOptionData
    {
        public string dayName;
        public float birdbucksPerPlayer;
        public float timeRamp;
        public int numberOfUnlocks;

        public List<ContractCaseUnlockData> unlocks = new List<ContractCaseUnlockData>();
        //More things will be added in here as certification is added
        //Could include upgrades
        //Could include day restrictions
    }
}
