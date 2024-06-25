using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class StoreRound : PlayerRound
    {
        [SerializeField]
        private GameObject storeObject;

        [SerializeField]
        private TMPro.TMP_Text currentMoneyText;

        [SerializeField]
        private TMPro.TMP_Text unaffordableNotificationText;

        [SerializeField]
        private float timeToShowUnaffordableNotification;

        [SerializeField]
        private Transform storeItemHolderParent;

        [SerializeField]
        private GameObject storeItemPrefab;

        [SerializeField]
        private int numberOfStoreItems;

        public int currentMoney => _currentMoney;

        private int _currentMoney;
        private float timeShowingUnaffordableNotification = 0f;

        private List<StoreItem.StoreItemType> shownStoreItems = new List<StoreItem.StoreItemType>();
        private List<ColourManager.BirdName> activePlayers = new List<ColourManager.BirdName>();

        public override void StartRound()
        {
            base.StartRound();

            if(SettingsManager.Instance.isHost)
            {
                GameManager.Instance.gameFlowManager.timeRemainingInPhase = timeInRound;
                activePlayers.AddRange(SettingsManager.Instance.GetAllActiveBirds());
            }

            ClearStoreItems();
            //Create store items
            for(int i = 0; i < numberOfStoreItems; i++)
            {
                CreateStoreItem();
            }

            storeObject.SetActive(true);
        }

        // Update is called once per frame
        void Update()
        {
            if(timeShowingUnaffordableNotification > 0f)
            {
                timeShowingUnaffordableNotification += Time.deltaTime;
                float timeRatio = timeShowingUnaffordableNotification / timeToShowUnaffordableNotification;
                unaffordableNotificationText.color = new Color(unaffordableNotificationText.color.r, unaffordableNotificationText.color.g, unaffordableNotificationText.color.b, 1 - timeRatio);
                if(timeRatio >= 1f)
                {
                    timeShowingUnaffordableNotification = 0f;
                    unaffordableNotificationText.gameObject.SetActive(false);
                }
            }
        }

        public void IncreaseCurrentMoney(int money)
        {
            _currentMoney += money;
            currentMoneyText.text = "Birdbucks:\n" + _currentMoney.ToString();
        }

        public void DecreaseCurrentMoney(int money)
        {
            _currentMoney -= money;
            currentMoneyText.text = "Birdbucks:\n" + _currentMoney.ToString();
        }

        public void ShowUnaffordableNotification()
        {
            unaffordableNotificationText.gameObject.SetActive(true);
            timeShowingUnaffordableNotification = Time.deltaTime;
        }

        public void FinishWithStoreForPlayer(ColourManager.BirdName player)
        {
            if(activePlayers.Contains(player))
            {
                activePlayers.Remove(player);
                if(activePlayers.Count == 0)
                {
                    GameManager.Instance.gameFlowManager.timeRemainingInPhase = 0f;
                }
            }
        }

        public void Close()
        {
            storeObject.SetActive(false);
            GameManager.Instance.gameDataHandler.CmdFinishWithStore(SettingsManager.Instance.birdName);
        }

        public void ClearStoreItems()
        {
            List<Transform> existingStoreItems = new List<Transform>();
            foreach(Transform child in storeItemHolderParent)
            {
                existingStoreItems.Add(child);
            }
            for(int i = existingStoreItems.Count - 1; i >= 0; i--)
            {
                GameObject.Destroy(existingStoreItems[i].gameObject);
            }
        }

        private void CreateStoreItem()
        {
            StoreItemData storeItemData = GameDataManager.Instance.GetUnusedStoreItem(shownStoreItems);
            GameObject storeItemObject = Instantiate(storeItemPrefab, storeItemHolderParent);
            StoreItem storeItem = storeItemObject.GetComponent<StoreItem>();
            if(storeItem != null)
            {
                if(storeItemData.itemType == StoreItem.StoreItemType.marker || storeItemData.itemType == StoreItem.StoreItemType.highlighter)
                {
                    //Generate a colour and set the value for the BG colour and the marker colour
                    Color randomColour = new Color(Random.Range(0.4f, 1f), Random.Range(0.4f, 1f), Random.Range(0.4f, 1f));
                    ((MarkerStoreItemData)storeItemData).markerColour = randomColour;
                    float red = randomColour.r;
                    float green = randomColour.g;
                    float blue = randomColour.b;
                    red = (1 - red) * 0.5f + red;
                    green = (1 - green) * 0.5f + green;
                    blue = (1 - blue) * 0.5f + blue;
                    if(storeItemData.itemType == StoreItem.StoreItemType.highlighter)
                    {
                        storeItemData.storeBGColour = new Color(red, green, blue, 0.4f);
                    }
                    else
                    {
                        storeItemData.storeBGColour = new Color(red, green, blue);
                    }
                }
                storeItem.Initialize(storeItemData);
                shownStoreItems.Add(storeItemData.itemType);
            }
        }
    }
}

