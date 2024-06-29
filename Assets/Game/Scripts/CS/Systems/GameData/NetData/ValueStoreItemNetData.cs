using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChickenScratch
{
    [System.Serializable]
    public class ValueStoreItemNetData
    {
        public StoreItem.StoreItemType itemType;
        public float value;
        public int index;

        public ValueStoreItemNetData()
        {

        }

        public ValueStoreItemNetData(ValueStoreItemData valueData)
        {
            itemType = valueData.itemType;
            value = valueData.value;
            index = valueData.index;
        }
    }
}
