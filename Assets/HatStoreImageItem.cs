
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class HatStoreImageItem : StoreImageItem
    {
        [SerializeField]
        private Image hatImage;

        public override void Initialize(StoreItemData storeItemData)
        {
            base.Initialize(storeItemData);
            HatStoreItemData hatItemData = (HatStoreItemData)storeItemData;

            HatData hatData = GameDataManager.Instance.GetHat(hatItemData.hatType);
            if(hatData != null)
            {
                hatImage.sprite = hatData.sprite;
                hatImage.rectTransform.sizeDelta = new Vector2(hatData.width, hatData.height);
            }
        }
    }
}

