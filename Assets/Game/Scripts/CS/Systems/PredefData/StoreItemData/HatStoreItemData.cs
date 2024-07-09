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

        public HatStoreItemData(HatStoreItemNetData netData)
        {
            HatStoreItemData gameData = (HatStoreItemData)GameDataManager.Instance.GetMatchingStoreItem(netData.itemType);
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
