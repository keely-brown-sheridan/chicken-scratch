using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChickenScratch
{
    [System.Serializable]
    public class CaseUpgradeStoreItemNetData
    {
        public string itemName;
        public int index;
        public CaseUpgradeStoreItemNetData()
        {

        }

        public CaseUpgradeStoreItemNetData(CaseUpgradeStoreItemData gameData)
        {
            itemName = gameData.itemName;
            index = gameData.index;
        }
    }
}
