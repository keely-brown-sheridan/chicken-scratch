using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChickenScratch
{
    [System.Serializable]
    public class StoreItemNetData
    {
        public StoreItem.StoreItemType itemType;
        public int index;

        public StoreItemNetData()
        {

        }

        public StoreItemNetData(StoreItemData storeItemData)
        {
            itemType = storeItemData.itemType;
            index = storeItemData.index;
        }
    }
}
