using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChickenScratch
{
    [System.Serializable]
    public class MarkerStoreItemNetData
    {
        public StoreItem.StoreItemType itemType;
        public Color markerColour;
        public Color bgColour;
        public int index;

        public MarkerStoreItemNetData()
        {

        }

        public MarkerStoreItemNetData(MarkerStoreItemData markerData)
        {
            itemType = markerData.itemType;
            markerColour = markerData.markerColour;
            bgColour = markerData.storeBGColour;
            index = markerData.index;
        }
    }
}
