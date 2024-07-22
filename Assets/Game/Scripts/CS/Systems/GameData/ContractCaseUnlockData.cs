using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChickenScratch
{
    [Serializable]
    public class ContractCaseUnlockData
    {
        public int minBirdbucks = -1, maxBirdbucks = -1;
        public float multiplier = -1f;
        public string identifier = "";
        public string certificationIdentifier = "";
    }
}
