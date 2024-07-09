using System.Collections.Generic;
using UnityEngine;

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
