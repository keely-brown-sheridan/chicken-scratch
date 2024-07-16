using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace ChickenScratch
{
    [CreateAssetMenu(fileName = "StoreItem", menuName = "GameData/Create Store Item")]
    public class StoreItemData : ScriptableObject
    {
        public StoreItem.StoreItemType itemType;
        public string itemName;
        public string itemDescription;
        public Color storeBGColour;
        public int cost;
        public GameObject itemImagePrefab;
        public bool isSinglePurchase = false;
        public int index;
        public int tier;
        public virtual void Initialize(StoreItemData existingItemData)
        {
            itemType = existingItemData.itemType;
            itemDescription = existingItemData.itemDescription;
            cost = existingItemData.cost;
            itemImagePrefab = existingItemData.itemImagePrefab;
            storeBGColour = existingItemData.storeBGColour;
            itemName = existingItemData.itemName;
            index = existingItemData.index;
        }

        public StoreItemData()
        {

        }


        public StoreItemData(StoreItemNetData netData)
        {
            StoreItemData gameData = GameDataManager.Instance.GetMatchingStoreItem(netData.itemType);
            if(gameData != null)
            {
                itemType = gameData.itemType;
                itemDescription = gameData.itemDescription;
                cost = gameData.cost;
                itemImagePrefab = gameData.itemImagePrefab;
                storeBGColour = gameData.storeBGColour;
                itemName = gameData.itemName;
                index = netData.index;
            }
        }
    }
}
