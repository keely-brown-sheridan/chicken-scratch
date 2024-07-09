using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChickenScratch
{
    [System.Serializable]
    public class HatStoreItemNetData
    {
        public StoreItem.StoreItemType itemType;
        public BirdHatData.HatType hatType;
        public int index;

        public HatStoreItemNetData()
        {

        }

        public HatStoreItemNetData(HatStoreItemData valueData)
        {
            itemType = valueData.itemType;
            hatType = valueData.hatType;
            index = valueData.index;
        }
    }
}
