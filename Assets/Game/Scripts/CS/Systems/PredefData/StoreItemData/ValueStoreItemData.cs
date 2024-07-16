

using UnityEngine;

namespace ChickenScratch
{
    [CreateAssetMenu(fileName = "ValueStoreItem", menuName = "GameData/Create Value Store Item")]
    public class ValueStoreItemData : StoreItemData
    {
        public float value;

        public ValueStoreItemData()
        {

        }

        public override void Initialize(StoreItemData existingItemData)
        {
            base.Initialize(existingItemData);
            ValueStoreItemData valueItem = existingItemData as ValueStoreItemData;
            value = valueItem.value;
        }

        public ValueStoreItemData(ValueStoreItemNetData netData)
        {
            ValueStoreItemData gameData = (ValueStoreItemData)GameDataManager.Instance.GetMatchingStoreItem(netData.itemType);
            if(gameData != null)
            {
                value = netData.value;
                itemType = gameData.itemType;
                itemDescription = gameData.itemDescription;
                cost = gameData.cost;
                itemImagePrefab = gameData.itemImagePrefab;
                itemName = gameData.itemName;
                storeBGColour = gameData.storeBGColour;
                index = netData.index;
            }
        }
    }
}
