using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ChickenScratch.ColourManager;

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

        [SerializeField]
        private Transform playerLongArmHolder;

        [SerializeField]
        private Transform storeLongArmHolder;

        [SerializeField]
        private float armUpdateFrequency;

        [SerializeField]
        private string unaffordableMessage;

        [SerializeField]
        private string alreadyHaveMessage;

        [SerializeField]
        private GameObject waitingOnPlayerPrefab;

        [SerializeField]
        private Transform waitingOnPlayerHolder;

        public int currentMoney => _currentMoney;

        private int _currentMoney;
        private float timeShowingUnaffordableNotification = 0f;

        private List<StoreItem.StoreItemType> soldStoreItems = new List<StoreItem.StoreItemType>();
        private List<ColourManager.BirdName> activePlayers = new List<ColourManager.BirdName>();
        private Dictionary<BirdName, GameObject> waitingOnPlayersObjectMap = new Dictionary<BirdName, GameObject>();
        private Dictionary<BirdName, Vector3> longArmPositionMap = new Dictionary<BirdName, Vector3>();
        private Dictionary<BirdName, StretchArm> longArmMap = new Dictionary<BirdName, StretchArm>();

        private Dictionary<int, StoreItemData> activeStoreItemMap = new Dictionary<int, StoreItemData>();
        private List<StoreItem> storeItems = new List<StoreItem>();


        private float timeSinceLastArmUpdate = 0f;

        public override void StartRound()
        {
            base.StartRound();
            ClearStoreItems();
            ClearPlayerWaitingVisuals();
            List<BirdName> allPlayers = SettingsManager.Instance.GetAllActiveBirds();
            waitingOnPlayersObjectMap.Clear();
            foreach (BirdName bird in allPlayers)
            {
                GameObject waitingOnPlayerObject = Instantiate(waitingOnPlayerPrefab, waitingOnPlayerHolder);
                waitingOnPlayersObjectMap.Add(bird, waitingOnPlayerObject);
                WaitingOnPlayerVisual waitingOnPlayerVisual = waitingOnPlayerObject.GetComponent<WaitingOnPlayerVisual>();
                waitingOnPlayerVisual.Initialize(bird);
            }

            if (SettingsManager.Instance.isHost)
            {
                activePlayers.Clear();
                activePlayers.AddRange(allPlayers);
                GameManager.Instance.gameFlowManager.timeRemainingInPhase = timeInRound;
                CreateStoreItems();
            }

            GenerateLongArms();
            
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
            currentMoneyText.text = _currentMoney.ToString();
        }

        public void DecreaseCurrentMoney(int money)
        {
            _currentMoney -= money;
            currentMoneyText.text = _currentMoney.ToString();
        }

        public void ShowUnaffordableNotification()
        {
            unaffordableNotificationText.text = unaffordableMessage;
            unaffordableNotificationText.gameObject.SetActive(true);
            timeShowingUnaffordableNotification = Time.deltaTime;
        }

        public void ShowAlreadyHaveNotification()
        {
            unaffordableNotificationText.text = alreadyHaveMessage;
            unaffordableNotificationText.gameObject.SetActive(true);
            timeShowingUnaffordableNotification = Time.deltaTime;
        }

        public void FinishWithStoreForPlayer(ColourManager.BirdName player)
        {
            GameManager.Instance.gameDataHandler.RpcRemoveStoreWaitingForPlayerVisual(player);
            if(activePlayers.Contains(player))
            {
                activePlayers.Remove(player);
                if(activePlayers.Count == 0)
                {
                    GameManager.Instance.gameFlowManager.timeRemainingInPhase = 0f;
                }
            }
        }

        public void RemoveWaitingForPlayerVisual(BirdName player)
        {
            if (waitingOnPlayersObjectMap.ContainsKey(player))
            {
                Destroy(waitingOnPlayersObjectMap[player]);
                waitingOnPlayersObjectMap.Remove(player);
            }
        }

        public void Close()
        {
            GameManager.Instance.gameDataHandler.CmdFinishWithStore(SettingsManager.Instance.birdName);
        }

        public void ClearStoreItems()
        {
            storeItems.Clear();
            activeStoreItemMap.Clear();
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

        public void ClearPlayerWaitingVisuals()
        {
            List<Transform> transformsToDestroy = new List<Transform>();
            foreach(Transform child in waitingOnPlayerHolder)
            {
                transformsToDestroy.Add(child);
            }
            for(int i = transformsToDestroy.Count - 1;i >= 0;i--)
            {
                Destroy(transformsToDestroy[i].gameObject);
            }
        }

        public void ClearLongArms()
        {
            longArmMap.Clear();
            List<Transform> existingLongArms = new List<Transform>();
            foreach (Transform child in playerLongArmHolder)
            {
                existingLongArms.Add(child);
            }
            foreach (Transform child in storeLongArmHolder)
            {
                existingLongArms.Add(child);
            }
            for (int i = existingLongArms.Count - 1; i >= 0; i--)
            {
                GameObject.Destroy(existingLongArms[i].gameObject);
            }
        }

        public void SetLongArmPosition(BirdName player, Vector3 inPosition)
        {
            if (!longArmPositionMap.ContainsKey(player))
            {
                longArmPositionMap.Add(player, inPosition);
            }
            else
            {
                longArmPositionMap[player] = inPosition;
            }
        }

        public void GenerateLongArms()
        {
            ClearLongArms();
            List<BirdName> allActiveBirds = SettingsManager.Instance.GetAllActiveBirds();
            foreach(BirdName birdName in allActiveBirds)
            {
                BirdData bird = GameDataManager.Instance.GetBird(birdName);
                if(bird == null)
                {
                    Debug.LogError("Could not generate long arm for player["+birdName.ToString()+"] because they are not mapped in the Colour Manager.");
                    continue;
                }

                if(birdName == SettingsManager.Instance.birdName)
                {
                    Instantiate(bird.storePlayerBirdArmPrefab, playerLongArmHolder);
                }
                else
                {
                    GameObject longArmObject = Instantiate(bird.storeBirdArmPrefab, storeLongArmHolder);
                    StretchArm longArm = longArmObject.GetComponent<StretchArm>();
                    if(longArm != null && !longArmMap.ContainsKey(longArm.birdName))
                    {
                        longArmMap.Add(longArm.birdName, longArm);
                    }
                    
                }
            }
        }

        public void UpdatePhase()
        {
            timeSinceLastArmUpdate += Time.deltaTime;

            if (timeSinceLastArmUpdate > armUpdateFrequency)
            {
                if (longArmPositionMap != null && longArmPositionMap.Count > 0)
                {
                    //Broadcast arm positions to all other clients
                    GameManager.Instance.gameDataHandler.RpcStorePhasePositionsWrapper(longArmPositionMap);

                    timeSinceLastArmUpdate = 0.0f;
                }

            }
        }

        public StretchArm GetLongArm(BirdName inBirdName)
        {
            if(longArmMap.ContainsKey(inBirdName))
            {
                return longArmMap[inBirdName];
            }
            return null;
        }

        public void SetLongArmTargetPosition(BirdName inBirdName, Vector3 inPosition)
        {
            if (inBirdName == SettingsManager.Instance.birdName)
            {
                return;
            }
            
            StretchArm birdArm = GetLongArm(inBirdName);
            if (birdArm != null)
            {
                birdArm.targetPosition = inPosition;
            }
        }

        public void CreateStoreItems()
        {
            //
            int iterator = 0;
            //Try to create an unlock
            if(CreateUnlockStoreItemData(iterator))
            {
                iterator++;
            }

            //Try to create an upgrade
            if(CreateUpgradeStoreItemData(iterator))
            {
                iterator++;
            }

            //Create store items
            for (int i = iterator; i < numberOfStoreItems; i++)
            {
                CreateStoreItemData(i);
            }
        }

        private bool CreateUnlockStoreItemData(int index)
        {
            CaseUnlockStoreItemData storeItemData = GameDataManager.Instance.GetUnlockStoreItem();
            if(storeItemData == null)
            {
                return false;
            }
            storeItemData.index = index;
            activeStoreItemMap.Add(index, storeItemData);
            GameManager.Instance.gameDataHandler.RpcSendUnlockStoreItemWrapper(storeItemData);
            return true;
        }

        private bool CreateUpgradeStoreItemData(int index)
        {
            CaseUpgradeStoreItemData storeItemData = GameDataManager.Instance.GetUpgradeStoreItem();
            if(storeItemData == null)
            {
                return false;
            }
            storeItemData.index = index;
            activeStoreItemMap.Add(index, storeItemData);
            GameManager.Instance.gameDataHandler.RpcSendUpgradeStoreItemWrapper(storeItemData);
            return true;
        }

        private void CreateStoreItemData(int index)
        {
            StoreItemData storeItemData = GameDataManager.Instance.GetStoreItem(soldStoreItems);
            storeItemData.index = index;
            activeStoreItemMap.Add(index, storeItemData);
            if (storeItemData.itemType == StoreItem.StoreItemType.marker || storeItemData.itemType == StoreItem.StoreItemType.highlighter)
            {
                //Generate a colour and set the value for the BG colour and the marker colour
                Color randomColour = new Color(Random.Range(0.4f, 1f), Random.Range(0.4f, 1f), Random.Range(0.4f, 1f));
                    
                float red = randomColour.r;
                float green = randomColour.g;
                float blue = randomColour.b;
                red = (1 - red) * 0.5f + red;
                green = (1 - green) * 0.5f + green;
                blue = (1 - blue) * 0.5f + blue;
                storeItemData.storeBGColour = new Color(red, green, blue, 1f);
                if (storeItemData.itemType == StoreItem.StoreItemType.highlighter)
                {
                    randomColour = new Color(randomColour.r, randomColour.g, randomColour.b, 0.4f);
                }
                MarkerStoreItemData markerItem = ((MarkerStoreItemData)storeItemData);
                markerItem.markerColour = randomColour;
                //Broadcast to players to create
                GameManager.Instance.gameDataHandler.RpcSendMarkerStoreItemWrapper(markerItem);
            }
            else if(storeItemData.itemType == StoreItem.StoreItemType.case_tab)
            {
                //Broadcast to players to create
                ValueStoreItemData valueItem = ((ValueStoreItemData)storeItemData);
                GameManager.Instance.gameDataHandler.RpcSendValueStoreItemWrapper(valueItem);
            }
            else if(storeItemData.itemType == StoreItem.StoreItemType.reroll || storeItemData.itemType == StoreItem.StoreItemType.stopwatch)
            {
                //Broadcast to players to create
                ChargedStoreItemData chargeItem = ((ChargedStoreItemData)storeItemData);
                GameManager.Instance.gameDataHandler.RpcSendChargeStoreItemWrapper(chargeItem);
            }
            else
            {
                //Broadcast to players to create
                GameManager.Instance.gameDataHandler.RpcSendStoreItemWrapper(storeItemData);
            }

            
        }

        public void CreateStoreItem(StoreItemData storeItemData)
        {
            GameObject storeItemObject = Instantiate(storeItemPrefab, storeItemHolderParent);
            StoreItem storeItem = storeItemObject.GetComponent<StoreItem>();
            storeItem.Initialize(storeItemData);
            storeItems.Add(storeItem);
        }

        public void HandleClientRequestItem(BirdName client, int itemIndex)
        {
            if(activeStoreItemMap.ContainsKey(itemIndex))
            {
                if (activeStoreItemMap[itemIndex].itemType == StoreItem.StoreItemType.case_unlock)
                {
                    CaseUnlockStoreItemData unlockData = (CaseUnlockStoreItemData)activeStoreItemMap[itemIndex];
                    GameDataManager.Instance.UnlockCaseChoice(unlockData);
                }
                else if (activeStoreItemMap[itemIndex].itemType == StoreItem.StoreItemType.case_upgrade)
                {
                    GameManager.Instance.gameDataHandler.RpcUpgradeCaseChoice(activeStoreItemMap[itemIndex].itemName);
                }
                activeStoreItemMap.Remove(itemIndex);
                GameManager.Instance.gameDataHandler.RpcPurchaseStoreItem(client, itemIndex);
            }
        }

        public void PurchaseStoreItem(BirdName purchaser, int itemIndex)
        {
            foreach(StoreItem storeItem in storeItems)
            {
                if(storeItem.index == itemIndex)
                {
                    storeItem.Purchase(purchaser);
                }
            }
        }
    }
}

