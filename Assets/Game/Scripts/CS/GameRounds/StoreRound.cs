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
        private Transform columnStoreItemHolderParent;

        [SerializeField]
        private Transform hatRowStoreItemHolderParent;

        [SerializeField]
        private GameObject storeItemPrefab;

        [SerializeField]
        private int baseNumberOfStoreItems;

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
        private BirdImage unlockerImage;

        [SerializeField]
        private StoreUnlockChoice optionA, optionB;

        [SerializeField]
        private List<BirdHatData.HatType> availableHats = new List<BirdHatData.HatType>();

        [SerializeField]
        private StoreBossArm storeBossArm;

        [SerializeField]
        private GameObject middleRowBGObject, bottomRowBGObject;

        [SerializeField]
        private Color unlockedRowColour;

        [SerializeField]
        private GameObject middleRowUnlockButtonObject, bottomRowUnlockButtonObject;

        [SerializeField]
        private TMPro.TMP_Text middleRowUnlockCostText, bottomRowUnlockCostText;

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

        private int middleUnlockCost, bottomUnlockCost;

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
        private int storeItemAdditions;
        private int currentStoreTier = 1;


        public override void StartRound()
        {
            base.StartRound();
            currentRestockCost = SettingsManager.Instance.gameMode.baseRestockCost;
            Cursor.visible = false;
            restockParentObject.SetActive(false);
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

            middleUnlockCost = SettingsManager.Instance.gameMode.middleStoreRowUnlockCost;
            bottomUnlockCost = SettingsManager.Instance.gameMode.bottomStoreRowUnlockCost;
            middleRowUnlockCostText.text = middleUnlockCost.ToString();
            bottomRowUnlockCostText.text = bottomUnlockCost.ToString();

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
            _currentState = State.store;

            GameManager.Instance.gameFlowManager.timeRemainingInPhase = storeTime;
            GameManager.Instance.gameDataHandler.RpcUpdateTimer(storeTime);
            CreateStoreItems();
            GameManager.Instance.gameDataHandler.RpcInitializeStore();
        }

        public void ServerInitializeUnlocks()
        {
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


            int numberOfPlayers = GameManager.Instance.gameFlowManager.GetNumberOfConnectedPlayers();

            //Add options to the pool that should be there for this day
            DayData currentDay = SettingsManager.Instance.gameMode.days[GameManager.Instance.playerFlowManager.currentDay];
            List<string> caseTypesToAdd = currentDay.caseTypesToAddToPool;
            GameManager.Instance.playerFlowManager.caseChoiceUnlockPool.AddRange(caseTypesToAdd);

            //Generate the options
            List<string> pool = new List<string>(GameManager.Instance.playerFlowManager.caseChoiceUnlockPool);

            List<string> choiceAOptions = new List<string>();
            for(int i = 0; i < currentDay.numberOfCaseTypeUnlocks; i++)
            {
                //Randomize the order
                pool = pool.OrderBy(x => System.Guid.NewGuid()).ToList();
                
                for(int j = pool.Count - 1; j >= 0; j--)
                {
                    CaseChoiceData choice = GameDataManager.Instance.GetCaseChoice(pool[j]);
                    if (choice != null && choice.numberOfTasks <= numberOfPlayers)
                    {
                        choiceAOptions.Add(choice.identifier);
                        pool.RemoveAt(j);
                        break;
                    }
                }
                
            }

            List<string> choiceBOptions = new List<string>();
            for (int i = 0; i < currentDay.numberOfCaseTypeUnlocks; i++)
            {
                //Randomize the order
                pool = pool.OrderBy(x => System.Guid.NewGuid()).ToList();
                for (int j = pool.Count - 1; j >= 0; j--)
                {
                    CaseChoiceData choice = GameDataManager.Instance.GetCaseChoice(pool[j]);
                    
                    if (choice != null && choice.numberOfTasks <= numberOfPlayers)
                    {
                        choiceBOptions.Add(choice.identifier);
                        pool.RemoveAt(j);
                        break;
                    }
                }
            }

            //Send relevant information to all players
            GameManager.Instance.gameDataHandler.RpcInitializeStoreUnlock(choiceAOptions, choiceBOptions, unionRep, Random.Range(0, 2) > 0);

            GameManager.Instance.gameFlowManager.timeRemainingInPhase = unlockTime;
            GameManager.Instance.gameDataHandler.RpcUpdateTimer(unlockTime);
        }

        public void ClientInitializeUnlocks(List<string> optionAChoices, List<string> optionBChoices, BirdName unionRep, bool defaultChoiceA)
        {
            _currentState = State.unlock;
            optionA.Initialize(optionAChoices, unionRep);
            optionB.Initialize(optionBChoices, unionRep);

            if(SettingsManager.Instance.birdName == unionRep)
            {
                unlocksInstructionText.text = "Choose one of the options to unlock for all players.";
            }
            else
            {
                unlocksInstructionText.text = "Waiting for " + GameManager.Instance.playerFlowManager.playerNameMap[unionRep] + " to choose an option.";
            }
            BirdData unlockerBird = GameDataManager.Instance.GetBird(unionRep);
            if(unlockerBird != null)
            {
                BirdHatData.HatType birdHat = GameManager.Instance.playerFlowManager.GetBirdHatType(unionRep);
                unlockerImage.Initialize(unionRep, birdHat);
            }
            
            inventoryHolderObject.SetActive(false);
            unlocksHolderObject.SetActive(true);
            defaultChoice = defaultChoiceA ? optionA : optionB;
            GameManager.Instance.playerFlowManager.currentTimeInRound = unlockTime;
            hasStartedReaching = false;
            
        }

        public void ClientInitializeStore()
        {
            _currentState = State.store;
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
            storeBossArm.CancelReach();
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
            foreach(Transform child in columnStoreItemHolderParent)
            {
                existingStoreItems.Add(child);
            }
            foreach (Transform child in hatRowStoreItemHolderParent)
            {
                existingStoreItems.Add(child);
            }
            for (int i = existingStoreItems.Count - 1; i >= 0; i--)
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
            GameDataManager.Instance.ResetCurrentShowingItems();
            int iterator = 0;

            int numberOfStoreItems = baseNumberOfStoreItems + storeItemAdditions;

            //Create the rest of the store items
            for (int i = iterator; i < numberOfStoreItems; i++)
            {
                CreateStoreItemData(i);
            }

            //Try to create two upgrades and two frequency items
            CreateColumnStoreItemData(100);
            if(currentStoreTier >= 2)
            {
                CreateColumnStoreItemData(101);
            }
            if(currentStoreTier >= 3)
            {
                CreateColumnStoreItemData(102);
            }
            int playerCount = SettingsManager.Instance.GetPlayerNameCount();
            if (playerCount == 2)
            {
                CreateUnlockStoreItemData(103);
            }

            CreateHatStoreItemData(104);
            if(playerCount > 2)
            {
                CreateHatStoreItemData(105);
            }
            if (playerCount > 5)
            {
                CreateHatStoreItemData(106);
            }
        }

        public void CreateRestockItem()
        {
            for(int i = 0; i < storeItems.Count; i++)
            {
                if (storeItems[i].index < 100 && storeItems[i].currentState == StoreItem.State.out_of_stock)
                {
                    CreateStoreItemData(i);
                    return;
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
            CaseUnlockStoreItemData unlockItem = new CaseUnlockStoreItemData();
            unlockItem.Initialize(storeItemData);
            unlockItem.index = index;
            activeStoreItemMap.Add(index, unlockItem);
            GameManager.Instance.gameDataHandler.RpcSendUnlockStoreItemWrapper(unlockItem);
            return true;
        }

        private bool CreateColumnStoreItemData(int index)
        {
            if(Random.Range(0,3) == 0)
            {
                return CreateUpgradeStoreItemData(index);
            }
            else
            {
                return CreateFrequencyStoreItemData(index);
            }
        }

        private bool CreateUpgradeStoreItemData(int index)
        {
            CaseUpgradeStoreItemData storeItemData = GameDataManager.Instance.GetUpgradeStoreItem(currentStoreTier);
            if(storeItemData == null)
            {
                //.LogError("No upgrade store items could be generated.");
                return false;
            }

            CaseUpgradeStoreItemData newStoreItem = new CaseUpgradeStoreItemData();
            newStoreItem.Initialize(storeItemData);
            newStoreItem.index = index;
            activeStoreItemMap.Add(index, newStoreItem);
            GameManager.Instance.gameDataHandler.RpcSendUpgradeStoreItemWrapper(newStoreItem);
            return true;
        }

        private bool CreateFrequencyStoreItemData(int index)
        {
            CaseFrequencyStoreItemData storeItemData = GameDataManager.Instance.GetFrequencyStoreItem(currentStoreTier);
            if(storeItemData == null)
            {
                return false;
            }

            CaseFrequencyStoreItemData newStoreItem = new CaseFrequencyStoreItemData();
            newStoreItem.Initialize(storeItemData);
            newStoreItem.index = index;
            activeStoreItemMap.Add(index, newStoreItem);
            GameManager.Instance.gameDataHandler.RpcSendFrequencyStoreItemWrapper(newStoreItem);
            return true;
        }

        private bool CreateHatStoreItemData(int index)
        {
            HatStoreItemData hatItemData = GameDataManager.Instance.GetHatStoreItem();

            if (hatItemData == null)
            {
                return false;
            }

            HatStoreItemData newHatItemData = new HatStoreItemData();
            newHatItemData.Initialize(hatItemData);
            newHatItemData.hatType = availableHats[Random.Range(0, availableHats.Count)];
            newHatItemData.index = index;
            activeStoreItemMap.Add(index, newHatItemData);
            GameManager.Instance.gameDataHandler.RpcSendHatStoreItemWrapper(newHatItemData);
            return true;
        }

        private void CreateStoreItemData(int index)
        {
            StoreItemData oldStoreItemData = GameDataManager.Instance.GetStoreItem(soldStoreItems, currentStoreTier);
            
            if (oldStoreItemData.itemType == StoreItem.StoreItemType.marker || oldStoreItemData.itemType == StoreItem.StoreItemType.highlighter)
            {
                MarkerStoreItemData markerItem = new MarkerStoreItemData();
                markerItem.Initialize(oldStoreItemData);
                markerItem.index = index;
                activeStoreItemMap.Add(index, markerItem);
                //Generate a colour and set the value for the BG colour and the marker colour
                Color randomColour = new Color(Random.Range(0.4f, 1f), Random.Range(0.4f, 1f), Random.Range(0.4f, 1f));
                    
                float red = randomColour.r;
                float green = randomColour.g;
                float blue = randomColour.b;
                red = (1 - red) * 0.5f + red;
                green = (1 - green) * 0.5f + green;
                blue = (1 - blue) * 0.5f + blue;
                markerItem.storeBGColour = new Color(red, green, blue, 1f);
                if (oldStoreItemData.itemType == StoreItem.StoreItemType.highlighter)
                {
                    randomColour = new Color(randomColour.r, randomColour.g, randomColour.b, 0.4f);
                }
                
                markerItem.markerColour = randomColour;
                //Broadcast to players to create
                GameManager.Instance.gameDataHandler.RpcSendMarkerStoreItemWrapper(markerItem);
            }
            else if(oldStoreItemData.itemType == StoreItem.StoreItemType.case_tab || oldStoreItemData.itemType == StoreItem.StoreItemType.coffee_pot ||
                    oldStoreItemData.itemType == StoreItem.StoreItemType.coffee_mug || oldStoreItemData.itemType == StoreItem.StoreItemType.nest_feathering ||
                    oldStoreItemData.itemType == StoreItem.StoreItemType.advertisement || oldStoreItemData.itemType == StoreItem.StoreItemType.contract)
            {
                //Broadcast to players to create
                ValueStoreItemData valueItem = new ValueStoreItemData();
                valueItem.Initialize(oldStoreItemData);
                valueItem.index = index;
                activeStoreItemMap.Add(index, valueItem);
                GameManager.Instance.gameDataHandler.RpcSendValueStoreItemWrapper(valueItem);
            }
            else if(oldStoreItemData.itemType == StoreItem.StoreItemType.reroll || oldStoreItemData.itemType == StoreItem.StoreItemType.stopwatch)
            {
                //Broadcast to players to create
                ChargedStoreItemData chargeItem = new ChargedStoreItemData();
                chargeItem.Initialize(oldStoreItemData);
                chargeItem.index = index;
                activeStoreItemMap.Add(index, chargeItem);
                GameManager.Instance.gameDataHandler.RpcSendChargeStoreItemWrapper(chargeItem);
            }
            else if(oldStoreItemData.itemType == StoreItem.StoreItemType.hat)
            {
                //Randomly choose a hat
                HatStoreItemData hatItem = new HatStoreItemData();
                hatItem.Initialize(oldStoreItemData);
                hatItem.index = index;
                activeStoreItemMap.Add(index, hatItem);
                hatItem.hatType = availableHats[Random.Range(0, availableHats.Count)];
                GameManager.Instance.gameDataHandler.RpcSendHatStoreItemWrapper(hatItem);
            }
            else if(oldStoreItemData.itemType == StoreItem.StoreItemType.case_upgrade)
            {
                Debug.LogError("Something funky is happening???");
                CaseUpgradeStoreItemData upgradeItem = new CaseUpgradeStoreItemData();
                upgradeItem.Initialize(oldStoreItemData);
                upgradeItem.index = index;
                activeStoreItemMap.Add(index, upgradeItem);

                //Broadcast to players to create
                GameManager.Instance.gameDataHandler.RpcSendUpgradeStoreItemWrapper(upgradeItem);
            }
            else
            {
                StoreItemData newStoreItemData = new StoreItemData();
                newStoreItemData.Initialize(oldStoreItemData);
                newStoreItemData.index = index;
                activeStoreItemMap.Add(index, newStoreItemData);

                //Broadcast to players to create
                GameManager.Instance.gameDataHandler.RpcSendStoreItemWrapper(newStoreItemData);
            }

            
        }

        public void CreateStoreItem(StoreItemData storeItemData)
        {
            bool restockingItem = false;
            foreach(StoreItem existingStoreItem in storeItems)
            {
                if(existingStoreItem.index == storeItemData.index)
                {
                    restockingItem = true;
                    existingStoreItem.Initialize(storeItemData);
                }
            }
            if(!restockingItem)
            {
                StoreItem storeItem;
                Transform spawningParent = storeItemHolderParent;
                if (storeItemData.itemType == StoreItem.StoreItemType.case_upgrade ||
                    storeItemData.itemType == StoreItem.StoreItemType.case_frequency ||
                    storeItemData.itemType == StoreItem.StoreItemType.case_unlock)
                {
                    spawningParent = columnStoreItemHolderParent;
                }
                if(storeItemData.itemType == StoreItem.StoreItemType.hat)
                {
                    spawningParent = hatRowStoreItemHolderParent;
                }
                GameObject storeItemObject = Instantiate(storeItemPrefab, spawningParent);
                storeItem = storeItemObject.GetComponent<StoreItem>();
                storeItem.Initialize(storeItemData);
                storeItems.Add(storeItem);
            }
            bool anItemIsOutOfStock = false;
            //If all store items are restocked then hide the restock button
            foreach(StoreItem currentItem in storeItems)
            {
                if(currentItem.currentState == StoreItem.State.out_of_stock && currentItem.index < 100)
                {
                    anItemIsOutOfStock = true;
                }
            }
            if(!anItemIsOutOfStock)
            {
                restockParentObject.SetActive(false);
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

                        foreach (StoreItemData upgrade in upgradeData.unlocks)
                        {
                            if(upgrade.itemType == StoreItem.StoreItemType.case_upgrade)
                            {
                                GameDataManager.Instance.AddUpgradeOption((CaseUpgradeStoreItemData)upgrade);
                            }
                            else if(upgrade.itemType == StoreItem.StoreItemType.case_frequency)
                            {
                                GameDataManager.Instance.AddFrequencyOption((CaseFrequencyStoreItemData)upgrade);
                            }
                        }
                        GameDataManager.Instance.UpgradeCaseChoice(upgradeData);
                        GameManager.Instance.gameDataHandler.RpcUpgradeCaseChoice(activeStoreItemMap[itemIndex].itemName);
                        GameDataManager.Instance.RemoveUpgrade(upgradeData);
                        break;
                    case StoreItem.StoreItemType.case_frequency:
                        CaseFrequencyStoreItemData frequencyData = (CaseFrequencyStoreItemData)activeStoreItemMap[itemIndex];
                        CaseChoiceData caseChoice = GameDataManager.Instance.GetCaseChoice(frequencyData.caseChoiceIdentifier);
                        if(caseChoice != null)
                        {
                            caseChoice.selectionFrequency += frequencyData.frequencyIncrease;
                            GameManager.Instance.gameDataHandler.RpcIncreaseCaseChoiceFrequency(activeStoreItemMap[itemIndex].itemName);
                            GameDataManager.Instance.RemoveFrequencyItem(frequencyData);
                        }
                        
                        foreach (StoreItemData upgrade in frequencyData.unlocks)
                        {
                            if (upgrade.itemType == StoreItem.StoreItemType.case_upgrade)
                            {
                                GameDataManager.Instance.AddUpgradeOption((CaseUpgradeStoreItemData)upgrade);
                            }
                            else if (upgrade.itemType == StoreItem.StoreItemType.case_frequency)
                            {
                                GameDataManager.Instance.AddFrequencyOption((CaseFrequencyStoreItemData)upgrade);
                            }
                        }
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
                    case StoreItem.StoreItemType.contract:
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

                if(itemType == StoreItem.StoreItemType.case_upgrade || itemType == StoreItem.StoreItemType.case_frequency)
                {
                    CreateColumnStoreItemData(itemIndex);
                }
                else if(itemType == StoreItem.StoreItemType.case_unlock)
                {
                    CreateUnlockStoreItemData(itemIndex);
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
                    if(storeItem.index < 100)
                    {
                        restockParentObject.SetActive(true);
                        restockCostText.text = currentRestockCost.ToString();
                    }
                    
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
                StatTracker.Instance.totalSpent += currentRestockCost;
                StatTracker.Instance.storeRestocks++;
                
                _currentMoney -= currentRestockCost;
                currentMoneyText.text = _currentMoney.ToString();
                GameManager.Instance.gameDataHandler.CmdRequestRestock();
            }
        }

        public bool HasChosen()
        {
            return optionA.hasChosen || optionB.hasChosen;
        }

        public void OnUnlockMiddleRowPress()
        {
            if(_currentMoney < middleUnlockCost)
            {
                ShowUnaffordableNotification();
                return;
            }

            _currentMoney -= middleUnlockCost;
            currentMoneyText.text = _currentMoney.ToString();
            GameManager.Instance.gameDataHandler.CmdUnlockMiddleStoreRow();
        }

        public void ServerUnlockRow()
        {
            int previousNumberOfStoreItems = baseNumberOfStoreItems + storeItemAdditions;
            storeItemAdditions += 4;
            currentStoreTier++;
            int numberOfStoreItems = baseNumberOfStoreItems + storeItemAdditions;
            for (int i = previousNumberOfStoreItems; i < numberOfStoreItems; i++)
            {
                CreateStoreItemData(i);
            }
        }

        public void UnlockMiddleRow()
        {
            middleRowBGObject.GetComponent<Image>().color = unlockedRowColour;
            middleRowUnlockButtonObject.SetActive(false);
            bottomRowUnlockButtonObject.SetActive(true);
        }

        public void OnUnlockBottomRowPress()
        {
            if (_currentMoney < bottomUnlockCost)
            {
                ShowUnaffordableNotification();
                return;
            }

            _currentMoney -= bottomUnlockCost;
            currentMoneyText.text = _currentMoney.ToString();
            GameManager.Instance.gameDataHandler.CmdUnlockBottomStoreRow();
        }

        public void UnlockBottomRow()
        {
            bottomRowBGObject.GetComponent<Image>().color = unlockedRowColour;
            bottomRowUnlockButtonObject.SetActive(false);
        }
    }
}

