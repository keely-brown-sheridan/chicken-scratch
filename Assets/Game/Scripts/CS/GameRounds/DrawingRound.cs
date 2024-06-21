using Mirror;
using Steamworks;
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
        public bool hasRequestedCaseDetails = false;
        public bool hasSentCaseDetails = false;
        public Dictionary<int,QueuedFolderData> queuedFolderMap = new Dictionary<int, QueuedFolderData>();
        private List<int> queuedCaseFolders = new List<int>();
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

        public float timeInCurrentCase = 0f;

        public UnityAction caseFolderOnStartAction;

        public int currentRound => _currentRound;
        private int _currentRound;

        private bool timeHasExpired = false;


        private void Awake()
        {
            if (!isInitialized)
            {
                Initialize();
            }
        }

        private void Update()
        {
            if(GameManager.Instance.playerFlowManager.currentPhaseName == GameFlowManager.GamePhase.drawing && !playerIsReady)
            {
                timeInCurrentCase += Time.deltaTime;
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
            
            isInitialized = true;
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
                GameManager.Instance.gameFlowManager.totalCompletedCases = 0;
                if (SettingsManager.Instance.gameMode.caseDeliveryMode == GameModeData.CaseDeliveryMode.queue)
                {
                    InitializeCabinetDrawers();
                }
                else
                {
                    GameManager.Instance.gameDataHandler.RpcUpdateNumberOfCases((int)(SettingsManager.Instance.GetCaseCountForDay()));
                    GameManager.Instance.gameDataHandler.RpcActivateCasePile();
                }
                GameManager.Instance.gameFlowManager.timeRemainingInPhase = SettingsManager.Instance.gameMode.totalGameTime;
                GameManager.Instance.gameDataHandler.RpcUpdateTimer(SettingsManager.Instance.gameMode.totalGameTime);
            }
            hasRequestedCaseDetails = false;
            hasSentCaseDetails = false;
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
                        currentScoreModifier = currentChainData.currentScoreModifier,
                        maxScoreModifier = currentChainData.maxScoreModifier
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
            if(queuedCaseFolders.Count == 0)
            {
                Debug.LogError("ERROR: Cannot select and update to a new folder state while there are no queued cases.");
                return;
            }

            int queuedCaseIndex = queuedCaseFolders[0];
            queuedCaseFolders.RemoveAt(0);

            if(!caseMap.ContainsKey(queuedCaseIndex))
            {
                Debug.LogError("ERROR: Cannot select and update to a new folder state for case["+queuedCaseIndex.ToString()+"] because it doesn't exist in the case map.");
                return;
            }

            if(!queuedFolderMap.ContainsKey(queuedCaseIndex))
            {
                Debug.LogError("ERROR: Cannot select and update to a new folder state for case["+queuedCaseIndex.ToString()+"] because it doesn't exist in the queued folder map.");
                return;
            }
            cabinetDrawerMap[playerCabinetIndex].currentChainData = caseMap[queuedCaseIndex];
            cabinetDrawerMap[playerCabinetIndex].currentChainData.identifier = queuedCaseIndex;
            currentState = queuedFolderMap[queuedCaseIndex].queuedState;

            ChainData chainData = cabinetDrawerMap[playerCabinetIndex].currentChainData;
            float currentTaskTime = chainData.currentTaskDuration;
            float scoreDecrement = SettingsManager.Instance.gameMode.scoreModifierDecrement;
            Color inFolderColour = Color.white;
            SetCurrentDrawingRound(queuedFolderMap[queuedCaseIndex]);
            int lastRoundIndex = currentRound-1;
            TaskModifier drawingBoxModifier = TaskModifier.standard;
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
                    promptingCaseFolder.Initialize(chainData.drawings[lastRoundIndex], ForceCaseExpirySubmit);
                    promptingCaseFolder.Show(inFolderColour, currentTaskTime, chainData.currentScoreModifier, chainData.maxScoreModifier, scoreDecrement);

                    break;
                case CaseState.drawing:
                    string prompt;
                    if (chainData.prompts.ContainsKey(lastRoundIndex))
                    {
                        prompt = chainData.prompts[lastRoundIndex].text;
                        //Author is not being set here
                        BirdName lastAuthor = chainData.prompts[lastRoundIndex].author;
                        if (lastAuthor == BirdName.none)
                        {
                            lastAuthor = chainData.playerOrder[lastRoundIndex];
                        }
                        inFolderColour = ColourManager.Instance.birdMap[lastAuthor].folderColour;
                    }
                    else
                    {
                        inFolderColour = ColourManager.Instance.birdMap[SettingsManager.Instance.birdName].folderColour;
                        prompt = chainData.correctPrompt;
                    }
                    foreach(TaskModifier modifier in chainData.currentTaskModifiers)
                    {
                        switch(modifier)
                        {
                            case TaskModifier.shrunk:
                            case TaskModifier.thirds_first:
                            case TaskModifier.thirds_second:
                            case TaskModifier.thirds_third:
                            case TaskModifier.top:
                            case TaskModifier.bottom:
                            case TaskModifier.top_left:
                            case TaskModifier.top_right:
                            case TaskModifier.bottom_left:
                            case TaskModifier.bottom_right:
                                drawingBoxModifier = modifier;
                                break;
                        }
                    }
                    drawingCaseFolder.Initialize(prompt, drawingBoxModifier, ForceCaseExpirySubmit);
                    drawingCaseFolder.Show(inFolderColour, currentTaskTime, chainData.currentScoreModifier, chainData.maxScoreModifier, scoreDecrement);

                    break;
                case CaseState.copy_drawing:
                    if (chainData.drawings.ContainsKey(lastRoundIndex))
                    {
                        inFolderColour = ColourManager.Instance.birdMap[chainData.drawings[lastRoundIndex].author].folderColour;
                    }
                    if(!chainData.drawings.ContainsKey(lastRoundIndex))
                    {
                        Debug.LogError("Could not access drawing for round["+lastRoundIndex.ToString()+"] in case["+chainData.identifier.ToString()+"].");
                    }
                    foreach (TaskModifier modifier in chainData.currentTaskModifiers)
                    {
                        switch (modifier)
                        {
                            case TaskModifier.shrunk:
                            case TaskModifier.thirds_first:
                            case TaskModifier.thirds_second:
                            case TaskModifier.thirds_third:
                            case TaskModifier.top:
                            case TaskModifier.bottom:
                            case TaskModifier.top_left:
                            case TaskModifier.top_right:
                            case TaskModifier.bottom_left:
                            case TaskModifier.bottom_right:
                                drawingBoxModifier = modifier;
                                break;
                        }
                    }
                    copyingCaseFolder.Initialize(chainData.drawings[lastRoundIndex], drawingBoxModifier, ForceCaseExpirySubmit);
                    copyingCaseFolder.Show(inFolderColour, currentTaskTime, chainData.currentScoreModifier, chainData.maxScoreModifier, scoreDecrement);
                    
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
                    foreach (TaskModifier modifier in chainData.currentTaskModifiers)
                    {
                        switch (modifier)
                        {
                            case TaskModifier.shrunk:
                            case TaskModifier.thirds_first:
                            case TaskModifier.thirds_second:
                            case TaskModifier.thirds_third:
                            case TaskModifier.top:
                            case TaskModifier.bottom:
                            case TaskModifier.top_left:
                            case TaskModifier.top_right:
                            case TaskModifier.bottom_left:
                            case TaskModifier.bottom_right:
                                drawingBoxModifier = modifier;
                                break;
                        }
                    }
                    addingCaseFolder.Initialize(chainData.drawings[lastRoundIndex], chainData.correctPrompt, drawingBoxModifier, ForceCaseExpirySubmit);
                    addingCaseFolder.Show(inFolderColour, currentTaskTime, chainData.currentScoreModifier, chainData.maxScoreModifier, scoreDecrement);
                    break;
                case CaseState.guessing:
                    guessingCaseFolder.Initialize(chainData.identifier, chainData.possibleWordsMap, chainData.drawings[lastRoundIndex], ForceCaseExpirySubmit);

                    inFolderColour = ColourManager.Instance.birdMap[chainData.drawings[lastRoundIndex].author].folderColour;

                    guessingCaseFolder.Show(inFolderColour, currentTaskTime, chainData.currentScoreModifier, chainData.maxScoreModifier, scoreDecrement);
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
            int currentRoundIndex = currentRound;
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

                    if (!ReleaseDeskDrawingFolder(selectedCase, timeHasExpired))
                    {
                        Debug.LogError("Failed to release desk drawing folder.");
                        return;
                    }
                    break;
                case CaseState.prompting:
                    if (!ReleaseDeskPromptFolder(selectedCabinet, timeHasExpired))
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

                    if (!ReleaseDeskCopyingFolder(selectedCase, timeHasExpired))
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

                    if (!ReleaseDeskAddingFolder(selectedCase, timeHasExpired))
                    {
                        Debug.LogError("Failed to release desk adding folder.");
                        return;
                    }
                    break;
                case CaseState.guessing:
                    if(timeHasExpired)
                    {
                        guessingCaseFolder.ForceGuess(selectedCase.identifier);
                    }
                    else
                    {
                        guessingCaseFolder.ChooseGuess(selectedCase.identifier);
                    }
                    
                    break;
            }

            Submit(false);
            timeHasExpired = false;
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
                Debug.LogError("Cannot release the drawing folder.");
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

            float timeUsed = SettingsManager.Instance.gameMode.totalGameTime - GameManager.Instance.playerFlowManager.currentTimeInRound;

            int currentRoundIndex = currentRound;
            if(currentRoundIndex == -1)
            {
                Debug.LogError("ERROR[ReleaseDeskDrawingFolder]: Could not isolate currentRoundIndex for case["+currentChain.identifier.ToString()+"]");
            }
            DrawingData newDrawing;
            if (!currentChain.drawings.ContainsKey(currentRoundIndex))
            {
                currentChain.drawings.Add(currentRoundIndex, new DrawingData());
            }
            newDrawing = currentChain.drawings[currentRoundIndex];
            newDrawing.caseID = currentChain.identifier;
            newDrawing.round = currentRoundIndex;
            newDrawing.author = SettingsManager.Instance.birdName;
            newDrawing.timeTaken = timeUsed;
            newDrawing.visuals = drawingCaseFolder.GetVisuals();

            return true;
        }

        private bool ReleaseDeskCopyingFolder(ChainData currentChain, bool force)
        {
            if (!copyingCaseFolder.HasStarted() && !force)
            {
                Debug.LogError("Cannot release the copying folder.");
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

            float timeUsed = SettingsManager.Instance.gameMode.totalGameTime - GameManager.Instance.playerFlowManager.currentTimeInRound;

            int currentRoundIndex = currentRound;
            if(currentRoundIndex == -1)
            {
                Debug.LogError("Error[ReleaseDeskCopyingFolder]: Could not isolate currentRoundIndex for case["+currentChain.identifier.ToString()+"]");
            }
            DrawingData newDrawing;
            if (!currentChain.drawings.ContainsKey(currentRoundIndex))
            {
                currentChain.drawings.Add(currentRoundIndex, new DrawingData());
                
            }
            newDrawing = currentChain.drawings[currentRoundIndex];
            newDrawing.caseID = currentChain.identifier;
            newDrawing.round = currentRoundIndex;
            newDrawing.author = SettingsManager.Instance.birdName;
            newDrawing.timeTaken = timeUsed;
            newDrawing.visuals = copyingCaseFolder.GetVisuals();

            return true;
        }

        private bool ReleaseDeskAddingFolder(ChainData currentChain, bool force)
        {
            if (!addingCaseFolder.HasStarted() && !force)
            {
                Debug.LogError("Cannot release the adding folder.");
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

            float timeUsed = SettingsManager.Instance.gameMode.totalGameTime - GameManager.Instance.playerFlowManager.currentTimeInRound;

            int currentRoundIndex = currentRound;
            if(currentRoundIndex == -1)
            {
                Debug.LogError("ERROR[ReleaseDeskAddingFolder]: Could not isolate currentRoundIndex for case[" + currentChain.identifier.ToString() +"]");
            }
            DrawingData newDrawing;
            if (!currentChain.drawings.ContainsKey(currentRoundIndex))
            {
                currentChain.drawings.Add(currentRoundIndex, new DrawingData());
            }
            newDrawing = currentChain.drawings[currentRoundIndex];
            newDrawing.caseID = currentChain.identifier;
            newDrawing.round = currentRoundIndex;
            newDrawing.author = SettingsManager.Instance.birdName;
            newDrawing.timeTaken = timeUsed;
            newDrawing.visuals = addingCaseFolder.GetVisuals();

            return true;
        }

        private bool ReleaseDeskPromptFolder(CabinetDrawer selectedCabinet, bool force)
        {
            if (!promptingCaseFolder.HasStarted() && !force)
            {
                Debug.LogError("Cannot release prompting folder.");
                return false;
            }

            PlayerTextInputData prompt;
            int caseID = selectedCabinet.currentChainData.identifier;
            int currentRoundIndex = currentRound;
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

            prompt.timeTaken = timeInCurrentCase;
            prompt.text = GameDelim.stripGameDelims(promptingCaseFolder.GetPromptText());
            
            return true;
        }

        public void Submit(bool force)
        {
            if(force && playerIsReady)
            {
                //The player does not have a cabinet ready, do not try to submit something
                GameManager.Instance.gameDataHandler.CmdTransitionCondition("force_submit:" + SettingsManager.Instance.birdName);
                return;
            }
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
                        int currentRoundIndex = currentRound;
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
                    if(force)
                    {
                        ReleaseDeskDrawingFolder(cabinetDrawerMap[playerCabinetIndex].currentChainData, true);
                    }
                    AudioManager.Instance.PlaySound("Stamp");
                    SubmitDrawing();
                    drawingCaseFolder.Hide();
                    currentScoreModifier = drawingCaseFolder.GetScoreModifier();
                    break;
                case CaseState.prompting:
                    AudioManager.Instance.PlaySound("Stamp");
                    SubmitPrompt(cabinetDrawerMap[playerCabinetIndex], force);
                    promptingCaseFolder.Hide();
                    currentScoreModifier = promptingCaseFolder.GetScoreModifier();
                    break;
                case CaseState.guessing:
                    if(force)
                    {
                        guessingCaseFolder.ForceGuess(cabinetDrawerMap[playerCabinetIndex].currentChainData.identifier);
                    }
                    guessingCaseFolder.Hide();
                    currentScoreModifier = guessingCaseFolder.GetScoreModifier();
                    break;
                case CaseState.copy_drawing:
                    if(force)
                    {
                        ReleaseDeskCopyingFolder(cabinetDrawerMap[playerCabinetIndex].currentChainData, true);
                    }
                    AudioManager.Instance.PlaySound("Stamp");
                    SubmitDrawing();
                    copyingCaseFolder.Hide();
                    currentScoreModifier = copyingCaseFolder.GetScoreModifier();
                    break;
                case CaseState.add_drawing:
                    if(force)
                    {
                        ReleaseDeskAddingFolder(cabinetDrawerMap[playerCabinetIndex].currentChainData, true);
                    }
                    AudioManager.Instance.PlaySound("Stamp");
                    SubmitDrawing();
                    addingCaseFolder.Hide();
                    currentScoreModifier = addingCaseFolder.GetScoreModifier();
                    break;
            }

            caseID = cabinetDrawerMap[playerCabinetIndex].currentChainData.identifier;
            GameManager.Instance.gameDataHandler.CmdUpdateCaseScoreModifier(caseID, currentScoreModifier);
            if (force)
            {
                GameManager.Instance.gameDataHandler.CmdTransitionCondition("force_submit:" + SettingsManager.Instance.birdName);
            }
            else
            {
                GameManager.Instance.playerFlowManager.drawingRound.onPlayerSubmitTask.Invoke();
                if (SettingsManager.Instance.gameMode.caseDeliveryMode == GameModeData.CaseDeliveryMode.free_for_all)
                {
                    if (currentState != CaseState.guessing)
                    {
                        GameManager.Instance.gameDataHandler.CmdTransitionCase(caseID);
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
                NetworkConnectionToClient birdConnection = SettingsManager.Instance.GetConnection(birdToSendTo);
                switch (queuedTaskData.taskType)
                {
                    case TaskType.prompt_drawing:
                        //Send them the prompt?
                        caseState = CaseState.drawing;
                        break;
                    case TaskType.prompting:
                        //Send them the drawing
                        caseState = CaseState.prompting;
                        break;
                    case TaskType.copy_drawing:
                        //Send them the drawing
                        caseState = CaseState.copy_drawing;
                        break;
                    case TaskType.add_drawing:
                        caseState = CaseState.add_drawing;
                        //Send them the original prompt
                        GameManager.Instance.gameDataHandler.TargetInitialCabinetPromptContents(birdConnection, currentCaseIndex, currentCase.correctPrompt, false);
                        break;
                    case TaskType.base_drawing:
                    case TaskType.compile_drawing:
                        //Send them the prompt
                        caseState = CaseState.drawing;
                        break;
                    case TaskType.base_guessing:
                        //Send them the possible prompts and drawing
                        caseState = CaseState.guessing;
                        break;
                }
                BirdName lastBird;
                if (currentCase.playerOrder.ContainsKey(currentRound-1))
                {
                    lastBird = currentCase.playerOrder[currentRound - 1];
                }
                else
                {
                    lastBird = birdToSendTo;
                }
                FolderUpdateData folderUpdateData = new FolderUpdateData()
                {
                    cabinetIndex = cabinetIndex,
                    currentState = caseState,
                    roundNumber = currentRound,
                    caseID = currentCaseIndex,
                    player = birdToSendTo,
                    taskTime = queuedTaskData.duration,
                    currentScoreModifier = currentCase.currentScoreModifier,
                    maxScoreModifier = currentCase.maxScoreModifier,
                    taskModifiers = currentCase.taskQueue[currentRound-1].modifiers,
                    lastPlayer = lastBird
                };
                GameManager.Instance.gameDataHandler.RpcUpdateFolderAsReady(folderUpdateData);
            }
            else
            {
                //If there are no queued cases then close the drawer
                GameManager.Instance.gameDataHandler.RpcCloseCabinetDrawer(cabinetIndex);
            }
        }

        public void StartChoiceCaseDrawing(int cabinetIndex, int caseID, string prompt, float taskTime, float currentModifierValue, float maxModifierValue, float modifierDecrement, TaskModifier drawingBoxModifier)
        {
            currentState = CaseState.drawing;

            ChainData newChain;
            if (!caseMap.ContainsKey(caseID))
            {
                newChain = new ChainData();
                caseMap.Add(caseID, newChain);
            }
            else
            {
                newChain = caseMap[caseID];
            }
            newChain.identifier = caseID;
            cabinetDrawerMap[cabinetIndex].currentChainData = newChain;
            Color folderColour = ColourManager.Instance.birdMap[SettingsManager.Instance.birdName].folderColour;
            SetCurrentDrawingRound(1);
            drawingCaseFolder.Initialize(prompt, drawingBoxModifier, ForceCaseExpirySubmit);
            drawingCaseFolder.Show(folderColour, taskTime, currentModifierValue, maxModifierValue, modifierDecrement);
        }

        private void ForceCaseExpirySubmit()
        {
            timeHasExpired = true;
            ReleaseDeskFolder();
        }

        private void SubmitDrawing()
        {
            ChainData selectedCase = cabinetDrawerMap[playerCabinetIndex].currentChainData;
            DrawingData newDrawing;
            int currentRoundIndex = currentRound;
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
            newDrawing.timeTaken = timeInCurrentCase;
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

        public void ShowCaseChoices(List<CaseChoiceNetData> choices)
        {
            caseChoicePanel.SetChoices(choices[0], choices[1], choices[2]);
        }

        private void SendEmptyDrawingToServer(BirdName author, int caseID, int round)
        {
            GameManager.Instance.gameDataHandler.CmdEmptyDrawing(author, caseID, round);
        }

        private void SubmitPrompt(CabinetDrawer selectedCabinet, bool force)
        {
            
            int caseID = selectedCabinet.currentChainData.identifier;
            int currentRoundIndex = currentRound;
            if (currentRoundIndex == -1)
            {
                Debug.LogError("ERROR[SubmitPrompt]: Could not isolate currentRoundIndex for case["+caseID.ToString()+"]");
            }
            PlayerTextInputData prompt;
            if (force)
            {
                ReleaseDeskPromptFolder(selectedCabinet, force);
                prompt = selectedCabinet.currentChainData.prompts.ContainsKey(currentRoundIndex) ? selectedCabinet.currentChainData.prompts[currentRoundIndex] : new PlayerTextInputData();
            }
            else
            {
                prompt = selectedCabinet.currentChainData.prompts.ContainsKey(currentRoundIndex) ? selectedCabinet.currentChainData.prompts[currentRoundIndex] : new PlayerTextInputData();
                canGetCabinet = true;
            }

            GameManager.Instance.gameDataHandler.CmdPrompt(selectedCabinet.currentChainData.identifier, currentRoundIndex, SettingsManager.Instance.birdName, prompt.text, prompt.timeTaken);
            

        }

        public int GetCabinetID(BirdName player)
        {
            
            foreach (KeyValuePair<int, CabinetDrawer> drawer in cabinetDrawerMap)
            {
                int caseID = drawer.Value.currentChainData.identifier;
                int currentRoundIndex = currentRound;

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
            SettingsManager.Instance.gameMode.casesRemaining = numberOfCases;
            casesRemainingText.text = numberOfCases.ToString() + " cases\n remaining";
            if(numberOfCases <= 0)
            {
                //Turn off the casepile
                newCaseCabinet.Deactivate();
            }
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
                                int currentRoundIndex = currentRound;
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
                GameManager.Instance.gameDataHandler.TargetForceSubmit(SettingsManager.Instance.GetConnection(player));
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
            GameManager.Instance.gameDataHandler.TargetEmptyDrawing(SettingsManager.Instance.GetConnection(nextPlayer), caseID, round, author);
        }

        public void SetInitialPrompt(int caseID, string prompt)
        {
            CabinetDrawer selectedCabinet = cabinetDrawerMap[playerCabinetIndex];
            ChainData selectedCabinetData = new ChainData();

            if (!caseMap.ContainsKey(caseID))
            {
                caseMap.Add(caseID, selectedCabinet.currentChainData);
            }
            else
            {
                selectedCabinetData = caseMap[caseID];
            }
            selectedCabinetData.identifier = caseID;
            selectedCabinetData.correctPrompt = prompt;
        }

        public void UpdateQueuedFolder(int caseID, int roundNumber, CaseState currentState)
        {
            QueuedFolderData queuedFolderData = new QueuedFolderData();
            queuedFolderData.round = roundNumber;
            queuedFolderData.queuedState = currentState;
            if(!queuedFolderMap.ContainsKey(caseID))
            {
                queuedFolderMap.Add(caseID, queuedFolderData);
            }
            else
            {
                queuedFolderMap[caseID] = queuedFolderData;
            }
            if(!queuedCaseFolders.Contains(caseID))
            {
                queuedCaseFolders.Add(caseID);
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
                if(selectedCabinetData.playerOrder.Count <= (tab+1))
                {
                    Debug.LogError("Could not access player order["+selectedCabinetData.playerOrder.Count.ToString()+"] for round["+(tab+1).ToString()+"] on case["+caseID.ToString()+"]");
                    
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
                    GameManager.Instance.gameDataHandler.TargetCabinetPromptContents(SettingsManager.Instance.GetConnection(queuedPlayer), caseID, tab, newPromptData.author, newPromptData.text);
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
            timeInCurrentCase = 0f;
            playerIsReady = false;
        }

 
        public void SetCurrentDrawingRound(QueuedFolderData folderData)
        {
            _currentRound = folderData.round;
        }

        public void SetCurrentDrawingRound(int newRound)
        {
            _currentRound = newRound;
        }
    }
}