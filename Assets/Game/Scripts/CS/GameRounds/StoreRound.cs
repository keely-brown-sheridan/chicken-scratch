using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ChickenScratch.ColourManager;
using UnityEngine.UI;
using System.Linq;

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
        private string drawingToolMessage;

        [SerializeField]
        private GameObject waitingOnPlayerPrefab;

        [SerializeField]
        private Transform waitingOnPlayerHolder;

        [SerializeField]
        private BirdImage playerFaceImage;

        [SerializeField]
        private GameObject restockParentObject;

        [SerializeField]
        private TMPro.TMP_Text restockCostText;

        [SerializeField]
        private GameObject readyButtonObject;

        [SerializeField]
        private GameObject inventoryHolderObject;

        [SerializeField]
        private GameObject unlocksHolderObject;

        [SerializeField]
        private TMPro.TMP_Text unlocksInstructionText;

        [SerializeField]
        private StoreUnlockChoice optionA, optionB;

        [SerializeField]
        private List<BirdHatData.HatType> availableHats = new List<BirdHatData.HatType>();

        [SerializeField]
        private StoreBossArm storeBossArm;

        public enum State
        {
            unlock, store
        }
        public State currentState => _currentState;
        private State _currentState = State.store;

        public float unlockTime => _unlockTime;
        [SerializeField]
        private float _unlockTime;

        public float storeTime => _storeTime;
        [SerializeField]
        private float _storeTime;

        [SerializeField]
        private float startReachingTime;

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

        private int currentRestockCost = 0;
        private float timeSinceLastArmUpdate = 0f;
        private bool hasStartedReaching = false;
        private StoreUnlockChoice defaultChoice;

        public override void StartRound()
        {
            base.StartRound();
            ResetRestock();
            readyButtonObject.SetActive(true);
            ClearStoreItems();
            ClearPlayerWaitingVisuals();
            List<BirdName> allPlayers = SettingsManager.Instance.GetAllActiveBirds();
            waitingOnPlayersObjectMap.Clear();


            if(allPlayers.Count > 2)
            {
                //We have enough for the unlocks phase
                if(SettingsManager.Instance.isHost)
                {
                    ServerInitializeUnlocks();
                }
            }
            else
            {
                _currentState = State.store;
                if (SettingsManager.Instance.isHost)
                {
                    ServerInitializeStore();
                }
            }

            

            //Set the face of the player
            BirdData playerBird = GameDataManager.Instance.GetBird(SettingsManager.Instance.birdName);
            if(playerBird != null)
            {
                BirdHatData.HatType birdHat = GameManager.Instance.playerFlowManager.GetBirdHatType(SettingsManager.Instance.birdName);
                playerFaceImage.Initialize(SettingsManager.Instance.birdName, birdHat);
            }
            

            GenerateLongArms();
            
            storeObject.SetActive(true);
        }

        public void ForceChoice()
        {
            defaultChoice.Choose(false);
        }

        public void ServerInitializeStore()
        {
            Debug.LogError("Server initializing store.");
            _currentState = State.store;

            GameManager.Instance.gameFlowManager.timeRemainingInPhase = storeTime;
            GameManager.Instance.gameDataHandler.RpcUpdateTimer(storeTime);
            CreateStoreItems();
            GameManager.Instance.gameDataHandler.RpcInitializeStore();
        }

        public void ServerInitializeUnlocks()
        {
            Debug.LogError("Server initializing unlocks.");
            _currentState = State.unlock;
            //Choose the player in charge of selecting the choice
            //Who has the most stars from the previous round?
            BirdName unionRep = BirdName.none;
            
            List<BirdName> allBirds = SettingsManager.Instance.GetAllActiveBirds();
            Dictionary<BirdName, int> birdStarMap = new Dictionary<BirdName, int>();
            foreach(BirdName bird in allBirds)
            {
                birdStarMap.Add(bird, 0);
            }

            foreach(EndgameCaseData caseData in GameManager.Instance.playerFlowManager.slidesRound.caseDataMap.Values)
            {
                foreach(EndgameTaskData task in caseData.taskDataMap.Values)
                {
                    if(birdStarMap.ContainsKey(task.ratingData.target))
                    {
                        birdStarMap[task.ratingData.target] += task.ratingData.likeCount;
                    }
                }
            }
            int mostLikesSoFar = -1;
            foreach (BirdName bird in allBirds)
            {
                if (birdStarMap[bird] > mostLikesSoFar)
                {
                    mostLikesSoFar = birdStarMap[bird];
                    unionRep = bird;
                }
            }

            //Add options to the pool that should be there for this day
            DayData currentDay = SettingsManager.Instance.gameMode.days[GameManager.Instance.playerFlowManager.currentDay];
            List<string> caseTypesToAdd = currentDay.caseTypesToAddToPool;
            GameManager.Instance.playerFlowManager.caseChoiceUnlockPool.AddRange(caseTypesToAdd);

            //Generate the options
            List<string> pool = GameManager.Instance.playerFlowManager.caseChoiceUnlockPool;

            List<string> choiceAOptions = new List<string>();
            for(int i = 0; i < currentDay.numberOfCaseTypeUnlocks; i++)
            {
                //Randomize the order
                pool = pool.OrderBy(x => System.Guid.NewGuid()).ToList();
                choiceAOptions.Add(pool[0]);
                pool.RemoveAt(0);
            }

            List<string> choiceBOptions = new List<string>();
            for (int i = 0; i < currentDay.numberOfCaseTypeUnlocks; i++)
            {
                //Randomize the order
                pool = pool.OrderBy(x => System.Guid.NewGuid()).ToList();
                choiceBOptions.Add(pool[0]);
                pool.RemoveAt(0);
            }

            //Send relevant information to all players
            GameManager.Instance.gameDataHandler.RpcInitializeStoreUnlock(choiceAOptions, choiceBOptions, unionRep, Random.Range(0, 2) > 0);

            GameManager.Instance.gameFlowManager.timeRemainingInPhase = unlockTime;
            GameManager.Instance.gameDataHandler.RpcUpdateTimer(unlockTime);
        }

        public void ClientInitializeUnlocks(List<string> optionAChoices, List<string> optionBChoices, BirdName unionRep, bool defaultChoiceA)
        {
            Debug.LogError("Initializing unlocks.");
            optionA.Initialize(optionAChoices, unionRep);
            optionB.Initialize(optionBChoices, unionRep);

            if(SettingsManager.Instance.birdName == unionRep)
            {
                unlocksInstructionText.text = "Choose one of the options to unlock for all players.";
            }
            else
            {
                unlocksInstructionText.text = "Waiting for " + unionRep.ToString() + " to choose an option.";
            }
            inventoryHolderObject.SetActive(false);
            unlocksHolderObject.SetActive(true);
            defaultChoice = defaultChoiceA ? optionA : optionB;
        }

        public void ClientInitializeStore()
        {
            Debug.LogError("Initializing store.");
            List<BirdName> allPlayers = SettingsManager.Instance.GetAllActiveBirds();
            foreach (BirdName bird in allPlayers)
            {
                GameObject waitingOnPlayerObject = Instantiate(waitingOnPlayerPrefab, waitingOnPlayerHolder);
                waitingOnPlayersObjectMap.Add(bird, waitingOnPlayerObject);
                WaitingOnPlayerVisual waitingOnPlayerVisual = waitingOnPlayerObject.GetComponent<WaitingOnPlayerVisual>();
                waitingOnPlayerVisual.Initialize(bird);
            }
            activePlayers.Clear();
            activePlayers.AddRange(allPlayers);
            inventoryHolderObject.SetActive(true);
            unlocksHolderObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            switch(currentState)
            {
                case State.unlock:
                    if(!hasStartedReaching && GameManager.Instance.playerFlowManager.currentTimeInRound <= startReachingTime)
                    {
                        
                        hasStartedReaching = true;
                        storeBossArm.StartReach(defaultChoice.buttonPosition, startReachingTime);
                    }
                    break;
            }
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

        public void ShowDrawingToolsNotification()
        {
            unaffordableNotificationText.text = drawingToolMessage;
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

        public void HideChoiceOptionButtons()
        {
            optionA.HideChoiceButton();
            optionB.HideChoiceButton();
        }

        public void Close()
        {
            readyButtonObject.SetActive(false);
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
            //Try to create an upgrade
            if(CreateUpgradeStoreItemData(iterator))
            {
                iterator++;
            }

            //Try to create a hat
            if (CreateHatStoreItemData(iterator))
            {
                iterator++;
            }

            //Create store items
            for (int i = iterator; i < numberOfStoreItems; i++)
            {
                CreateStoreItemData(i);
            }
        }

        public void CreateRestockItems()
        {
            for(int i = 0; i < storeItems.Count; i++)
            {
                if (storeItems[i].currentState == StoreItem.State.out_of_stock)
                {
                    CreateStoreItemData(i);
                }
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

        private bool CreateHatStoreItemData(int index)
        {
            HatStoreItemData hatItemData = (HatStoreItemData)GameDataManager.Instance.GetMatchingStoreItem(StoreItem.StoreItemType.hat);
            if (hatItemData == null)
            {
                return false;
            }
            hatItemData.hatType = availableHats[Random.Range(0, availableHats.Count)];
            hatItemData.index = index;
            activeStoreItemMap.Add(index, hatItemData);
            GameManager.Instance.gameDataHandler.RpcSendHatStoreItemWrapper(hatItemData);
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
            else if(storeItemData.itemType == StoreItem.StoreItemType.hat)
            {
                //Randomly choose a hat
                HatStoreItemData hatItem = ((HatStoreItemData)storeItemData);
                hatItem.hatType = availableHats[Random.Range(0, availableHats.Count)];
                GameManager.Instance.gameDataHandler.RpcSendHatStoreItemWrapper(hatItem);
            }
            else
            {
                //Broadcast to players to create
                GameManager.Instance.gameDataHandler.RpcSendStoreItemWrapper(storeItemData);
            }

            
        }

        public void CreateStoreItem(StoreItemData storeItemData)
        {
            StoreItem storeItem;
            if(storeItems.Count > storeItemData.index)
            {
                storeItem = storeItems[storeItemData.index];
                if(storeItem.currentState == StoreItem.State.out_of_stock)
                {
                    storeItem.Initialize(storeItemData);
                }
                else
                {
                    Debug.LogError("Trying to restock an item that already has stock.");
                }
            }
            else
            {
                GameObject storeItemObject = Instantiate(storeItemPrefab, storeItemHolderParent);
                storeItem = storeItemObject.GetComponent<StoreItem>();
                storeItem.Initialize(storeItemData);
                storeItems.Add(storeItem);
            }
            
        }

        public void HandleClientRequestItem(BirdName client, int itemIndex)
        {
            if(activeStoreItemMap.ContainsKey(itemIndex))
            {
                switch(activeStoreItemMap[itemIndex].itemType)
                {
                    case StoreItem.StoreItemType.case_unlock:
                        CaseUnlockStoreItemData unlockData = (CaseUnlockStoreItemData)activeStoreItemMap[itemIndex];
                        GameDataManager.Instance.UnlockCaseChoice(unlockData.caseChoiceIdentifier);
                        break;
                    case StoreItem.StoreItemType.case_upgrade:
                        CaseUpgradeStoreItemData upgradeData = (CaseUpgradeStoreItemData)activeStoreItemMap[itemIndex];
                        GameManager.Instance.gameDataHandler.RpcUpgradeCaseChoice(activeStoreItemMap[itemIndex].itemName);
                        break;
                    case StoreItem.StoreItemType.coffee_pot:
                        GameDataManager.Instance.RemoveStoreItemType(StoreItem.StoreItemType.coffee_pot);
                        ValueStoreItemData coffeePotData = (ValueStoreItemData)activeStoreItemMap[itemIndex];

                        GameManager.Instance.playerFlowManager.dailyTimeIncrease += coffeePotData.value;
                        break;
                    case StoreItem.StoreItemType.coffee_mug:
                        ValueStoreItemData coffeeMugData = (ValueStoreItemData)activeStoreItemMap[itemIndex];
                        GameManager.Instance.playerFlowManager.baseTimeIncrease += coffeeMugData.value;
                        break;
                    case StoreItem.StoreItemType.advertisement:
                        ValueStoreItemData advertisementData = (ValueStoreItemData)activeStoreItemMap[itemIndex];
                        GameManager.Instance.playerFlowManager.baseCasesIncrease += (int)advertisementData.value;
                        break;
                    case StoreItem.StoreItemType.nest_feathering:
                        ValueStoreItemData nestFeatheringData = (ValueStoreItemData)activeStoreItemMap[itemIndex];
                        GameManager.Instance.playerFlowManager.baseQuotaDecrement -= (int)nestFeatheringData.value;
                        break;
                    case StoreItem.StoreItemType.hat:
                        HatStoreItemData hatData = (HatStoreItemData)activeStoreItemMap[itemIndex];

                        //Broadcast to all players to update the hat for the player
                        GameManager.Instance.gameDataHandler.RpcSetPlayerHat(client, hatData.hatType);

                        //Remove that hat as an option for store items in the future
                        if(availableHats.Contains(hatData.hatType))
                        {
                            availableHats.Remove(hatData.hatType);
                        }
                        break;
                }

                soldStoreItems.Add(activeStoreItemMap[itemIndex].itemType);
                
                
                GameManager.Instance.gameDataHandler.RpcPurchaseStoreItem(client, itemIndex);
                StoreItem.StoreItemType itemType = activeStoreItemMap[itemIndex].itemType;
                activeStoreItemMap.Remove(itemIndex);

                if(itemType == StoreItem.StoreItemType.case_upgrade)
                {
                    CreateUpgradeStoreItemData(itemIndex);
                }
                
                
            }
        }

        public void PurchaseStoreItem(BirdName purchaser, int itemIndex)
        {
            foreach(StoreItem storeItem in storeItems)
            {
                if(storeItem.index == itemIndex)
                {
                    storeItem.Purchase(purchaser);
                    restockParentObject.SetActive(true);
                    
                    currentRestockCost += SettingsManager.Instance.gameMode.itemRestockCost;
                    restockCostText.text = currentRestockCost.ToString();
                    return;
                }
            }
        }

        public void RequestStoreRestock()
        {
            if(currentMoney < currentRestockCost)
            {
                ShowUnaffordableNotification();
            }
            else
            {
                restockParentObject.SetActive(false);
                StatTracker.Instance.totalSpent += currentRestockCost;
                StatTracker.Instance.storeRestocks++;

                bool areAllOutOfStock = true;
                foreach(StoreItem storeItem in storeItems)
                {
                    if(storeItem.currentState == StoreItem.State.in_stock)
                    {
                        areAllOutOfStock = false;
                        break;
                    }
                }
                if(areAllOutOfStock)
                {
                    StatTracker.Instance.restockedEmptyShop = true;
                }

                _currentMoney -= currentRestockCost;
                currentMoneyText.text = _currentMoney.ToString();
                GameManager.Instance.gameDataHandler.CmdRequestRestock();
            }
        }

        public void ResetRestock()
        {
            restockParentObject.SetActive(false);
            currentRestockCost = SettingsManager.Instance.gameMode.baseRestockCost;
            restockCostText.text = currentRestockCost.ToString();
        }
    }
}

