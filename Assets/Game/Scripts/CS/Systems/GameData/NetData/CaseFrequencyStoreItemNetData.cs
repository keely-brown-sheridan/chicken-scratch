using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChickenScratch
{
    [Serializable]
    public class CaseFrequencyStoreItemNetData
    {
        public string itemName;
        public int index;
        public CaseFrequencyStoreItemNetData()
        {

        }

        public CaseFrequencyStoreItemNetData(CaseFrequencyStoreItemData gameData)
        {
            itemName = gameData.itemName;
            index = gameData.index;
        }
    }
}
