using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ChickenScratch
{
    [CreateAssetMenu(fileName = "ChargedStoreItem", menuName = "GameData/Create Charged Store Item")]
    public class ChargedStoreItemData : StoreItemData
    {
        public int numberOfUses;

        public ChargedStoreItemData()
        {

        }

        public override void Initialize(StoreItemData existingItemData)
        {
            base.Initialize(existingItemData);
            ChargedStoreItemData chargeData = existingItemData as ChargedStoreItemData;
            numberOfUses = chargeData.numberOfUses;
        }

        public ChargedStoreItemData(ChargeStoreItemNetData netData)
        {
            ChargedStoreItemData gameData = (ChargedStoreItemData)GameDataManager.Instance.GetMatchingStoreItem(netData.itemType);
            if(gameData != null)
            {
                numberOfUses = netData.charge;
                itemType = gameData.itemType;
                itemDescription = gameData.itemDescription;
                itemName = gameData.itemName;
                cost = gameData.cost;
                itemImagePrefab = gameData.itemImagePrefab;
                storeBGColour = gameData.storeBGColour;
                index = netData.index;
            }
        }
    }
}
