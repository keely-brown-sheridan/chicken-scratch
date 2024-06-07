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
            loading, game_tutorial, instructions, drawing, results, slides_tutorial, slides, accolades, invalid
        }
        public GamePhase currentGamePhase = GamePhase.loading;

        public enum PlayerRole
        {
            worker, botcher, invalid
        }
        public List<BirdName> connectedPlayers = new List<BirdName>();

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

        public int numberOfPlayers;
        public float timeRemainingInPhase;
        public bool active;
        public float armUpdateFrequency = 0.5f;

        public TutorialSequence botcherGameTutorialSequence, bossRushGameTutorialSequence, botcherSlidesTutorialSequence, bossRushSlidesTutorialSequence, accusationTutorialSequence;
        public TutorialSequence endlessModeTutorialSequence;
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

        private int totalCompletedCases = 0;

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
            bool notAllPlayersHaveConnected = GameManager.Instance.gameFlowManager.connectedPlayers.Count != SettingsManager.Instance.playerNameMap.Count;
            if (notAllPlayersHaveConnected)
            {
                return;
            }
            if (!connectedPlayers.Contains(SettingsManager.Instance.birdName))
            {
                connectedPlayers.Add(SettingsManager.Instance.birdName);
            }
            if (!playerFlowManager)
            {
                playerFlowManager = GameManager.Instance.playerFlowManager;
            }

            playerFlowManager.serverIsReady = true;
            GameManager.Instance.gameDataHandler.RpcServerIsReady();

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
            foreach (KeyValuePair<BirdName, string> player in SettingsManager.Instance.playerNameMap)
            {
                activeBirdNames.Add(player.Key);
                gamePlayers.Add(player.Key, new global::ChickenScratch.PlayerData() { birdName = player.Key, playerName = player.Value, playerRole = PlayerRole.worker });
                playerFlowManager.playerNameMap.Add(player.Key, player.Value);
            }

            numberOfPlayers = gamePlayers.Count;

            //Assign cabinets to each player
            int iterator = 1;
            foreach(KeyValuePair<BirdName,PlayerData> player in gamePlayers)
            {
                playerCabinetMap.Add(player.Key, iterator);
                GameManager.Instance.gameDataHandler.TargetAssignCabinetToPlayer(SettingsManager.Instance.birdConnectionMap[player.Key], iterator);
                iterator++;
               
            }
            SetPlayerObjectOwnership();


            switch (SettingsManager.Instance.gameMode.caseDeliveryMode)
            {
                case GameModeData.CaseDeliveryMode.queue:
                    InitializeQueueMode();
                    break;
                case GameModeData.CaseDeliveryMode.free_for_all:
                    GameManager.Instance.gameDataHandler.RpcActivateCasePile();
                    break;
            }
        }

        private void InitializeQueueMode()
        {
            Debug.Log("Setting the number of cabinet rounds. Gameplayers[" + gamePlayers.Count.ToString() + "]");
            playerFlowManager.numberOfCabinetRounds = (gamePlayers.Count % 2) == 0 ? gamePlayers.Count - 1 : gamePlayers.Count - 2;
            if (playerFlowManager.numberOfCabinetRounds > 5)
            {
                playerFlowManager.numberOfCabinetRounds = 5;
            }
            GameManager.Instance.gameDataHandler.RpcPlayerInitializationWrapper(playerFlowManager.numberOfCabinetRounds, SettingsManager.Instance.playerNameMap);

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
                GameManager.Instance.playerFlowManager.drawingRound.UpdateCaseRound(chain.identifier, 1);
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
                playerFlowManager.drawingRound.SetInitialPrompt(currentChain.identifier, currentChain.correctPrompt, false);
            }
            else
            {
                GameManager.Instance.gameFlowManager.addTransitionCondition("initial_cabinet_prompt_receipt:" + currentPlayer);
                GameManager.Instance.gameDataHandler.TargetAssignCabinetToPlayer(SettingsManager.Instance.birdConnectionMap[currentPlayer], cabinetIndex);
                GameManager.Instance.gameDataHandler.TargetInitialCabinetPromptContents(SettingsManager.Instance.birdConnectionMap[currentPlayer], currentChain.identifier, currentChain.correctPrompt, true);
            }

            //Send possible words
            GameManager.Instance.gameDataHandler.TargetPossibleWordsWrapper(SettingsManager.Instance.birdConnectionMap[currentChain.guesser], currentChain.identifier, currentChain.possibleWordsMap);
            
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
            if(totalCompletedCases >= SettingsManager.Instance.gameMode.numberOfCases)
            {
                Debug.LogError("Total completed cases["+totalCompletedCases.ToString()+"] numberOfCases in game[" + SettingsManager.Instance.gameMode.numberOfCases.ToString()+ "]");
                //Drawing round is over, moving on to the next round
                //timeRemainingInPhase = 0f;
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

        public int CreateCaseFromChoice(BirdName startingPlayer, CaseChoiceData choice)
        {
            //Pick the case that matches the choice
            CaseTemplateData selectedCase = new CaseTemplateData(choice);

            PlayerTextInputData tempPromptData = new PlayerTextInputData()
            {
                author = BirdName.none
            };
            ChainData newChain = new ChainData();
            newChain.identifier = currentLowestUnusedCaseNumber;
            currentLowestUnusedCaseNumber++;
            newChain.taskQueue = selectedCase.queuedTasks;
            newChain.currentRound = 1;
            GameManager.Instance.playerFlowManager.drawingRound.UpdateCaseRound(newChain.identifier, newChain.currentRound);

            //May need to add case modifiers here

            //Set the player order
            newChain.playerOrder.Add(1, startingPlayer);
            List<BirdName> randomizedPlayers = GameManager.Instance.gameFlowManager.connectedPlayers.OrderBy(cp => Guid.NewGuid()).ToList();

            //Randomize the player list and then add players until the number of tasks has been filled
            for (int j = 0; j < randomizedPlayers.Count; j++)
            {
                if (choice.numberOfTasks < newChain.playerOrder.Count)
                {
                    break;
                }
                if (randomizedPlayers[j] != startingPlayer)
                {
                    newChain.playerOrder.Add(newChain.playerOrder.Count + 1, randomizedPlayers[j]);
                }
                
            }

            newChain.guesser = newChain.playerOrder[choice.numberOfTasks];

            //set correct prompt, correctWordsMap, possibleWordsMap and the first prompt text value
            newChain.SetWordsFromChoice(choice);
            tempPromptData.text = newChain.correctPrompt;
            newChain.prompts.Add(1, tempPromptData);
            newChain.currentScoreModifier = selectedCase.startingScoreModifier;
            if (!GameManager.Instance.playerFlowManager.drawingRound.caseMap.ContainsKey(newChain.identifier))
            {
                GameManager.Instance.playerFlowManager.drawingRound.caseMap.Add(newChain.identifier, newChain);
            }

            SendPossibleWordsToGuesser(newChain);

            return newChain.identifier;
        }

        public void SendTaskToNextPlayer(int caseID)
        {
            ChainData caseData = GameManager.Instance.playerFlowManager.drawingRound.caseMap[caseID];
            
            caseData.currentRound++;
            GameManager.Instance.playerFlowManager.drawingRound.UpdateCaseRound(caseID, caseData.currentRound);
            if(!caseData.playerOrder.ContainsKey(caseData.currentRound))
            {
                return;
            }
            BirdName nextBird = caseData.playerOrder[caseData.currentRound];

            AddCaseToQueue(nextBird, caseData, caseID, caseData.currentRound, caseData.playerOrder[caseData.currentRound - 1]);
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
            BirdName currentPlayer;
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

                //Set the player order
                for (int j = 0; j < playerFlowManager.numberOfCabinetRounds + 1; j++)
                {
                    currentPlayer = i + j >= playerColours.Count ? playerColours[i + j - playerColours.Count] : playerColours[i + j];
                    newChain.playerOrder.Add(j + 1, currentPlayer);
                }

                newChain.guesser = newChain.playerOrder[playerFlowManager.numberOfCabinetRounds + 1];

                //Put the new chain into the corresponding cabinet
                int cabinetID = playerCabinetMap[newChain.playerOrder[1]];
                GameManager.Instance.playerFlowManager.drawingRound.cabinetDrawerMap[cabinetID].currentChainData = newChain;

                switch (SettingsManager.Instance.gameMode.wordDistributionMode)
                {
                    case GameModeData.WordDistributionMode.random:
                        wordManager.PopulateStandardCaseWords(newChain, selectedCase.startingWords);
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
            int disconnectingCabinetIndex = playerCabinetMap[disconnectingPlayer];
            GameManager.Instance.playerFlowManager.drawingRound.cabinetDrawerMap[disconnectingCabinetIndex].currentChainData = null;
            playerCabinetMap.Remove(disconnectingPlayer);

            activeBirdNames.Remove(disconnectingPlayer);
            numberOfPlayers = gamePlayers.Count;

            playerFlowManager.numberOfCabinetRounds = (gamePlayers.Count % 2) == 0 ? gamePlayers.Count - 1 : gamePlayers.Count - 2;
            playerFlowManager.numberOfCabinetRounds = Mathf.Clamp(playerFlowManager.numberOfCabinetRounds, 3, 5);

            //Iterate through the remaining cases and redetermine the player order
            List<BirdName> playerColours = gamePlayers.Keys.ToList();
            BirdName currentPlayer;

            for (int i = 0; i < gamePlayers.Count; i++)
            {
                if (GameManager.Instance.gameFlowManager.disconnectedPlayers.Contains(playerColours[i]))
                {
                    continue;
                }
                //Find the cabinet with the matching starting player
                int matchingCabinetIndex = playerCabinetMap[playerColours[i]];
                CabinetDrawer matchingCabinet = GameManager.Instance.playerFlowManager.drawingRound.cabinetDrawerMap[matchingCabinetIndex];
                ChainData matchingChain = matchingCabinet.currentChainData;
                matchingChain.playerOrder.Clear();

                //Set the player order
                for (int j = 0; j < playerFlowManager.numberOfCabinetRounds + 1; j++)
                {
                    currentPlayer = i + j >= playerColours.Count ? playerColours[i + j - playerColours.Count] : playerColours[i + j];
                    matchingChain.playerOrder.Add(j + 1, currentPlayer);
                }
                matchingChain.guesser = matchingChain.playerOrder[playerFlowManager.numberOfCabinetRounds + 1];

                //Send possible words
                GameManager.Instance.gameDataHandler.TargetPossibleWordsWrapper(SettingsManager.Instance.birdConnectionMap[matchingChain.guesser], matchingChain.identifier, matchingChain.possibleWordsMap);
            }
        }

        public void populateEmptyCasesForDisconnect(BirdName disconnectingPlayer)
        {
            foreach (KeyValuePair<int, ChainData> caseData in playerFlowManager.drawingRound.caseMap)
            {
                if (caseData.Value.guesser == disconnectingPlayer)
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
                    if (!playerFlowManager.drawingRound.hasSentCaseDetails)
                    {
                        foreach (BirdName bird in gamePlayers.Keys)
                        {
                            if (!disconnectedPlayers.Contains(bird) && bird != SettingsManager.Instance.birdName)
                            {
                                activeTransitionConditions.Add("endgame_data_loaded:" + bird);
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
                        Debug.LogError("Round is over! Moving on now..");
                        foreach (BirdName bird in gamePlayers.Keys)
                        {
                            if (!disconnectedPlayers.Contains(bird) && bird != SettingsManager.Instance.birdName)
                            {
                                activeTransitionConditions.Add("ratings_loaded:" + bird);
                                activeTransitionConditions.Add("stats_loaded:" + bird);
                            }
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
                            activeTransitionConditions.Add("ratings_loaded:" + bird);
                            activeTransitionConditions.Add("stats_loaded:" + bird);
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
                    if (SettingsManager.Instance.showFastResults)
                    {
                        if (!playerFlowManager.slidesRound.quickplaySummarySlide.isActive)
                        {
                            playerFlowManager.slidesRound.inProgress = false;
                            GameManager.Instance.playerFlowManager.slidesRound.ShowFastResults();
                            GameManager.Instance.gameDataHandler.RpcShowQuickResults();
                        }
                        return;
                    }
                    else
                    {
                        //Get rid of this return once we've got slides working
                        return;
                        currentGamePhase = GamePhase.accolades;
                    }
                    break;
                case GamePhase.accolades:
                    if (playerFlowManager.accoladesRound.isActive)
                    {
                        return;
                    }
                    currentGamePhase = GamePhase.results;
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
            Debug.LogError("Adding["+ condition + "] to transition conditions.");
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
            Debug.LogError("Resolving transition condition[" + condition + "]. There are still " + activeTransitionConditions.Count + " active conditions.");

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
                int prefixDifficulty = currentCase.correctWordsMap.ContainsKey(1) ? currentCase.correctWordsMap[1].difficulty : 0;
                int nounDifficulty = currentCase.correctWordsMap.ContainsKey(2) ? currentCase.correctWordsMap[2].difficulty : 0;
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

        public void SetPlayerObjectOwnership()
        {
            bool isTheaterMode = GameManager.Instance.currentGameScene == GameManager.GameScene.theater;
            List<int> playerOrder = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7 };
            playerOrder = playerOrder.OrderBy(a => Guid.NewGuid()).ToList();
            int iterator = 0;

            foreach (BirdName player in SettingsManager.Instance.playerNameMap.Keys)
            {
                GameManager.Instance.gameDataHandler.RpcRandomizedSetBirdPosition(playerOrder[iterator], player);
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
                if (!connectedPlayers.Contains(cabinetPairing.Key)) continue;

                CabinetDrawer drawer = GameManager.Instance.playerFlowManager.drawingRound.cabinetDrawerMap[cabinetPairing.Value];
                drawer.setCabinetOwner(cabinetPairing.Key);
                GameManager.Instance.gameDataHandler.RpcSetCabinetOwner(drawer.id, drawer.currentPlayer);
            }
        }

        public void updateQueuedFolderVisuals(BirdName birdName)
        {
            int cabinetID = playerCabinetMap[birdName];
            List<BirdName> queuedFolderColours = new List<BirdName>();
            if (queuedChains.ContainsKey(birdName))
            {
                foreach (Tuple<ChainData, int, int, BirdName> queuedCase in queuedChains[birdName])
                {
                    queuedFolderColours.Add(queuedCase.Item4);
                }
            }
            playerFlowManager.drawingRound.cabinetDrawerMap[cabinetID].setQueuedFolders(queuedFolderColours);
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

        public void SendPossibleWordsToGuesser(ChainData caseData)
        {
            //Send the possible options to the guesser
            //Send possible words
            GameManager.Instance.gameDataHandler.TargetPossibleWordsWrapper(SettingsManager.Instance.birdConnectionMap[caseData.guesser], caseData.identifier, caseData.possibleWordsMap);
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
        public GameFlowManager.PlayerRole playerRole = GameFlowManager.PlayerRole.worker;
        public BirdName nextInOrder = BirdName.none;
        public string playerName = "";
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