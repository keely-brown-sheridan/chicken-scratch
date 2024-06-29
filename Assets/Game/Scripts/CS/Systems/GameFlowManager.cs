using Mirror;
using Newtonsoft.Json;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using static ChickenScratch.ColourManager;
using static ChickenScratch.DrawingRound;

namespace ChickenScratch
{
    public class GameFlowManager : MonoBehaviour
    {
        public enum GamePhase
        {
            loading, game_tutorial, instructions, drawing, results, slides_tutorial, slides, accolades, store, review, accusation, invalid
        }
        public GamePhase currentGamePhase = GamePhase.loading;

        public enum PlayerRole
        {
            worker, botcher, invalid
        }
        public List<NetworkConnectionToClient> connectedPlayers = new List<NetworkConnectionToClient>();
        public List<BirdName> connectedBirds = new List<BirdName>();

        public float loadingTimeLimit;
        public GameObject linePrefab;
        public Transform drawingsContainer;
        public int maxRounds = 5;

        public int numberOfPrefixesInChain = 10;
        public int numberOfNounsInChain = 10;
        public int numberOfBotcherPrefixOptions = 5;
        public int numberOfBotcherNounOptions = 5;

        public Dictionary<BirdName, Vector3> birdArmPositionMap = new Dictionary<BirdName, Vector3>();
        public Dictionary<BirdName, Vector3> accuseArmStretchMap = new Dictionary<BirdName, Vector3>();

        public Dictionary<BirdName, PlayerData> gamePlayers = new Dictionary<BirdName, PlayerData>();
        public List<BirdName> disconnectedPlayers = new List<BirdName>();
        public Dictionary<BirdName, int> playerCabinetMap = new Dictionary<BirdName, int>();
        public Dictionary<BirdName, List<Tuple<ChainData, int, int, BirdName>>> queuedChains = new Dictionary<BirdName, List<Tuple<ChainData, int, int, BirdName>>>();
        public List<BirdName> activeBirdNames = new List<BirdName>();

        public float timeRemainingInPhase;
        public bool active;
        public float armUpdateFrequency = 0.5f;

        public TutorialSequence botcherGameTutorialSequence, bossRushGameTutorialSequence, botcherSlidesTutorialSequence, bossRushSlidesTutorialSequence, accusationTutorialSequence;
        public TutorialSequence endlessModeTutorialSequence;

        public List<BirdName> scoreTrackerPlayers;
        private PlayerFlowManager playerFlowManager;
        private float timeSinceLastArmUpdate = 0.0f;
        public int totalPlayersConnected = 1;

        public bool IsInitialized => isInitialized;
        private bool isInitialized = false;
        private int currentLowestUnusedCaseNumber = 1;
        public int currentModifier = 1;
        public int numberOfCorrectCasesInARow = 0;
        public int totalPoints = 0;
        public List<string> activeTransitionConditions = new List<string>();

        public WordManager wordManager = new WordManager();

        [SerializeField]
        private RoleData workerRole;

        public int totalCompletedCases = 0;
        private bool hasRequestedBirdClaim = false;
        private bool triedToInitialize = false;

        // Start is called before the first frame update
        void Start()
        {
            Initialize();
        }


        // Update is called once per frame
        void Update()
        {
            if (!isInitialized)
            {
                Initialize();
                return;
            }
            if (!active)
            {
                return;
            }
            timeRemainingInPhase -= Time.deltaTime;
            

            if (timeRemainingInPhase <= 0)
            {
                TransitionPhase();
            }
            else
            {
                UpdatePhase();
            }
        }

        private void Initialize()
        {
            
            int playerCount = SettingsManager.Instance.GetPlayerNameCount();
            bool notAllPlayersHaveConnected = connectedPlayers.Count != playerCount;//allPlayersHaveReadied up
            if(notAllPlayersHaveConnected)
            {
                return;
            }
            if(!hasRequestedBirdClaim)
            {
                GameManager.Instance.gameDataHandler.RpcRequestConnectionAcknowledgment();
                hasRequestedBirdClaim = true;
            }
            bool notAllPlayersHaveClaimedBirds = connectedBirds.Count != playerCount;
            if (notAllPlayersHaveClaimedBirds)
            {
                return;
            }

            if (!playerFlowManager)
            {
                playerFlowManager = GameManager.Instance.playerFlowManager;
            }
            if (triedToInitialize) return;
            triedToInitialize = true;
            GameManager.Instance.gameDataHandler.RpcServerIsReady();
            GameManager.Instance.gameFlowManager.SetCabinetOwnership();
            InitializeGame();
            
            timeRemainingInPhase = loadingTimeLimit;
            active = true;
            isInitialized = true;
        }

        private void InitializeGame()
        {
            wordManager.LoadPromptWords();
            wordManager.LoadCustomWords();
            wordManager.DisableInactiveCategories(SettingsManager.Instance.wordGroupNames);

            //Initialize the players
            List<BirdName> allPlayerBirds = SettingsManager.Instance.GetAllActiveBirds();
            List<RoleData> rolesToAssign = SettingsManager.Instance.gameMode.rolesToDistribute;
            for(int i = rolesToAssign.Count; i < allPlayerBirds.Count; i++)
            {
                rolesToAssign.Add(workerRole);
            }
            rolesToAssign = rolesToAssign.OrderBy(r => Guid.NewGuid()).ToList();
            int iterator = 0;
            foreach (BirdName playerBird in allPlayerBirds)
            {
                string playerName = SettingsManager.Instance.GetPlayerName(playerBird);
                activeBirdNames.Add(playerBird);
                gamePlayers.Add(playerBird, new PlayerData() { birdName = playerBird, playerName = playerName, playerRoleType = rolesToAssign[iterator].roleType });
                GameManager.Instance.gameDataHandler.TargetInitializePlayer(SettingsManager.Instance.GetConnection(playerBird), playerName, playerBird, rolesToAssign[iterator].roleType);
                playerFlowManager.playerNameMap.Add(playerBird, playerName);
                iterator++;
            }
            GameManager.Instance.gameDataHandler.RPCUpdatePlayerNameMapWrapper(playerFlowManager.playerNameMap);

            //Assign cabinets to each player
            iterator = 1;
            foreach(KeyValuePair<BirdName,PlayerData> player in gamePlayers)
            {
                playerCabinetMap.Add(player.Key, iterator);
                GameManager.Instance.gameDataHandler.TargetAssignCabinetToPlayer(SettingsManager.Instance.GetConnection(player.Key), iterator);
                iterator++;
               
            }
            SetPlayerObjectOwnership();


            switch (SettingsManager.Instance.gameMode.caseDeliveryMode)
            {
                case GameModeData.CaseDeliveryMode.queue:
                    InitializeQueueMode();
                    break;
                case GameModeData.CaseDeliveryMode.free_for_all:
                    
                    break;
            }
        }

        private void InitializeQueueMode()
        {
            SettingsManager.Instance.ServerBroadcastPlayerNames();
           

            InitializeChains();

            BirdName currentPlayer;

            //Send cases to players
            foreach (KeyValuePair<int, CabinetDrawer> cabinet in playerFlowManager.drawingRound.cabinetDrawerMap)
            {
                ChainData chain = cabinet.Value.currentChainData;
                if (chain == null || !chain.active)
                {
                    continue;
                }
                if (!playerFlowManager.drawingRound.caseMap.ContainsKey(chain.identifier))
                {
                    playerFlowManager.drawingRound.caseMap.Add(chain.identifier, chain);
                }

                currentPlayer = chain.playerOrder[1];
                AddCaseToQueue(currentPlayer, chain, chain.identifier, 1, currentPlayer);

                switch (SettingsManager.Instance.gameMode.wordDistributionMode)
                {
                    case GameModeData.WordDistributionMode.random:
                        DistributeRandomWordsToPlayer(currentPlayer, chain, cabinet.Key);
                        break;
                    default:
                        Debug.LogError("Word distribution mode[" + SettingsManager.Instance.gameMode.wordDistributionMode.ToString() + "] has not been implemented.");
                        break;
                }
            }
        }


        private void DistributeRandomWordsToPlayer(BirdName currentPlayer, ChainData currentChain, int cabinetIndex)
        {
            if (currentPlayer == SettingsManager.Instance.birdName)
            {
                playerFlowManager.drawingRound.playerCabinetIndex = cabinetIndex;
                playerFlowManager.drawingRound.SetInitialPrompt(currentChain.identifier, currentChain.correctPrompt);
            }
            else
            {
                GameManager.Instance.gameFlowManager.addTransitionCondition("initial_cabinet_prompt_receipt:" + currentPlayer);
                GameManager.Instance.gameDataHandler.TargetAssignCabinetToPlayer(SettingsManager.Instance.GetConnection(currentPlayer), cabinetIndex);
                GameManager.Instance.gameDataHandler.TargetInitialCabinetPromptContents(SettingsManager.Instance.GetConnection(currentPlayer), currentChain.identifier, currentChain.correctPrompt, true);
            }

            //Send possible words
            GameManager.Instance.gameDataHandler.TargetPossibleWordsWrapper(SettingsManager.Instance.GetConnection(currentChain.guessData.author), currentChain.identifier, currentChain.possibleWordsMap);
            
        }

        private void InitializeChains()
        {
            switch (SettingsManager.Instance.gameMode.caseDeliveryMode)
            {
                case GameModeData.CaseDeliveryMode.queue:
                case GameModeData.CaseDeliveryMode.free_for_all:
                    InitializeCaseQueue();
                    return;
                default:
                    Debug.LogError("Delivery mode[" + SettingsManager.Instance.gameMode.caseDeliveryMode.ToString() + "] has not been implemented yet.");
                    return;
            }
        }

        public void IncreaseNumberOfCompletedCases()
        {
            totalCompletedCases++;
            int requiredCaseCount = SettingsManager.Instance.GetCaseCountForDay();

            if(totalCompletedCases >= requiredCaseCount)
            {
                //Drawing round is over, moving on to the next round
                timeRemainingInPhase = 0f;
            }
        }

        public void AddCaseToQueue(BirdName birdName, ChainData caseData, int caseID, int round, BirdName lastAuthor)
        {
            if (!queuedChains.ContainsKey(birdName))
            {
                queuedChains.Add(birdName, new List<Tuple<ChainData, int, int, BirdName>>());
            }

            Tuple<ChainData, int, int, BirdName> caseGrouping = Tuple.Create(caseData, caseID, round, lastAuthor);
            queuedChains[birdName].Add(caseGrouping);

            updateQueuedFolderVisuals(birdName);
        }

        public int CreateCaseFromChoice(BirdName startingPlayer, CaseChoiceNetData choiceNetData)
        {
            //Pick the case that matches the choice
            CaseChoiceData choiceData = GameDataManager.Instance.GetCaseChoice(choiceNetData.caseChoiceIdentifier);
            if(choiceData == null)
            {
                Debug.LogError("ERROR[CreateCaseFromChoice]: Could not find choice data for identifier["+choiceNetData.caseChoiceIdentifier+"]");
                return - 1;
            }
            CaseTemplateData selectedCase = new CaseTemplateData(choiceData);

            PlayerTextInputData tempPromptData = new PlayerTextInputData()
            {
                author = BirdName.none
            };
            ChainData newChain = new ChainData();
            newChain.pointsForBonus = choiceData.bonusPoints;
            newChain.pointsPerCorrectWord = choiceData.pointsPerCorrectWord;
            newChain.identifier = currentLowestUnusedCaseNumber;
            currentLowestUnusedCaseNumber++;
            newChain.taskQueue = selectedCase.queuedTasks;
            newChain.caseTypeColour = selectedCase.caseTypeColour;
            newChain.caseTypeName = selectedCase.caseTypeName;
            newChain.currentRound = 1;
            //May need to add case modifiers here

            //Set the player order
            newChain.playerOrder.Add(1, startingPlayer);
            List<BirdName> randomizedPlayers = GameManager.Instance.gameFlowManager.connectedBirds.OrderBy(cp => Guid.NewGuid()).ToList();

            //Randomize the player list and then add players until the number of tasks has been filled
            for (int j = 0; j < randomizedPlayers.Count; j++)
            {
                if (choiceData.numberOfTasks < newChain.playerOrder.Count)
                {
                    break;
                }
                if (randomizedPlayers[j] != startingPlayer)
                {
                    newChain.playerOrder.Add(newChain.playerOrder.Count + 1, randomizedPlayers[j]);
                }
            }
            if(!newChain.playerOrder.ContainsKey(choiceData.numberOfTasks))
            {
                Debug.LogError("ERROR[CreateCaseFromChoice]: Player order is missing task["+choiceData.numberOfTasks.ToString()+"] for case["+newChain.identifier.ToString()+"]");
                return -1;
            }

            newChain.guessData.author = newChain.playerOrder[choiceData.numberOfTasks];

            //set correct prompt, correctWordsMap, possibleWordsMap and the first prompt text value
            newChain.SetWordsFromChoice(choiceNetData);
            tempPromptData.text = newChain.correctPrompt;
            newChain.prompts.Add(1, tempPromptData);
            newChain.currentScoreModifier = selectedCase.startingScoreModifier + choiceNetData.modifierIncreaseValue;
            newChain.scoreModifierDecrement = choiceNetData.scoreModifierDecrement;
            newChain.maxScoreModifier = choiceNetData.maxScoreModifier;
            if (!GameManager.Instance.playerFlowManager.drawingRound.caseMap.ContainsKey(newChain.identifier))
            {
                GameManager.Instance.playerFlowManager.drawingRound.caseMap.Add(newChain.identifier, newChain);
            }

            SendPossibleWordsToGuesser(newChain);

            return newChain.identifier;
        }

        public void SendTaskToNextPlayer(int caseID)
        {
            if(!GameManager.Instance.playerFlowManager.drawingRound.caseMap.ContainsKey(caseID))
            {
                Debug.LogError("ERROR[SendTaskToNextPlayer]: CaseMap does not contain case[" + caseID.ToString() + "]");
                return;
            }
            ChainData caseData = GameManager.Instance.playerFlowManager.drawingRound.caseMap[caseID];
            
            caseData.currentRound++;
            
            if (caseData.taskQueue.Count <= caseData.currentRound-1)
            {
                Debug.LogError("ERROR[SendTaskToNextPlayer]: Could not queue the next task["+caseData.currentRound.ToString()+"]. TaskQueue["+caseData.taskQueue.Count.ToString()+"] doesn't contain that task.");
                return;
            }
            if(!caseData.playerOrder.ContainsKey(caseData.currentRound) || !caseData.playerOrder.ContainsKey(caseData.currentRound-1))
            {
                Debug.LogError("ERROR[SendTaskToNextPlayer]: Player order is either missing this round["+caseData.currentRound.ToString()+"] or last round["+(caseData.currentRound-1).ToString()+"]");
                return;
            }
            
            BirdName nextBird = caseData.playerOrder[caseData.currentRound];
            
            BirdName lastBird = caseData.playerOrder[caseData.currentRound - 1];
            if(lastBird == BirdName.none)
            {
                Debug.LogError("ERROR[SendTaskToNextPlayer]: Attempting to send task to next player but the last player is none.");
                return;
            }
            AddCaseToQueue(nextBird, caseData, caseID, caseData.currentRound, lastBird);
            //If there's only one case queued then we need to open the cabinet drawer
            if (queuedChains.ContainsKey(nextBird) && queuedChains[nextBird].Count == 1)
            {
                GameManager.Instance.playerFlowManager.drawingRound.SendNextInQueue(nextBird);
            }
        }

        public void dequeueFrontCase(BirdName birdName)
        {
            if (queuedChains.ContainsKey(birdName) && queuedChains[birdName].Count > 0)
            {
                queuedChains[birdName].RemoveAt(0);

                if (queuedChains[birdName].Count == 0)
                {
                    GameManager.Instance.gameDataHandler.RpcCloseCabinetDrawer(playerCabinetMap[birdName]);
                }
            }
            updateQueuedFolderVisuals(birdName);
        }

        private void InitializeCaseQueue()
        {
            PlayerTextInputData tempPromptData;
            ChainData newChain;
            List<BirdName> playerColours = gamePlayers.Keys.ToList();
            for (int i = 0; i < gamePlayers.Count; i++)
            {
                if (GameManager.Instance.gameFlowManager.disconnectedPlayers.Contains(playerColours[i]))
                {
                    continue;
                }

                //Pick the base case
                CaseTemplateData selectedCase = SettingsManager.Instance.gameMode.baseTemplateData;
                string correctPrompt = "";

                wordManager.ClearUsedWords();
                tempPromptData = new PlayerTextInputData()
                {
                    author = BirdName.none
                };
                newChain = new ChainData();
                Debug.LogError("Initializing case queue and creating new case["+currentLowestUnusedCaseNumber.ToString()+"]");
                newChain.identifier = currentLowestUnusedCaseNumber;
                currentLowestUnusedCaseNumber++;

                int cabinetID = playerCabinetMap[newChain.playerOrder[1]];
                GameManager.Instance.playerFlowManager.drawingRound.cabinetDrawerMap[cabinetID].currentChainData = newChain;

                switch (SettingsManager.Instance.gameMode.wordDistributionMode)
                {
                    case GameModeData.WordDistributionMode.random:
                        wordManager.PopulateStandardCaseWords(newChain, selectedCase.startingWordIdentifiers);
                        newChain.correctPrompt = correctPrompt;
                        tempPromptData.text = correctPrompt;
                        newChain.prompts.Add(1, tempPromptData);
                        break;
                }
            }

            return;
        }

        public void reorderCasesOnDisconnect(BirdName disconnectingPlayer)
        {
            //Should there be a transition condition added here for each player for receiving the updates?

            //Remove the case of the disconnecting player
            //int disconnectingCabinetIndex = playerCabinetMap[disconnectingPlayer];
            //GameManager.Instance.playerFlowManager.drawingRound.cabinetDrawerMap[disconnectingCabinetIndex].currentChainData = null;
            playerCabinetMap.Remove(disconnectingPlayer);

            activeBirdNames.Remove(disconnectingPlayer);

            //playerFlowManager.numberOfCabinetRounds = (gamePlayers.Count % 2) == 0 ? gamePlayers.Count - 1 : gamePlayers.Count - 2;
            //playerFlowManager.numberOfCabinetRounds = Mathf.Clamp(playerFlowManager.numberOfCabinetRounds, 3, 5);

            //Iterate through the remaining cases and redetermine the player order
            List<BirdName> playerColours = gamePlayers.Keys.ToList();
            BirdName currentPlayer;

            //for (int i = 0; i < gamePlayers.Count; i++)
            //{
            //    if (GameManager.Instance.gameFlowManager.disconnectedPlayers.Contains(playerColours[i]))
            //    {
            //        continue;
            //    }
            //    //Find the cabinet with the matching starting player
            //    int matchingCabinetIndex = playerCabinetMap[playerColours[i]];
            //    CabinetDrawer matchingCabinet = GameManager.Instance.playerFlowManager.drawingRound.cabinetDrawerMap[matchingCabinetIndex];
            //    ChainData matchingChain = matchingCabinet.currentChainData;
            //    matchingChain.playerOrder.Clear();

            //    //Set the player order
            //    for (int j = 0; j < playerFlowManager.numberOfCabinetRounds + 1; j++)
            //    {
            //        currentPlayer = i + j >= playerColours.Count ? playerColours[i + j - playerColours.Count] : playerColours[i + j];
            //        matchingChain.playerOrder.Add(j + 1, currentPlayer);
            //    }
            //    matchingChain.guessData.author = matchingChain.playerOrder[playerFlowManager.numberOfCabinetRounds + 1];

            //    //Send possible words
            //    GameManager.Instance.gameDataHandler.TargetPossibleWordsWrapper(SettingsManager.Instance.birdConnectionMap[matchingChain.guessData.author], matchingChain.identifier, matchingChain.possibleWordsMap);
            //}
        }

        public void populateEmptyCasesForDisconnect(BirdName disconnectingPlayer)
        {
            foreach (KeyValuePair<int, ChainData> caseData in playerFlowManager.drawingRound.caseMap)
            {
                if (caseData.Value.guessData.author == disconnectingPlayer)
                {
                    //We don't need to do anything for the guessing round with the disconnecting player,
                    //the logic for handling it already works fine
                    continue;
                }
                foreach (KeyValuePair<int, BirdName> player in caseData.Value.playerOrder)
                {
                    if (player.Value == disconnectingPlayer)
                    {
                        if (player.Key % 2 == 0) //If the round is a prompting round
                        {
                            //Debug.LogError("Adding empty prompt for case["+caseData.Key.ToString()+"] round["+player.Key.ToString()+"]");
                            PlayerTextInputData promptData = new PlayerTextInputData();
                            promptData.author = player.Value;
                            promptData.text = "";
                            promptData.timeTaken = 0f;
                            caseData.Value.prompts.Add(player.Key, promptData);
                        }
                        else //If the round is a drawing round
                        {
                            //Debug.LogError("Adding empty drawing for case["+caseData.Key.ToString()+"] round["+player.Key.ToString()+"]");
                            DrawingData drawingData = new DrawingData(caseData.Key, player.Key, player.Value);
                            drawingData.author = player.Value;
                            drawingData.visuals = new List<DrawingLineData>();
                            drawingData.timeTaken = 0f;
                            caseData.Value.drawings.Add(player.Key, drawingData);
                        }
                    }
                }
            }
        }

        private void TransitionPhase()
        {
            if (!playerFlowManager)
            {
                playerFlowManager = GameManager.Instance.playerFlowManager;
            }
            switch (currentGamePhase)
            {
                case GamePhase.loading:

                    foreach (BirdName player in gamePlayers.Keys)
                    {
                        if (player == SettingsManager.Instance.birdName)
                        {
                            continue;
                        }
                    }

                    if (SettingsManager.Instance.GetSetting("tutorials"))
                    {
                        currentGamePhase = GamePhase.game_tutorial;
                    }
                    else
                    {
                        currentGamePhase = GamePhase.instructions;
                    }

                    break;
                case GamePhase.game_tutorial:
                    if (bossRushGameTutorialSequence.active ||
                        !isRoundOver())
                    {
                        return;
                    }
                    bossRushGameTutorialSequence.gameObject.SetActive(false);
 
                    currentGamePhase = GamePhase.instructions;
                    break;
                case GamePhase.instructions:
                    if (!isRoundOver())
                    {
                        return;
                    }
                    currentGamePhase = GamePhase.drawing;

                    break;
                case GamePhase.drawing:
                    if(!playerFlowManager.drawingRound.hasRequestedCaseDetails)
                    {
                        playerFlowManager.OnOutOfTime();
                        return;
                    }
                    else if (!playerFlowManager.drawingRound.hasSentCaseDetails)
                    {
                        if (!isRoundOver())
                        {
                            return;
                        }
                        queuedChains.Clear();
                        foreach (BirdName bird in gamePlayers.Keys)
                        {
                            updateQueuedFolderVisuals(bird);
                            if (!disconnectedPlayers.Contains(bird) && bird != SettingsManager.Instance.birdName)
                            {
                                addTransitionCondition("endgame_data_loaded:" + bird);
                            }
                        }
                        GameManager.Instance.playerFlowManager.slidesRound.GenerateEndgameData();
                        playerFlowManager.drawingRound.hasSentCaseDetails = true;
                        return;
                    }
                    else
                    {
                        if (!isRoundOver())
                        {
                            UpdatePhase();
                            return;
                        }
                        
                        //Send the difficulty values of the correct values to each player
                        broadcastCaseDifficultyValues();
                        currentGamePhase = GamePhase.slides;
                    }
                    break;
                case GamePhase.slides_tutorial:
                    if (bossRushSlidesTutorialSequence.active ||
                            !isRoundOver())
                    {
                        return;
                    }
                    foreach (BirdName bird in gamePlayers.Keys)
                    {
                        if (!disconnectedPlayers.Contains(bird) && bird != SettingsManager.Instance.birdName)
                        {
                            addTransitionCondition("ratings_loaded:" + bird);
                            addTransitionCondition("stats_loaded:" + bird);
                        }
                    }
                    bossRushSlidesTutorialSequence.gameObject.SetActive(false);

                    currentGamePhase = GamePhase.slides;
                    break;
                case GamePhase.slides:
                    if (playerFlowManager.slidesRound.inProgress)
                    {
                        if (isRoundOver())
                        {
                            playerFlowManager.slidesRound.inProgress = false;
                        }
                        return;
                    }
                    else if (!isRoundOver())
                    {
                        return;
                    }
                    currentGamePhase = GameManager.Instance.playerFlowManager.slidesRound.phaseToTransitionTo;

                    break;
                case GamePhase.accolades:
                    if (playerFlowManager.accoladesRound.isActive)
                    {
                        return;
                    }
                    currentGamePhase = GamePhase.results;
                    break;
                case GamePhase.accusation:
                    if (!isRoundOver())
                    {
                        return;
                    }

                    //Possible outcomes:
                    //A tie
                    //An elimination
                        //Botcher is eliminated - game is over
                        //Worker is eliminated 
                            //There are more un-eliminated workers than

                    break;
                case GamePhase.review:
                    if (!isRoundOver())
                    {
                        return;
                    }
                    if(playerFlowManager.reviewRound.accusedBird != BirdName.none)
                    {
                        currentGamePhase = GamePhase.accusation;
                    }
                    else
                    {
                        currentGamePhase = GamePhase.store;
                    }
                    break;
                case GamePhase.store:
                    foreach(BirdName bird in SettingsManager.Instance.GetAllActiveBirds())
                    {
                        string potentiallyActiveCondition = "store_complete:" + bird.ToString();
                        if (activeTransitionConditions.Contains(potentiallyActiveCondition))
                        {
                            activeTransitionConditions.Remove(potentiallyActiveCondition);
                        }
                    }
                    currentGamePhase = GamePhase.instructions;
                    break;
                    
                case GamePhase.results:
                    return;
            }

            playerFlowManager.loadingCircleObject.SetActive(false);
            timeRemainingInPhase = playerFlowManager.currentTimeInRound;

            //Debug.Log("Broadcasting the phase["+currentGamePhase.ToString()+"].");
            //Broadcast the phase
            GameManager.Instance.gameDataHandler.RpcUpdateGamePhase(currentGamePhase);

        }

        public void addTransitionCondition(string condition)
        {
            if (activeTransitionConditions.Contains(condition))
            {
                Debug.LogError("Condition["+condition+"] already found in list of active conditions, could not add.");
                return;
            }
            //Debug.LogError("Adding["+ condition + "] to transition conditions.");
            activeTransitionConditions.Add(condition);
        }

        public void resolveTransitionCondition(string condition)
        {
            if (!activeTransitionConditions.Contains(condition))
            {
                //Debug.LogError("Condition["+condition+"] not found in list of active conditions, could not resolve.");
                return;
            }
            activeTransitionConditions.Remove(condition);
            //Debug.LogError("Resolving transition condition[" + condition + "]. There are still " + activeTransitionConditions.Count + " active conditions.");

            if (isRoundOver())
            {
                //Debug.LogError("Round is over, setting time remaining in phase to 0.");
                //timeRemainingInPhase = 0.0f;
            }
        }

        public void clearPlayerTransitionConditions(BirdName disconnectingPlayer)
        {
            List<string> playerTransitionConditionTags = new List<string>()
        {
            "force_submit",
            "drawing_submitted",
            "empty_drawing_receipt",
            "drawing_receipt",
            "prompt_submitted",
            "guess_submitted",
            "ratings_loaded",
            "tutorial_finished",
            "cabinet_prompt_receipt",
            "stats_loaded"
        };
            foreach (string playerTransitionConditionTag in playerTransitionConditionTags)
            {
                string potentialPlayerTransitionCondition = playerTransitionConditionTag + ":" + disconnectingPlayer.ToString();
                if (activeTransitionConditions.Contains(potentialPlayerTransitionCondition))
                {
                    activeTransitionConditions.Remove(potentialPlayerTransitionCondition);
                }
            }
        }

        private void broadcastCaseDifficultyValues()
        {
            foreach (ChainData currentCase in GameManager.Instance.playerFlowManager.drawingRound.caseMap.Values)
            {
                CaseWordData correctPrefix = GameDataManager.Instance.GetWord(currentCase.correctWordIdentifierMap[1]);
                CaseWordData correctNoun = GameDataManager.Instance.GetWord(currentCase.correctWordIdentifierMap[2]);
                int prefixDifficulty = correctPrefix != null ? correctPrefix.difficulty : 0;
                int nounDifficulty = correctNoun != null ? correctNoun.difficulty : 0;
                GameManager.Instance.gameDataHandler.RpcCaseDifficultyValues(currentCase.identifier, prefixDifficulty, nounDifficulty);
            }
        }

        private void UpdatePhase()
        {
            if (!playerFlowManager)
            {
                playerFlowManager = GameManager.Instance.playerFlowManager;
            }
            switch (currentGamePhase)
            {
                case GamePhase.loading:
                case GamePhase.instructions:
                case GamePhase.drawing:
                    timeSinceLastArmUpdate += Time.deltaTime;

                    if (timeSinceLastArmUpdate > armUpdateFrequency)
                    {
                        if (birdArmPositionMap != null && birdArmPositionMap.Count > 0)
                        {
                            //Broadcast arm positions to all other clients
                            GameManager.Instance.gameDataHandler.RpcDrawingPhasePositionsWrapper(birdArmPositionMap);

                            timeSinceLastArmUpdate = 0.0f;
                        }

                    }
                    break;
                case GamePhase.store:
                    GameManager.Instance.playerFlowManager.storeRound.UpdatePhase();
                    break;
            }
        }

        public bool isRoundOver()
        {
            if (activeTransitionConditions.Count > 0)
            {
                return false;
            }

            return true;
        }

        public int GetNumberOfConnectedPlayers()
        {
            return gamePlayers.Count - disconnectedPlayers.Count;
        }

        public void SetPlayerObjectOwnership()
        {
            bool isTheaterMode = GameManager.Instance.currentGameScene == GameManager.GameScene.theater;
            List<int> playerOrder = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7 };
            playerOrder = playerOrder.OrderBy(a => Guid.NewGuid()).ToList();
            int iterator = 0;

            List<BirdName> allPlayerBirds = SettingsManager.Instance.GetAllActiveBirds();
            foreach (BirdName player in allPlayerBirds)
            {
                GameManager.Instance.gameDataHandler.RpcRandomizedSetBirdIndex(playerOrder[iterator], player);
                GameManager.Instance.playerFlowManager.slidesRound.initializeGalleryBird(playerOrder[iterator], player);
                GameManager.Instance.playerFlowManager.accoladesRound.initializeAccoladeBirdRow(playerOrder[iterator], player);
                iterator++;
            }
            SetCabinetOwnership();
        }

        public void SetCabinetOwnership()
        {
            foreach (KeyValuePair<BirdName, int> cabinetPairing in playerCabinetMap)
            {
                if (SettingsManager.Instance.GetConnection(cabinetPairing.Key) == null) continue;

                CabinetDrawer drawer = GameManager.Instance.playerFlowManager.drawingRound.cabinetDrawerMap[cabinetPairing.Value];
                drawer.setCabinetOwner(cabinetPairing.Key);
                GameManager.Instance.gameDataHandler.RpcSetCabinetOwner(drawer.id, drawer.currentPlayer);
            }
        }

        public void updateQueuedFolderVisuals(BirdName birdName)
        {
            if (!playerCabinetMap.ContainsKey(birdName))
            {
                Debug.LogError("ERROR[updateQueuedFolderVisuals]: Could not isolate matching cabinet for player[" + birdName.ToString() + "]");
                return;
            }
            int cabinetID = playerCabinetMap[birdName];
            if(!GameManager.Instance.playerFlowManager.drawingRound.cabinetDrawerMap.ContainsKey(cabinetID))
            {
                Debug.LogError("ERROR[updateQueuedFolderVisuals]: Could not isolate matching cabinet[" + cabinetID.ToString() + "]");
                return;
            }
            List<BirdName> queuedFolderColours = new List<BirdName>();
            if (queuedChains.ContainsKey(birdName))
            {
                foreach (Tuple<ChainData, int, int, BirdName> queuedCase in queuedChains[birdName])
                {
                    queuedFolderColours.Add(queuedCase.Item4);
                }
            }
            GameManager.Instance.playerFlowManager.drawingRound.cabinetDrawerMap[cabinetID].setQueuedFolders(queuedFolderColours);
            GameManager.Instance.gameDataHandler.RpcUpdateQueuedFolderVisuals(cabinetID, queuedFolderColours);
        }

        public void SetBirdArmPosition(BirdName inBirdName, Vector3 currentPosition)
        {
            if (!birdArmPositionMap.ContainsKey(inBirdName))
            {
                birdArmPositionMap.Add(inBirdName, currentPosition);
            }
            else
            {
                birdArmPositionMap[inBirdName] = currentPosition;
            }
        }

        public void SetLongArmPosition(BirdName inBirdName, Vector3 currentPosition, PlayerStretchArm.Variant variant)
        {
            switch(variant)
            {
                case PlayerStretchArm.Variant.store:
                    GameManager.Instance.playerFlowManager.storeRound.SetLongArmPosition(inBirdName, currentPosition);
                    break;
            }
            
        }

        public void SendPossibleWordsToGuesser(ChainData caseData)
        {
            //Send the possible options to the guesser
            //Send possible words
            GameManager.Instance.gameDataHandler.TargetPossibleWordsWrapper(SettingsManager.Instance.GetConnection(caseData.guessData.author), caseData.identifier, caseData.possibleWordsMap);
        }
    }

    public class PlayerTextInputData
    {
        public BirdName author = BirdName.none;
        public string text = "";
        public float timeTaken = 0.0f;
    }

    public class PlayerRatingData
    {
        public BirdName target = BirdName.none;
        public int likeCount = 0;
        public int dislikeCount = 0;

        public override string ToString()
        {
            return likeCount + GameDelim.SUB + dislikeCount;
        }
    }

    public class PlayerData
    {
        public BirdName birdName = BirdName.none;
        public RoleData.RoleType playerRoleType = RoleData.RoleType.worker;
        public BirdName nextInOrder = BirdName.none;
        public string playerName = "";
        public bool isEliminated = false;
    }

    public class DrawingVisualsPackage
    {
        public ColourManager.BirdName author = BirdName.none;
        public Dictionary<int, string> messages = new Dictionary<int, string>();
        public int expectedNumberOfMessages = -1;
        public int caseID = -1;
        public int round = -1;
        public string packageID = "";
        public float timeTaken = 0.0f;
    }

}