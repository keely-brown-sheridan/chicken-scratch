using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ChickenScratch
{
    [CreateAssetMenu(fileName = "HatStoreItem", menuName = "GameData/Create Hat Store Item")]
    public class HatStoreItemData : StoreItemData
    {
        public BirdHatData.HatType hatType;

        public HatStoreItemData()
        {

        }

        public override void Initialize(StoreItemData existingItemData)
        {
            base.Initialize(existingItemData);
            HatStoreItemData hatData = existingItemData as HatStoreItemData;
            hatType = hatData.hatType;
        }

        public HatStoreItemData(HatStoreItemData existingHatData)
        {
            hatType = existingHatData.hatType;
            itemType = existingHatData.itemType;
            itemDescription = existingHatData.itemDescription;
            cost = existingHatData.cost;
            itemImagePrefab = existingHatData.itemImagePrefab;
            itemName = existingHatData.itemName;
            storeBGColour = existingHatData.storeBGColour;
            index = existingHatData.index;
        }

        public HatStoreItemData(HatStoreItemNetData netData)
        {
            HatStoreItemData gameData = (HatStoreItemData)GameDataManager.Instance.GetHatStoreItem();
            if (gameData != null)
            {
                hatType = netData.hatType;
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
