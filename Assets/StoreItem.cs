using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class StoreItem : MonoBehaviour
    {
        public enum StoreItemType
        {
            eraser, marker, white_out, reroll, category_preview, score_tracker, highlighter, stopwatch, case_tab, case_unlock, case_upgrade,
            coffee_pot, coffee_mug, advertisement, nest_feathering, hat, case_frequency, campaign, case_certification, sponsorship, coffee_run, 
            favour, corruption
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



        public int index => _index;

        private StoreItemData storeItemData;
        private int cost;
        private int _index = -1;
        
        public enum State
        {
            in_stock, out_of_stock
        }
        public State currentState => _currentState;
        private State _currentState = State.in_stock;

        public void Initialize(StoreItemData inStoreItemData)
        {
            List<Transform> itemImages = new List<Transform>();
            foreach(Transform child in itemImageHolder)
            {
                if(itemImageHolder == child)
                {
                    continue;
                }
                itemImages.Add(child);
            }
            for(int i = itemImages.Count -1 ; i >= 0; i--)
            {
                Destroy(itemImages[i].gameObject);
            }

            activeContentsObject.SetActive(true);
            inactiveContentsObject.SetActive(false);
            _currentState = State.in_stock;
            storeItemData = inStoreItemData;
            itemNameText.text = inStoreItemData.itemName;
            itemDescriptionText.text = inStoreItemData.itemDescription;
            itemCostText.text = inStoreItemData.cost.ToString();
            cost = inStoreItemData.cost;
            _index = inStoreItemData.index;
            bgImage.color = inStoreItemData.storeBGColour;
            if(inStoreItemData.itemImagePrefab == null || itemImageHolder == null)
            {
                Debug.LogError("prefab is null[" + (inStoreItemData.itemImagePrefab == null).ToString() + "] image holder is null[" + (itemImageHolder == null).ToString() + "]");
                return;
            }
            GameObject storeItemImageObject = Instantiate(inStoreItemData.itemImagePrefab, itemImageHolder);
            StoreImageItem storeItemImage = storeItemImageObject.GetComponent<StoreImageItem>();
            storeItemImage.Initialize(inStoreItemData);
        }

        public void TryPurchase()
        {
            //Check if the player can afford it
            if(GameManager.Instance.playerFlowManager.storeRound.currentMoney >= cost)
            {
                if(GameManager.Instance.playerFlowManager.HasStoreItem(storeItemData.itemType))
                {
                    GameManager.Instance.playerFlowManager.storeRound.ShowAlreadyHaveNotification();
                    AudioManager.Instance.PlaySound(cantAffordSFX, true);
                }
                else
                {
                    bool isDrawingTool = storeItemData.itemType == StoreItemType.eraser || storeItemData.itemType == StoreItemType.marker || storeItemData.itemType == StoreItemType.highlighter;
                    if(isDrawingTool && GameManager.Instance.playerFlowManager.numberOfDrawingTools == 4)
                    {
                        GameManager.Instance.playerFlowManager.storeRound.ShowDrawingToolsNotification();
                        AudioManager.Instance.PlaySound(cantAffordSFX, true);
                    }
                    else
                    {
                        //Request to purchase from the server
                        GameManager.Instance.gameDataHandler.CmdTryToPurchaseStoreItem(SettingsManager.Instance.birdName, storeItemData.index);
                    }
                    
                }
                
            }
            else
            {
                GameManager.Instance.playerFlowManager.storeRound.ShowUnaffordableNotification();
                AudioManager.Instance.PlaySound(cantAffordSFX, true);
            }
        }

        public void Purchase(ColourManager.BirdName purchaser)
        {
            if(purchaser == SettingsManager.Instance.birdName)
            {
                if(storeItemData.itemType == StoreItemType.eraser ||
                    storeItemData.itemType == StoreItemType.highlighter ||
                    storeItemData.itemType == StoreItemType.marker)
                {
                    GameManager.Instance.playerFlowManager.numberOfDrawingTools++;
                }

                //Purchase it
                GameManager.Instance.playerFlowManager.storeRound.DecreaseCurrentMoney(cost);
                GameManager.Instance.playerFlowManager.AddStoreItem(storeItemData);

                StatTracker.Instance.totalItemsPurchased++;
                StatTracker.Instance.totalSpent += cost;
                if(storeItemData.itemType == StoreItemType.coffee_pot || storeItemData.itemType == StoreItemType.coffee_mug)
                {
                    StatTracker.Instance.totalCoffeeItemsPurchased++;
                }

                //Play a sound effect
                AudioManager.Instance.PlaySound(purchaseSFX);
            }

            //Cover up in the shop
            activeContentsObject.SetActive(false);
            inactiveContentsObject.SetActive(true);
            _currentState = State.out_of_stock;
        }


    }

}
