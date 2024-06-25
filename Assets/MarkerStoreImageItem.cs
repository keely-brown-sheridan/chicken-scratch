using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class MarkerStoreImageItem : StoreImageItem
    {
        [SerializeField]
        private Image markerFillImage;

        public override void Initialize(StoreItemData storeItemData)
        {
            markerFillImage.color = ((MarkerStoreItemData)storeItemData).markerColour;
            base.Initialize(storeItemData);
        }
    }
}

