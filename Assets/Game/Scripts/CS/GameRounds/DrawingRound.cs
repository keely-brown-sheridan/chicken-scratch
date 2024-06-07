using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static ChickenScratch.ColourManager;
using static ChickenScratch.TaskData;

namespace ChickenScratch
{
    public class DrawingRound : PlayerRound
    {
        public enum CaseState
        {
            prompting, drawing, copy_drawing, add_drawing, waiting, guessing, invalid
        }
        public CaseState currentState = CaseState.drawing;

        public Text playerNameText;
        public List<CabinetDrawer> allCabinets = new List<CabinetDrawer>();
        public List<BirdArm> allBirdArms = new List<BirdArm>();
        public PlayerBirdArm playerBirdArm;
        
        public bool canGetCabinet;

        public SpriteRenderer deskRenderer;
        public Dictionary<int, CabinetDrawer> cabinetDrawerMap;
        public Dictionary<int, ChainData> caseMap = new Dictionary<int, ChainData>();
        public int maxLineMessageLength = 300;

        public bool isInitialized = false;
        public bool hasSentCaseDetails = false;
        public Dictionary<int,int> currentRoundMap = new Dictionary<int, int>();
        public int playerCabinetIndex = -1;

        public UnityEvent onPlayerSubmitTask;
        public UnityEvent onPlayerStartTask;
        public bool playerIsReady = true;

        [SerializeField]
        private DrawingCaseFolder drawingCaseFolder;

        [SerializeField]
        private PromptingCaseFolder promptingCaseFolder;

        [SerializeField]
        private CopyingCaseFolder copyingCaseFolder;

        [SerializeField]
        private AddingCaseFolder addingCaseFolder;

        [SerializeField]
        private GuessingCaseFolder guessingCaseFolder;

        [SerializeField]
        private TMPro.TMP_Text casesRemainingText;

        private Dictionary<BirdName, BirdArm> birdArmMap;

        [SerializeField]
        private CaseChoicePanel caseChoicePanel;
        
        public CasePile newCaseCabinet;

        public bool stampIsActive = false;



        private void Awake()
        {
            if (!isInitialized)
            {
                Initialize();
            }
        }

        public void Initialize()
        {
            onPlayerStartTask.AddListener(HandlePlayerStartTask);
            onPlayerSubmitTask.AddListener(HandlePlayerSubmitTask);
            timeInRound = SettingsManager.Instance.gameMode.totalGameTime;

            if (cabinetDrawerMap == null || cabinetDrawerMap.Count == 0)
            {
                cabinetDrawerMap = new Dictionary<int, CabinetDrawer>();
                foreach (CabinetDrawer cabinet in allCabinets)
                {
                    cabinetDrawerMap.Add(cabinet.id, cabinet);
                }
            }

            birdArmMap = new Dictionary<ColourManager.BirdName, BirdArm>();
            foreach (BirdArm birdArm in allBirdArms)
            {
                birdArmMap.Add(birdArm.birdName, birdArm);
            }
            playerIsReady = true;
            hasSentCaseDetails = false;
            isInitialized = true;
            UpdateNumberOfCases(SettingsManager.Instance.gameMode.numberOfCases);
        }

        public override void StartRound()
        {
            base.StartRound();

            if (!isInitialized)
            {
                Initialize();
            }

            if (SettingsManager.Instance.isHost)
            {
                if(SettingsManager.Instance.gameMode.caseDeliveryMode == GameModeData.CaseDeliveryMode.queue)
                {
                    InitializeCabinetDrawers();
                }
                GameManager.Instance.gameFlowManager.timeRemainingInPhase = SettingsManager.Instance.gameMode.totalGameTime;
                GameManager.Instance.gameDataHandler.RpcUpdateTimer(SettingsManager.Instance.gameMode.totalGameTime);
            }

            //Start the clock
            GameManager.Instance.playerFlowManager.loadingCircleObject.SetActive(false);
        }

        private void InitializeCabinetDrawers()
        {
            BirdName currentBird;
            foreach (KeyValuePair<int, CabinetDrawer> cabinetDrawer in cabinetDrawerMap)
            {
                currentBird = cabinetDrawer.Value.currentChainData != null && cabinetDrawer.Value.currentChainData.active ? cabinetDrawer.Value.currentChainData.playerOrder[1] : BirdName.none;

                if (currentBird != BirdName.none && GameManager.Instance.gameFlowManager.gamePlayers.ContainsKey(currentBird) && !GameManager.Instance.gameFlowManager.disconnectedPlayers.Contains(currentBird))
                {
                    GameManager.Instance.gameFlowManager.addTransitionCondition("drawing_submitted:" + currentBird);
                    GameManager.Instance.gameFlowManager.addTransitionCondition("drawing_receipt:" + cabinetDrawer.Value.currentChainData.playerOrder[2] + ":" + cabinetDrawer.Value.currentChainData.identifier.ToString());
                    //Open the cabinet drawer
                    cabinetDrawer.Value.setAsReady(currentBird);
                    ChainData currentChainData = cabinetDrawer.Value.currentChainData;
                    //Broadcast to other players to open the drawers
                    FolderUpdateData folderUpdateData = new FolderUpdateData()
                    {
                        cabinetIndex = cabinetDrawer.Key,
                        currentState = CaseState.drawing,
                        roundNumber = 1,
                        caseID = cabinetDrawer.Value.currentChainData.identifier,
                        player = currentBird,
                        taskTime = currentChainData.taskQueue[0].duration,
                        currentScoreModifier = currentChainData.currentScoreModifier
                    };
                    GameManager.Instance.gameDataHandler.RpcUpdateFolderAsReady(folderUpdateData);
                }
            }
        }

        public void SetDrawerAsClosed(int cabinetIndex)
        {
            CabinetDrawer selectedCabinet = cabinetDrawerMap[cabinetIndex];
            selectedCabinet.close();
            if (selectedCabinet.ready)
            {
                selectedCabinet.ready = false;
                selectedCabinet.gameObject.SetActive(true);
            }
        }

        public void UpdateToNewFolderState()
        {
            ChainData chainData = cabinetDrawerMap[playerCabinetIndex].currentChainData;
            float currentTaskTime = chainData.currentTaskDuration;
            float scoreDecrement = SettingsManager.Instance.gameMode.scoreModifierDecrement;
            Color inFolderColour = Color.white;
            int lastRoundIndex = currentRoundMap.ContainsKey(chainData.identifier) ? currentRoundMap[chainData.identifier] -1 : -1;
            if(lastRoundIndex == -1)
            {
                Debug.LogError("ERROR[UpdateToNewFolderState]: Last round index could not be set for case[" + chainData.identifier + "].");
            }
            stampIsActive = false;

            switch (currentState)
            {
                case CaseState.prompting:
                    if (chainData.drawings.ContainsKey(lastRoundIndex))
                    {
                        inFolderColour = ColourManager.Instance.birdMap[chainData.drawings[lastRoundIndex].author].folderColour;
                    }
                    promptingCaseFolder.RegisterToTimer(ForceSubmit);
                    promptingCaseFolder.Initialize(chainData.drawings[lastRoundIndex]);
                    promptingCaseFolder.Show(inFolderColour, currentTaskTime, chainData.currentScoreModifier, scoreDecrement);

                    break;
                case CaseState.drawing:
                    string prompt;
                    if (chainData.prompts.ContainsKey(lastRoundIndex))
                    {
                        prompt = chainData.prompts[lastRoundIndex].text;
                        inFolderColour = ColourManager.Instance.birdMap[chainData.prompts[lastRoundIndex].author].folderColour;
                    }
                    else
                    {
                        inFolderColour = ColourManager.Instance.birdMap[SettingsManager.Instance.birdName].folderColour;
                        prompt = chainData.correctPrompt;
                    }
                    drawingCaseFolder.RegisterToTimer(ForceSubmit);
                    drawingCaseFolder.Initialize(prompt);
                    drawingCaseFolder.Show(inFolderColour, currentTaskTime, chainData.currentScoreModifier, scoreDecrement);

                    break;
                case CaseState.copy_drawing:
                    if (chainData.drawings.ContainsKey(lastRoundIndex))
                    {
                        inFolderColour = ColourManager.Instance.birdMap[chainData.drawings[lastRoundIndex].author].folderColour;
                    }
                    copyingCaseFolder.RegisterToTimer(ForceSubmit);
                    if(!chainData.drawings.ContainsKey(lastRoundIndex))
                    {
                        Debug.LogError("Could not access drawing for round["+lastRoundIndex.ToString()+"] in case["+chainData.identifier.ToString()+"].");
                    }
                    copyingCaseFolder.Initialize(chainData.drawings[lastRoundIndex]);
                    copyingCaseFolder.Show(inFolderColour, currentTaskTime, chainData.currentScoreModifier, scoreDecrement);
                    
                    break;
                case CaseState.add_drawing:
                    if (!chainData.drawings.ContainsKey(lastRoundIndex))
                    {
                        Debug.LogError("Could not access drawing for round[" + lastRoundIndex.ToString() + "] in case[" + chainData.identifier.ToString() + "].");
                    }
                    if (chainData.drawings.ContainsKey(lastRoundIndex))
                    {
                        inFolderColour = ColourManager.Instance.birdMap[chainData.drawings[lastRoundIndex].author].folderColour;
                    }
                    addingCaseFolder.RegisterToTimer(ForceSubmit);
                    addingCaseFolder.Initialize(chainData.drawings[lastRoundIndex], chainData.correctPrompt);
                    addingCaseFolder.Show(inFolderColour, currentTaskTime, chainData.currentScoreModifier, scoreDecrement);
                    break;
                case CaseState.guessing:
                    guessingCaseFolder.RegisterToTimer(ForceSubmit);
                    guessingCaseFolder.Initialize(chainData.identifier, chainData.possibleWordsMap, chainData.drawings[lastRoundIndex]);

                    inFolderColour = ColourManager.Instance.birdMap[chainData.drawings[lastRoundIndex].author].folderColour;

                    guessingCaseFolder.Show(inFolderColour, currentTaskTime, chainData.currentScoreModifier, scoreDecrement);
                    break;
            }

            AudioManager.Instance.PlaySoundVariant("sfx_game_int_folder_select");
        }

        public void DisableCabinet(int cabinetID)
        {
            cabinetDrawerMap[cabinetID].gameObject.SetActive(false);
        }

        public CabinetDrawer GetCabinet(int inIndex)
        {
            return cabinetDrawerMap[inIndex];
        }

        public BirdArm GetBirdArm(BirdName inBirdName)
        {
            return birdArmMap[inBirdName];
        }

        public void ReleaseDeskFolder()
        {
            CabinetDrawer selectedCabinet = cabinetDrawerMap[playerCabinetIndex];
            ChainData selectedCase = selectedCabinet.currentChainData;
            DrawingData newDrawing;
            int currentRoundIndex = currentRoundMap.ContainsKey(selectedCase.identifier) ? currentRoundMap[selectedCase.identifier] : -1;
            if(currentRoundIndex == -1)
            {
                Debug.LogError("ERROR[ReleaseDeskFolder]: Could not isolate currentRoundIndex for case["+selectedCase.identifier.ToString()+"].");
            }
            //AudioManager.Instance.PlaySoundVariant("sfx_game_int_folder_submit");
            switch (currentState)
            {
                case CaseState.drawing:
                    if (!cabinetDrawerMap[playerCabinetIndex].currentChainData.drawings.ContainsKey(currentRoundIndex))
                    {
                        newDrawing = new DrawingData(selectedCase.identifier, currentRoundIndex, SettingsManager.Instance.birdName);
                        selectedCase.drawings.Add(currentRoundIndex, newDrawing);
                    }

                    if (!ReleaseDeskDrawingFolder(selectedCase, false))
                    {
                        Debug.LogError("Failed to release desk drawing folder.");
                        return;
                    }
                    break;
                case CaseState.prompting:
                    if (!ReleaseDeskPromptFolder(selectedCabinet, false))
                    {
                        Debug.LogError("Failed to release desk prompting folder.");
                        return;
                    }
                    break;
                case CaseState.copy_drawing:
                    
                    if (!cabinetDrawerMap[playerCabinetIndex].currentChainData.drawings.ContainsKey(currentRoundIndex))
                    {
                        newDrawing = new DrawingData(selectedCase.identifier, currentRoundIndex, SettingsManager.Instance.birdName);
                        selectedCase.drawings.Add(currentRoundIndex, newDrawing);
                    }

                    if (!ReleaseDeskCopyingFolder(selectedCase, false))
                    {
                        Debug.LogError("Failed to release desk copying folder.");
                        return;
                    }
                    break;
                case CaseState.add_drawing:
                    if (!cabinetDrawerMap[playerCabinetIndex].currentChainData.drawings.ContainsKey(currentRoundIndex))
                    {
                        newDrawing = new DrawingData(selectedCase.identifier, currentRoundIndex, SettingsManager.Instance.birdName);
                        selectedCase.drawings.Add(currentRoundIndex, newDrawing);
                    }

                    if (!ReleaseDeskAddingFolder(selectedCase, false))
                    {
                        Debug.LogError("Failed to release desk adding folder.");
                        return;
                    }
                    break;
                case CaseState.guessing:
                    guessingCaseFolder.ChooseGuess();
                    break;
            }

            Submit(false);
        }

        public void UpdateBirdFolderStatus(BirdName birdName, Color folderColour, bool isOn)
        {
            birdArmMap[birdName].heldFolderObject.SetActive(isOn);
            birdArmMap[birdName].heldFolderObject.GetComponent<SpriteRenderer>().color = folderColour;
        }

        private bool ReleaseDeskDrawingFolder(ChainData currentChain, bool force)
        {
            if (!drawingCaseFolder.HasStarted() && !force)
            {
                return false;
            }
            if (SettingsManager.Instance.GetSetting("stickies"))
            {
                TutorialSticky drawingToolsSticky = GameManager.Instance.playerFlowManager.instructionRound.drawingToolsSticky;
                TutorialSticky drawingToolsSticky2 = GameManager.Instance.playerFlowManager.instructionRound.drawingToolsSticky2;
                if (!drawingToolsSticky.hasBeenClicked)
                {
                    drawingToolsSticky.Click();
                }
                if (!drawingToolsSticky2.hasBeenClicked)
                {
                    drawingToolsSticky2.Click();
                }
            }

            float timeUsed = SettingsManager.Instance.gameMode.baseTimeInDrawingRound - GameManager.Instance.playerFlowManager.currentTimeInRound;

            int currentRoundIndex = currentRoundMap.ContainsKey(currentChain.identifier) ? currentRoundMap[currentChain.identifier] : -1;
            if(currentRoundIndex == -1)
            {
                Debug.LogError("ERROR[ReleaseDeskDrawingFolder]: Could not isolate currentRoundIndex for case["+currentChain.identifier.ToString()+"]");
            }
            DrawingData newDrawing = currentChain.drawings[currentRoundIndex];

            newDrawing.author = SettingsManager.Instance.birdName;
            newDrawing.timeTaken = timeUsed;
            newDrawing.visuals = drawingCaseFolder.GetVisuals();

            return true;
        }

        private bool ReleaseDeskCopyingFolder(ChainData currentChain, bool force)
        {
            if (!copyingCaseFolder.HasStarted() && !force)
            {
                Debug.LogError("HasStarted[" + copyingCaseFolder.HasStarted().ToString() + "], force[" + force.ToString() + "]");
                return false;
            }
            if (SettingsManager.Instance.GetSetting("stickies"))
            {
                TutorialSticky drawingToolsSticky = GameManager.Instance.playerFlowManager.instructionRound.drawingToolsSticky;
                TutorialSticky drawingToolsSticky2 = GameManager.Instance.playerFlowManager.instructionRound.drawingToolsSticky2;
                if (!drawingToolsSticky.hasBeenClicked)
                {
                    drawingToolsSticky.Click();
                }
                if (!drawingToolsSticky2.hasBeenClicked)
                {
                    drawingToolsSticky2.Click();
                }
            }

            float timeUsed = SettingsManager.Instance.gameMode.baseTimeInDrawingRound - GameManager.Instance.playerFlowManager.currentTimeInRound;

            int currentRoundIndex = currentRoundMap.ContainsKey(currentChain.identifier) ? currentRoundMap[currentChain.identifier] : -1;
            if(currentRoundIndex == -1)
            {
                Debug.LogError("Error[ReleaseDeskCopyingFolder]: Could not isolate currentRoundIndex for case["+currentChain.identifier.ToString()+"]");
            }
            DrawingData newDrawing = currentChain.drawings[currentRoundIndex];

            newDrawing.author = SettingsManager.Instance.birdName;
            newDrawing.timeTaken = timeUsed;
            newDrawing.visuals = copyingCaseFolder.GetVisuals();

            copyingCaseFolder.Hide();

            return true;
        }

        private bool ReleaseDeskAddingFolder(ChainData currentChain, bool force)
        {
            if (!addingCaseFolder.HasStarted() && !force)
            {
                Debug.LogError("HasStarted[" + copyingCaseFolder.HasStarted().ToString() + "], force[" + force.ToString() + "]");
                return false;
            }
            if (SettingsManager.Instance.GetSetting("stickies"))
            {
                TutorialSticky drawingToolsSticky = GameManager.Instance.playerFlowManager.instructionRound.drawingToolsSticky;
                TutorialSticky drawingToolsSticky2 = GameManager.Instance.playerFlowManager.instructionRound.drawingToolsSticky2;
                if (!drawingToolsSticky.hasBeenClicked)
                {
                    drawingToolsSticky.Click();
                }
                if (!drawingToolsSticky2.hasBeenClicked)
                {
                    drawingToolsSticky2.Click();
                }
            }

            float timeUsed = SettingsManager.Instance.gameMode.baseTimeInDrawingRound - GameManager.Instance.playerFlowManager.currentTimeInRound;

            int currentRoundIndex = currentRoundMap.ContainsKey(currentChain.identifier) ? currentRoundMap[currentChain.identifier] : -1;
            if(currentRoundIndex == -1)
            {
                Debug.LogError("ERROR[ReleaseDeskAddingFolder]: Could not isolate currentRoundIndex for case[" + currentChain.identifier.ToString() +"]");
            }
            DrawingData newDrawing = currentChain.drawings[currentRoundIndex];

            newDrawing.author = SettingsManager.Instance.birdName;
            newDrawing.timeTaken = timeUsed;
            newDrawing.visuals = addingCaseFolder.GetVisuals();

            addingCaseFolder.Hide();

            return true;
        }

        private bool ReleaseDeskPromptFolder(CabinetDrawer selectedCabinet, bool force)
        {
            if (!promptingCaseFolder.HasStarted() && !force)
            {
                return false;
            }

            PlayerTextInputData prompt;
            int caseID = selectedCabinet.currentChainData.identifier;
            int currentRoundIndex = currentRoundMap.ContainsKey(caseID) ? currentRoundMap[caseID] : -1;
            if(currentRoundIndex == -1)
            {
                Debug.LogError("ERROR[ReleaseDeskPromptFolder]: Could not isolate currentRoundIndex for case["+caseID.ToString()+"]");
            }
            if (!selectedCabinet.currentChainData.prompts.ContainsKey(currentRoundIndex))
            {
                prompt = new PlayerTextInputData();
                selectedCabinet.currentChainData.prompts.Add(currentRoundIndex, prompt);
            }
            else
            {
                prompt = selectedCabinet.currentChainData.prompts[currentRoundIndex];
            }

            float timeUsed = 0f;
            prompt.timeTaken = timeUsed;
            prompt.text = GameDelim.stripGameDelims(promptingCaseFolder.GetPromptText());
            
            promptingCaseFolder.Hide();

            return true;
        }

        public void Submit(bool force)
        {
            int caseID;
            float currentScoreModifier = 1f;
            if (playerCabinetIndex == -1 ||
                        !cabinetDrawerMap.ContainsKey(playerCabinetIndex))
            {
                playerCabinetIndex = -1;
                foreach (CabinetDrawer cabinet in cabinetDrawerMap.Values)
                {
                    if (cabinet.currentChainData.active)
                    {
                        caseID = cabinet.currentChainData.identifier;
                        int currentRoundIndex = currentRoundMap.ContainsKey(caseID) ? currentRoundMap[caseID] : -1;
                        if(cabinet.currentChainData.playerOrder.ContainsKey(currentRoundIndex) && cabinet.currentChainData.playerOrder[currentRoundIndex] == SettingsManager.Instance.birdName)
                        {
                            playerCabinetIndex = cabinet.id;
                        }
                        
                    }
                }
                if (playerCabinetIndex == -1)
                {
                    Debug.LogError("ERROR[Submit]: Could not isolate cabinet for player, check to make sure the currentRoundMap is working.");
                }
            }
            switch (currentState)
            {
                case CaseState.drawing:
                    AudioManager.Instance.PlaySound("Stamp");
                    SubmitDrawing();
                    drawingCaseFolder.DeregisterFromTimer(ForceSubmit);
                    drawingCaseFolder.Hide();
                    currentScoreModifier = drawingCaseFolder.GetScoreModifier();
                    break;
                case CaseState.prompting:
                    AudioManager.Instance.PlaySound("Stamp");
                    SubmitPrompt(cabinetDrawerMap[playerCabinetIndex], force);
                    promptingCaseFolder.DeregisterFromTimer(ForceSubmit);
                    promptingCaseFolder.Hide();
                    currentScoreModifier = promptingCaseFolder.GetScoreModifier();
                    break;
                case CaseState.guessing:
                    guessingCaseFolder.DeregisterFromTimer(ForceSubmit);
                    guessingCaseFolder.Hide();
                    currentScoreModifier = guessingCaseFolder.GetScoreModifier();
                    break;
                case CaseState.copy_drawing:
                    AudioManager.Instance.PlaySound("Stamp");
                    SubmitDrawing();
                    copyingCaseFolder.DeregisterFromTimer(ForceSubmit);
                    copyingCaseFolder.Hide();
                    currentScoreModifier = copyingCaseFolder.GetScoreModifier();
                    break;
                case CaseState.add_drawing:
                    AudioManager.Instance.PlaySound("Stamp");
                    SubmitDrawing();
                    addingCaseFolder.DeregisterFromTimer(ForceSubmit);
                    addingCaseFolder.Hide();
                    currentScoreModifier = addingCaseFolder.GetScoreModifier();
                    break;
            }

            

            caseID = cabinetDrawerMap[playerCabinetIndex].currentChainData.identifier;
            if (force)
            {
                if (SettingsManager.Instance.isHost)
                {
                    GameManager.Instance.gameFlowManager.resolveTransitionCondition("force_submit:" + SettingsManager.Instance.birdName);
                }
                else
                {
                    GameManager.Instance.gameDataHandler.CmdTransitionCondition("force_submit:" + SettingsManager.Instance.birdName);
                }
            }
            else
            {
                GameManager.Instance.playerFlowManager.drawingRound.onPlayerSubmitTask.Invoke();
                if (SettingsManager.Instance.gameMode.caseDeliveryMode == GameModeData.CaseDeliveryMode.free_for_all)
                {
                    if (currentState != CaseState.guessing)
                    {
                        GameManager.Instance.gameDataHandler.CmdTransitionCase(caseID, currentScoreModifier);
                    }
                    GameManager.Instance.gameDataHandler.CmdRequestNextCase(SettingsManager.Instance.birdName);
                }
            }
            
        }

        public void SendNextInQueue(BirdName birdToSendTo)
        {
            int cabinetIndex = GameManager.Instance.gameFlowManager.playerCabinetMap[birdToSendTo];
            Dictionary<BirdName, List<Tuple<ChainData, int, int, BirdName>>> queuedChains = GameManager.Instance.gameFlowManager.queuedChains;
            if (queuedChains.ContainsKey(birdToSendTo) &&
                queuedChains[birdToSendTo].Count > 0)
            {
                Tuple<ChainData, int, int, BirdName> firstQueuedChain = queuedChains[birdToSendTo][0];
                //If there are, then open the drawer for a guessing instance
                ChainData currentCase = firstQueuedChain.Item1;
                int currentCaseIndex = firstQueuedChain.Item2;
                int currentRound = firstQueuedChain.Item3;
                //Open the cabinet
                cabinetDrawerMap[cabinetIndex].setAsReady(birdToSendTo);
                CaseState caseState = CaseState.invalid;

                TaskData queuedTaskData = firstQueuedChain.Item1.taskQueue[firstQueuedChain.Item3-1];
                NetworkConnectionToClient birdConnection = SettingsManager.Instance.birdConnectionMap[birdToSendTo];
                switch (queuedTaskData.taskType)
                {
                    case TaskData.TaskType.prompt_drawing:
                        //Send them the prompt?
                        caseState = CaseState.drawing;
                        break;
                    case TaskData.TaskType.prompting:
                        //Send them the drawing
                        caseState = CaseState.prompting;
                        break;
                    case TaskData.TaskType.copy_drawing:
                        //Send them the drawing
                        caseState = CaseState.copy_drawing;
                        break;
                    case TaskData.TaskType.add_drawing:
                        caseState = CaseState.add_drawing;
                        //Send them the original prompt
                        GameManager.Instance.gameDataHandler.TargetInitialCabinetPromptContents(birdConnection, currentCaseIndex, currentCase.correctPrompt, false);
                        break;
                    case TaskData.TaskType.base_drawing:
                        //Send them the prompt
                        caseState = CaseState.drawing;
                        break;
                    case TaskData.TaskType.base_guessing:
                        //Send them the possible prompts and drawing
                        caseState = CaseState.guessing;
                        break;
                }

                FolderUpdateData folderUpdateData = new FolderUpdateData()
                {
                    cabinetIndex = cabinetIndex,
                    currentState = caseState,
                    roundNumber = currentRound,
                    caseID = currentCaseIndex,
                    player = birdToSendTo,
                    taskTime = queuedTaskData.duration,
                    currentScoreModifier = currentCase.currentScoreModifier
                };
                GameManager.Instance.gameDataHandler.RpcUpdateFolderAsReady(folderUpdateData);
            }
            else
            {
                //If there are no queued cases then close the drawer
                GameManager.Instance.gameDataHandler.RpcCloseCabinetDrawer(cabinetIndex);
            }
        }

        public void StartChoiceCaseDrawing(int caseID, string prompt, float taskTime, float currentModifierValue, float modifierDecrement)
        {
            drawingCaseFolder.RegisterToTimer(ForceSubmit);
            currentState = CaseState.drawing;
            ChainData newChain = new ChainData();
            cabinetDrawerMap[playerCabinetIndex].currentChainData = newChain;

            newChain.identifier = caseID;
            Color folderColour = ColourManager.Instance.birdMap[SettingsManager.Instance.birdName].folderColour;
            drawingCaseFolder.Initialize(prompt);
            drawingCaseFolder.Show(folderColour, taskTime, currentModifierValue, modifierDecrement);
        }

        private void ForceSubmit()
        {
            Submit(true);
        }

        private void SubmitDrawing()
        {
            ChainData selectedCase = cabinetDrawerMap[playerCabinetIndex].currentChainData;
            DrawingData newDrawing;
            int currentRoundIndex = currentRoundMap.ContainsKey(selectedCase.identifier) ? currentRoundMap[selectedCase.identifier] : -1;
            if(currentRoundIndex == -1)
            {
                Debug.LogError("ERROR[SubmitDrawing]: Could not isolate currentRoundIndex for case["+selectedCase.identifier.ToString()+"]");
            }
            if (!cabinetDrawerMap[playerCabinetIndex].currentChainData.drawings.ContainsKey(currentRoundIndex))
            {
                Debug.LogError("Creating new drawing because cabinet did not contain a drawing.");
                newDrawing = new DrawingData(selectedCase.identifier, currentRoundIndex, SettingsManager.Instance.birdName);
                selectedCase.addDrawing(currentRoundIndex, newDrawing);
            }
            else
            {
                newDrawing = selectedCase.drawings[currentRoundIndex];
            }

            GameManager.Instance.gameDataHandler.CmdSendDrawing(newDrawing);
            GameManager.Instance.gameDataHandler.CmdTransitionCondition("drawing_submitted:" + SettingsManager.Instance.birdName.ToString());

            canGetCabinet = true;
        }

        public void SendEmptyCabinetDrawingToClients(BirdName nextPlayer, BirdName author, int caseID, int round)
        {
            StatTracker.Instance.AddEmptySubmitter(author);
            GameManager.Instance.gameFlowManager.dequeueFrontCase(author);
            GameManager.Instance.gameFlowManager.resolveTransitionCondition("drawing_submitted:" + author);
            GameManager.Instance.gameFlowManager.resolveTransitionCondition("drawing_receipt:" + nextPlayer + ":" + caseID.ToString());
            //Send empty drawing to all other players
            foreach (PlayerData player in GameManager.Instance.gameFlowManager.gamePlayers.Values)
            {
                if (player.birdName != SettingsManager.Instance.birdName)
                {
                    GameManager.Instance.gameFlowManager.addTransitionCondition("empty_drawing_receipt:" + player.birdName.ToString());

                    SendEmptyDrawingToClient(caseID, round, player.birdName, author);
                }
            }
        }

        public void ShowCaseChoices(CaseChoiceData choice1, CaseChoiceData choice2)
        {
            caseChoicePanel.SetChoices(choice1, choice2);
        }

        private void SendEmptyDrawingToServer(BirdName author, int caseID, int round)
        {
            GameManager.Instance.gameDataHandler.CmdEmptyDrawing(author, caseID, round);
        }

        private void SubmitPrompt(CabinetDrawer selectedCabinet, bool force)
        {
            int caseID = selectedCabinet.currentChainData.identifier;
            int currentRoundIndex = currentRoundMap.ContainsKey(caseID) ? currentRoundMap[caseID] : -1;
            if(currentRoundIndex == -1)
            {
                Debug.LogError("ERROR[SubmitPrompt]: Could not isolate currentRoundIndex for case["+caseID.ToString()+"]");
            }
            PlayerTextInputData prompt;
            if (force)
            {
                if (SettingsManager.Instance.isHost)
                {
                    GameManager.Instance.gameFlowManager.dequeueFrontCase(SettingsManager.Instance.birdName);
                }
                else
                {
                    GameManager.Instance.gameDataHandler.CmdDequeueFrontCase(SettingsManager.Instance.birdName);
                }
                ReleaseDeskPromptFolder(selectedCabinet, force);
                prompt = selectedCabinet.currentChainData.prompts.ContainsKey(currentRoundIndex) ? selectedCabinet.currentChainData.prompts[currentRoundIndex] : new PlayerTextInputData();

            }
            else
            {
                prompt = selectedCabinet.currentChainData.prompts.ContainsKey(currentRoundIndex) ? selectedCabinet.currentChainData.prompts[currentRoundIndex] : new PlayerTextInputData();
            }

            GameManager.Instance.gameDataHandler.CmdPrompt(selectedCabinet.currentChainData.identifier, currentRoundIndex, SettingsManager.Instance.birdName, prompt.text, prompt.timeTaken);
            canGetCabinet = true;

        }

        public int GetCabinetID(BirdName player)
        {
            
            foreach (KeyValuePair<int, CabinetDrawer> drawer in cabinetDrawerMap)
            {
                int caseID = drawer.Value.currentChainData.identifier;
                int currentRoundIndex = currentRoundMap.ContainsKey(caseID) ? currentRoundMap[caseID] : -1;

                if (drawer.Value.currentChainData.playerOrder.ContainsKey(currentRoundIndex) &&
                    drawer.Value.currentChainData.playerOrder[currentRoundIndex] == player)
                {
                    return drawer.Key;
                }
            }
            Debug.LogError("ERROR[GetCabinetID]: Could not isolate cabinet ID. Double check that the currentRoundMap is working.");
            return -1;
        }

        public void UpdateNumberOfCases(int numberOfCases)
        {
            SettingsManager.Instance.gameMode.numberOfCases = numberOfCases;
            casesRemainingText.text = numberOfCases.ToString() + " cases\n remaining";
        }

        public void ForcePlayerToSubmit(BirdName player)
        {
            
            GameManager.Instance.gameFlowManager.addTransitionCondition("force_submit:" + player.ToString());
            if (SettingsManager.Instance.birdName == player)
            {
                Submit(true);
            }
            else
            {
                int cabinetID = -1;
                switch (currentState)
                {
                    case CaseState.drawing:
                    case CaseState.prompting:
                    case CaseState.guessing:
                        foreach (CabinetDrawer drawer in cabinetDrawerMap.Values)
                        {
                            if (drawer.currentChainData.active)
                            {
                                int caseID = drawer.currentChainData.identifier;
                                int currentRoundIndex = currentRoundMap.ContainsKey(caseID) ? currentRoundMap[caseID] : -1;
                                if(drawer.currentChainData.playerOrder[currentRoundIndex] == player)
                                {
                                    cabinetID = drawer.id;
                                }
                            }
                        }
                        break;
                    default:
                        return;
                }


                if (cabinetID == -1)
                {
                    Debug.LogError("Could not match cabinet ID to player[" + player.ToString() + "] to request a forced submission.");
                }
                GameManager.Instance.gameDataHandler.TargetForceSubmit(SettingsManager.Instance.birdConnectionMap[player]);
            }
        }

        public void SetBirdArmTargetPosition(BirdName inBirdName, Vector3 inPosition)
        {
            if (inBirdName == SettingsManager.Instance.birdName)
            {
                return;
            }

            GetBirdArm(inBirdName).targetPosition = inPosition;
        }

        public void SendEmptyDrawingToClient(int caseID, int round, BirdName nextPlayer, BirdName author)
        {
            GameManager.Instance.gameDataHandler.TargetEmptyDrawing(SettingsManager.Instance.birdConnectionMap[nextPlayer], caseID, round, author);
        }

        public void SetInitialPrompt(int caseID, string prompt, bool requiresConfirmation)
        {
            CabinetDrawer selectedCabinet = cabinetDrawerMap[playerCabinetIndex];
            ChainData selectedCabinetData = new ChainData();

            if (!caseMap.ContainsKey(caseID))
            {
                selectedCabinet.currentChainData = selectedCabinetData;
                caseMap.Add(caseID, selectedCabinet.currentChainData);
            }
            else
            {
                selectedCabinetData = caseMap[caseID];
                selectedCabinet.currentChainData = selectedCabinetData;
            }
            Debug.LogError("Setting initial prompt["+prompt+"] for case["+caseID.ToString()+"]");
            selectedCabinetData.identifier = caseID;

            if (!SettingsManager.Instance.isHost)
            {
                selectedCabinetData.correctPrompt = prompt;
                if (requiresConfirmation)
                {
                    GameManager.Instance.gameDataHandler.CmdTransitionCondition("initial_cabinet_prompt_receipt:" + SettingsManager.Instance.birdName);
                }
            }
            drawingCaseFolder.Initialize(prompt);
        }

        public void UpdateCaseRound(int caseID, int round)
        {
            if(!currentRoundMap.ContainsKey(caseID))
            {
                currentRoundMap.Add(caseID, round);
            }
            else
            {
                currentRoundMap[caseID] = round;
            }
        }

        public void SetPrompt(int caseID, int tab, BirdName author, string prompt, float timeTaken)
        {
            ChainData selectedCabinetData;
            BirdName queuedPlayer;
            if (!caseMap.ContainsKey(caseID))
            {
                caseMap.Add(caseID, new ChainData());
            }

            selectedCabinetData = caseMap[caseID];
            selectedCabinetData.identifier = caseID;
            PlayerTextInputData newPromptData = new PlayerTextInputData();
            newPromptData.text = prompt;
            newPromptData.timeTaken = timeTaken;
            newPromptData.author = author;
            if (!selectedCabinetData.prompts.ContainsKey(tab))
            {
                selectedCabinetData.prompts.Add(tab, newPromptData);
            }
            else
            {
                newPromptData = selectedCabinetData.prompts[tab];
            }
            

            if (SettingsManager.Instance.isHost)
            {
                if (prompt == "")
                {
                    StatTracker.Instance.AddEmptySubmitter(author);
                }
                queuedPlayer = selectedCabinetData.playerOrder[tab + 1];
                if (GameManager.Instance.gameFlowManager.disconnectedPlayers.Contains(queuedPlayer))
                {
                    //Next player in the queue has been disconnected
                    //handleEmptyDrawingToServer(DrawingData.DrawingType.cabinet, queuedPlayer, caseID, tab);
                }
                else if (queuedPlayer != SettingsManager.Instance.birdName)
                {
                    GameManager.Instance.gameFlowManager.addTransitionCondition("cabinet_prompt_receipt:" + queuedPlayer);
                    GameManager.Instance.gameDataHandler.TargetCabinetPromptContents(SettingsManager.Instance.birdConnectionMap[queuedPlayer], caseID, tab, newPromptData.author, newPromptData.text);
                }
            }
            else
            {
                GameManager.Instance.gameDataHandler.CmdTransitionCondition("cabinet_prompt_receipt:" + SettingsManager.Instance.birdName);
            }
        }

        public void HandleEmptyDrawingToServer( BirdName author, int caseID, int round)
        {
            BirdName nextInQueue = caseMap[caseID].playerOrder[round + 1];
            if (!caseMap[caseID].drawings.ContainsKey(round))
            {
                caseMap[caseID].addDrawing(round, new DrawingData(caseID, round, author));
            }


            SendEmptyCabinetDrawingToClients(nextInQueue, author, caseID, round);
        }

        public void HandleEmptyDrawingToPlayer(int caseID, int round, BirdName author)
        {
            //Debug.LogError("Adding empty drawing to player for case["+caseID.ToString()+"] on round["+round.ToString()+"].");
            DrawingData emptyDrawingData = new DrawingData(caseID, round, author);
            if (!caseMap[caseID].drawings.ContainsKey(round))
            {
                caseMap[caseID].addDrawing(round, emptyDrawingData);
            }
            GameManager.Instance.gameDataHandler.CmdTransitionCondition("empty_drawing_receipt:" + SettingsManager.Instance.birdName);
        }

        private void HandlePlayerSubmitTask()
        {
            playerIsReady = true;
        }
        private void HandlePlayerStartTask()
        {
            playerIsReady = false;
        }
    }
}