using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;

namespace ChickenScratch
{
    [CreateAssetMenu(fileName = "MarkerStoreItem", menuName = "GameData/Create Marker Store Item")]
    public class MarkerStoreItemData : StoreItemData
    {
        public Color markerColour;

        public MarkerStoreItemData()
        {

        }

        public override void Initialize(StoreItemData existingItemData)
        {
            base.Initialize(existingItemData);
            MarkerStoreItemData markerData = existingItemData as MarkerStoreItemData;
            markerColour = markerData.markerColour;
        }

        public MarkerStoreItemData(MarkerStoreItemNetData netData)
        {
            MarkerStoreItemData gameData = (MarkerStoreItemData)GameDataManager.Instance.GetMatchingStoreItem(netData.itemType);
            if(gameData != null)
            {
                markerColour = netData.markerColour;
                itemType = gameData.itemType;
                itemDescription = gameData.itemDescription;
                cost = gameData.cost;
                itemImagePrefab = gameData.itemImagePrefab;
                storeBGColour = netData.bgColour;
                itemName = gameData.itemName;
                index = netData.index;
            }
        }
    }
}
