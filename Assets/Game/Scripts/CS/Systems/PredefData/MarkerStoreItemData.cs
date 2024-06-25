using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ChickenScratch
{
    [CreateAssetMenu(fileName = "MarkerStoreItem", menuName = "GameData/Create Marker Store Item")]
    public class MarkerStoreItemData : StoreItemData
    {
        public Color markerColour;
    }
}
