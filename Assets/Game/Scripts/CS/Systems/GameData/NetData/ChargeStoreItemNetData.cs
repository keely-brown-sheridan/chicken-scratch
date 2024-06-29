using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChickenScratch
{
    [System.Serializable]
    public class ChargeStoreItemNetData
    {
        public StoreItem.StoreItemType itemType;
        public int charge;
        public int index;

        public ChargeStoreItemNetData()
        {

        }

        public ChargeStoreItemNetData(ChargedStoreItemData chargeData)
        {
            itemType = chargeData.itemType;
            charge = chargeData.numberOfUses;
            index = chargeData.index;
        }
    }
}
