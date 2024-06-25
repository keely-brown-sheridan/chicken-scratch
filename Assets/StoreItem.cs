using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class StoreItem : MonoBehaviour
    {
        public enum StoreItemType
        {
            eraser, marker, white_out, reroll, category_preview, score_tracker, highlighter, stopwatch, case_tab
        }
        [SerializeField]
        private TMPro.TMP_Text itemNameText;

        [SerializeField]
        private TMPro.TMP_Text itemDescriptionText;

        [SerializeField]
        private TMPro.TMP_Text itemCostText;

        [SerializeField]
        private Image bgImage;

        [SerializeField]
        private Transform itemImageHolder;

        [SerializeField]
        private GameObject activeContentsObject;

        [SerializeField]
        private GameObject inactiveContentsObject;

        [SerializeField]
        private string purchaseSFX;

        [SerializeField]
        private string cantAffordSFX;

        private StoreItemData storeItemData;
        private int cost;

        public void Initialize(StoreItemData inStoreItemData)
        {
            storeItemData = inStoreItemData;
            itemNameText.text = inStoreItemData.itemName;
            itemDescriptionText.text = inStoreItemData.itemDescription;
            itemCostText.text = inStoreItemData.cost.ToString();
            cost = inStoreItemData.cost;
            bgImage.color = inStoreItemData.storeBGColour;
            GameObject storeItemImageObject = Instantiate(inStoreItemData.itemImagePrefab, itemImageHolder);
            StoreImageItem storeItemImage = storeItemImageObject.GetComponent<StoreImageItem>();
            storeItemImage.Initialize(inStoreItemData);
        }

        public void Purchase()
        {
            //Check if the player can afford it
            if(GameManager.Instance.playerFlowManager.storeRound.currentMoney >= cost)
            {
                //Purchase it
                GameManager.Instance.playerFlowManager.storeRound.DecreaseCurrentMoney(cost);
                GameManager.Instance.playerFlowManager.AddStoreItem(storeItemData);

                //Play a sound effect
                AudioManager.Instance.PlaySound(purchaseSFX);

                //Cover up in the shop
                activeContentsObject.SetActive(false);
                inactiveContentsObject.SetActive(true);
            }
            else
            {
                GameManager.Instance.playerFlowManager.storeRound.ShowUnaffordableNotification();
                AudioManager.Instance.PlaySound(cantAffordSFX, true);
            }
        }
    }

}
