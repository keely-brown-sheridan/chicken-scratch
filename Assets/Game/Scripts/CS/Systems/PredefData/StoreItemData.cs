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
    }
}
