using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChickenScratch
{
    [System.Serializable]
    public class CaseUnlockStoreItemNetData
    {
        public string caseChoiceIdentifier;
        public int index;

        public CaseUnlockStoreItemNetData()
        {

        }

        public CaseUnlockStoreItemNetData(CaseUnlockStoreItemData gameData)
        {
            caseChoiceIdentifier = gameData.caseChoiceIdentifier;
            index = gameData.index;
        }
    }
}
