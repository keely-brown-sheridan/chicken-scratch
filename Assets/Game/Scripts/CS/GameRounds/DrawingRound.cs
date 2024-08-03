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
            prompting, drawing, copy_drawing, add_drawing, waiting, guessing, blender_drawing, morph_guessing, competition_guessing, binary_guessing, invalid
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
        private List<int> queuedCaseFolderIdentifiers = new List<int>();
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
        private BlendingCaseFolder blendingCaseFolder;

        [SerializeField]
        private GuessingCaseFolder guessingCaseFolder;

        [SerializeField]
        private BinaryCaseFolder binaryGuessingCaseFolder;

        [SerializeField]
        private MorphingGuessCaseFolder morphGuessCaseFolder;

        [SerializeField]
        private CompetingCaseFolder competingCaseFolder;

        [SerializeField]
        private TMPro.TMP_Text casesRemainingText;

        private Dictionary<BirdName, BirdArm> birdArmMap;

        [SerializeField]
        private CaseChoicePanel caseChoicePanel;

        [SerializeField]
        private ScoreTracker scoreTracker;

        public CasePile newCaseCabinet;

        public bool stampIsActive = false;

        public float timeInCurrentCase = 0f;

        public UnityAction caseFolderOnStartAction;

        public TMPro.TMP_Text birdbucksText;

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

            foreach(CabinetDrawer cabinet in cabinetDrawerMap.Values)
            {
                cabinet.ResetCabinetTask();
            }

            queuedCaseFolderIdentifiers.Clear();
            queuedFolderMap.Clear();
            birdbucksText.text = GameManager.Instance.playerFlowManager.storeRound.currentMoney.ToString();
            if (SettingsManager.Instance.isHost)
            {
                GameManager.Instance.gameFlowManager.totalCompletedCases = 0;
                GameManager.Instance.gameDataHandler.RpcUpdateNumberOfCases(GameManager.Instance.playerFlowManager.GetCasesForDay());
                GameManager.Instance.gameDataHandler.RpcActivateCasePile();

                GameManager.Instance.gameFlowManager.timeRemainingInPhase = GameManager.Instance.playerFlowManager.GetTimeInDay();
                GameManager.Instance.gameDataHandler.RpcUpdateTimer(GameManager.Instance.playerFlowManager.GetTimeInDay());
            }
            hasRequestedCaseDetails = false;
            hasSentCaseDetails = false;
            //Start the clock
            GameManager.Instance.playerFlowManager.loadingCircleObject.SetActive(false);

            if(GameManager.Instance.playerFlowManager.HasStoreItem(StoreItem.StoreItemType.score_tracker))
            {
                scoreTracker.Reset();
                scoreTracker.gameObject.SetActive(true);
            }

            if(SettingsManager.Instance.GetSetting("stickies"))
            {
                TutorialSticky caseCabinetSticky = GameManager.Instance.playerFlowManager.instructionRound.caseCabinetSticky;
                if(!caseCabinetSticky.hasBeenPlaced)
                {
                    caseCabinetSticky.Queue(true);
                }
            }
        }

        public void SetDrawerAsClosed(int cabinetIndex)
        {
            if(!cabinetDrawerMap.ContainsKey(cabinetIndex))
            {
                Debug.LogError("Could not set drawer as closed because cabinetDrawerMap does not contain cabinetIndex["+cabinetIndex.ToString()+"]");
                return;
            }
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
            
            if(queuedCaseFolderIdentifiers.Count == 0)
            {
                Debug.LogError("ERROR: Cannot select and update to a new folder state while there are no queued cases.");
                return;
            }

            int queuedCaseIndex = queuedCaseFolderIdentifiers[0];
            queuedCaseFolderIdentifiers.RemoveAt(0);

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
            for (int i = 0; i < queuedFolderMap[queuedCaseIndex].playerOrder.Count; i++)
            {
                if(!chainData.playerOrder.ContainsKey(i+1))
                {
                    chainData.playerOrder.Add(i + 1, queuedFolderMap[queuedCaseIndex].playerOrder[i]);
                }
            }
            if (!StatTracker.Instance.uniqueCaseChoiceIdentifiers.Contains(chainData.caseTypeName))
            {
                StatTracker.Instance.uniqueCaseChoiceIdentifiers.Add(chainData.caseTypeName);
            }
            float currentTaskTime = chainData.currentTaskDuration;
            float scoreDecrement = chainData.scoreModifierDecrement;
            Color inFolderColour = Color.white;
            SetCurrentDrawingRound(queuedFolderMap[queuedCaseIndex]);
            int lastRoundIndex = currentRound-1;

            if(lastRoundIndex == -1)
            {
                Debug.LogError("ERROR[UpdateToNewFolderState]: Last round index could not be set for case[" + chainData.identifier + "].");
            }
            stampIsActive = false;
            BirdName lastDrawer;
            BirdData lastDrawerBird;
            int firstDrawingRound = -1;
            int secondDrawingRound = -1;
            int thirdDrawingRound = -1;
            GameManager.Instance.gameDataHandler.CmdSetPlayerCabinetTask(SettingsManager.Instance.birdName, currentState);
            switch (currentState)
            {
                case CaseState.prompting:
                    int mostRecentDrawingTask = -1;
                    foreach(int requiredTask in chainData.requiredTasks)
                    {
                        if (chainData.drawings.ContainsKey(requiredTask))
                        {
                            if(mostRecentDrawingTask < requiredTask)
                            {
                                mostRecentDrawingTask = requiredTask;
                            }

                        }
                    }
                    if(mostRecentDrawingTask == -1)
                    {
                        Debug.LogError("Could not find a required task with a drawing.");
                    }
                    BirdName drawingAuthor = chainData.drawings[mostRecentDrawingTask].author;
                    BirdData drawingBird = GameDataManager.Instance.GetBird(drawingAuthor);
                    if (drawingBird == null)
                    {
                        Debug.LogError("Could not set folder colour because the drawing author[" + drawingAuthor.ToString() + "] was not mapped in the Colour Manager.");
                    }
                    else
                    {
                        inFolderColour = drawingBird.folderColour;
                    }
                    int promptingLength = -1;
                    if(chainData.currentTaskModifiers.Contains(TaskModifier.capped_prompt))
                    {
                        promptingLength = 8;
                    }
                    promptingCaseFolder.Initialize(queuedFolderMap[queuedCaseIndex].round, chainData.drawings[mostRecentDrawingTask], ForceCaseExpirySubmit, chainData.currentTaskModifiers, promptingLength);
                    if (GameManager.Instance.playerFlowManager.HasStoreItem(StoreItem.StoreItemType.category_preview))
                    {
                        promptingCaseFolder.ShowCategory(queuedFolderMap[queuedCaseIndex].wordCategory);
                    }
                    promptingCaseFolder.Show(inFolderColour, currentTaskTime, chainData.currentScoreModifier, chainData.maxScoreModifier, scoreDecrement);

                    break;
                case CaseState.drawing:
                    string prompt = "";
                    if (chainData.currentTaskType == TaskType.base_drawing)
                    {
                        BirdData playerBird = GameDataManager.Instance.GetBird(SettingsManager.Instance.birdName);
                        if (playerBird == null)
                        {
                            Debug.LogError("Could not update folder colour because player bird[" + SettingsManager.Instance.birdName.ToString() + "] was not mapped in the Colour Manager.");
                        }
                        else
                        {
                            inFolderColour = playerBird.folderColour;
                        }


                        if (chainData.currentTaskModifiers.Contains(TaskModifier.hidden_prefix))
                        {
                            CaseWordData promptWord = GameDataManager.Instance.GetWord(chainData.correctWordIdentifierMap[2]);
                            if (promptWord != null)
                            {
                                prompt = promptWord.value;
                            }
                            else
                            {
                                prompt = "";
                            }
                        }
                        else if (chainData.currentTaskModifiers.Contains(TaskModifier.hidden_noun))
                        {
                            CaseWordData promptWord = GameDataManager.Instance.GetWord(chainData.correctWordIdentifierMap[1]);
                            if (promptWord != null)
                            {
                                prompt = promptWord.value;
                            }
                            else
                            {
                                prompt = "";
                            }
                        }
                        else
                        {
                            prompt = chainData.correctPrompt;
                        }
                    }
                    else 
                    {
                        int lastPromptingIndex = -1;
                        foreach(int requiredIndex in chainData.requiredTasks)
                        {
                            if(chainData.prompts.ContainsKey(requiredIndex))
                            {
                                prompt += chainData.prompts[requiredIndex].text + " ";
                                if(lastPromptingIndex < requiredIndex)
                                {
                                    lastPromptingIndex = requiredIndex;
                                }
                            }
                        }
                        prompt = prompt.Trim();
                        //Author is not being set here
                        BirdName lastAuthor = chainData.prompts[lastPromptingIndex].author;
                        if (lastAuthor == BirdName.none)
                        {
                            lastAuthor = chainData.playerOrder[lastPromptingIndex];
                        }
                        BirdData lastBird = GameDataManager.Instance.GetBird(lastAuthor);
                        if (lastBird == null)
                        {
                            Debug.LogError("Could not update folder colour because last bird[" + lastBird.ToString() + "] was not mapped in the Colour Manager.");
                        }
                        else
                        {
                            inFolderColour = lastBird.folderColour;
                        }
                    }


                    drawingCaseFolder.Initialize(chainData.identifier, queuedFolderMap[queuedCaseIndex].round, prompt, chainData.currentTaskModifiers, ForceCaseExpirySubmit);
                    if (GameManager.Instance.playerFlowManager.HasStoreItem(StoreItem.StoreItemType.category_preview))
                    {
                        drawingCaseFolder.ShowCategory(queuedFolderMap[queuedCaseIndex].wordCategory);
                    }
                    drawingCaseFolder.Show(inFolderColour, currentTaskTime, chainData.currentScoreModifier, chainData.maxScoreModifier, scoreDecrement);

                    break;
                case CaseState.copy_drawing:
                    if (chainData.drawings.ContainsKey(lastRoundIndex))
                    {
                        lastDrawer = chainData.drawings[lastRoundIndex].author;
                        lastDrawerBird = GameDataManager.Instance.GetBird(lastDrawer);
                        if(lastDrawerBird == null)
                        {
                            Debug.LogError("Could not update folder colour because last drawer bird["+lastDrawer.ToString()+"] was not mapped in the Colour Manager.");
                        }
                        else
                        {
                            inFolderColour = lastDrawerBird.folderColour;
                        }
                        
                    }
                    if(!chainData.drawings.ContainsKey(lastRoundIndex))
                    {
                        Debug.LogError("Could not access drawing for round["+lastRoundIndex.ToString()+"] in case["+chainData.identifier.ToString()+"].");
                    }

                    if(chainData.currentTaskModifiers.Contains(TaskModifier.morphed))
                    {
                        //Get the variant word and then give it to the copying folder
                        CaseWordData variantWord = GameDataManager.Instance.GetWord(chainData.correctWordIdentifierMap[1]);
                        if(variantWord != null)
                        {
                            copyingCaseFolder.Initialize(queuedFolderMap[queuedCaseIndex].round, chainData.drawings[lastRoundIndex], chainData.currentTaskModifiers, ForceCaseExpirySubmit, variantWord.value);
                        }

                    }
                    else
                    {
                        copyingCaseFolder.Initialize(queuedFolderMap[queuedCaseIndex].round, chainData.drawings[lastRoundIndex], chainData.currentTaskModifiers, ForceCaseExpirySubmit);
                    }

                    if (GameManager.Instance.playerFlowManager.HasStoreItem(StoreItem.StoreItemType.category_preview))
                    {
                        copyingCaseFolder.ShowCategory(queuedFolderMap[queuedCaseIndex].wordCategory);
                    }
                    copyingCaseFolder.Show(inFolderColour, currentTaskTime, chainData.currentScoreModifier, chainData.maxScoreModifier, scoreDecrement);
                    
                    break;
                case CaseState.add_drawing:
                    if (!chainData.drawings.ContainsKey(lastRoundIndex))
                    {
                        Debug.LogError("Could not access drawing for round[" + lastRoundIndex.ToString() + "] in case[" + chainData.identifier.ToString() + "].");
                    }
                    if (chainData.drawings.ContainsKey(lastRoundIndex))
                    {
                        lastDrawer = chainData.drawings[lastRoundIndex].author;
                        lastDrawerBird = GameDataManager.Instance.GetBird(lastDrawer);
                        if (lastDrawerBird == null)
                        {
                            Debug.LogError("Could not update folder colour because last drawer bird[" + lastDrawer.ToString() + "] was not mapped in the Colour Manager.");
                        }
                        else
                        {
                            inFolderColour = lastDrawerBird.folderColour;
                        }
                    }
                    
                    addingCaseFolder.Initialize(queuedFolderMap[queuedCaseIndex].round, chainData.drawings[lastRoundIndex], chainData.correctPrompt, chainData.currentTaskModifiers, ForceCaseExpirySubmit);
                    if (GameManager.Instance.playerFlowManager.HasStoreItem(StoreItem.StoreItemType.category_preview))
                    {
                        addingCaseFolder.ShowCategory(queuedFolderMap[queuedCaseIndex].wordCategory);
                    }
                    addingCaseFolder.Show(inFolderColour, currentTaskTime, chainData.currentScoreModifier, chainData.maxScoreModifier, scoreDecrement);
                    break;
                case CaseState.blender_drawing:
                    if(chainData.requiredTasks.Count < 2)
                    {
                        Debug.LogError("Blender task could not be initialized because there are not enough required tasks queued[" + chainData.requiredTasks.Count.ToString() + "]");
                        return;
                    }
                    int drawingRound1 = chainData.requiredTasks[0];
                    int drawingRound2 = chainData.requiredTasks[1];
                    if (!chainData.drawings.ContainsKey(drawingRound1))
                    {
                        Debug.LogError("Could not access drawing for round[" + drawingRound1.ToString() + "] in case[" + chainData.identifier.ToString() + "].");
                    }
                    if (!chainData.drawings.ContainsKey(drawingRound2))
                    {
                        Debug.LogError("Could not access drawing for round[" + drawingRound2.ToString() + "] in case[" + chainData.identifier.ToString() + "].");
                    }
                    if (chainData.drawings.ContainsKey(lastRoundIndex))
                    {
                        lastDrawer = chainData.drawings[lastRoundIndex].author;
                        lastDrawerBird = GameDataManager.Instance.GetBird(lastDrawer);
                        if (lastDrawerBird == null)
                        {
                            Debug.LogError("Could not update folder colour because last drawer bird[" + lastDrawer.ToString() + "] was not mapped in the Colour Manager.");
                        }
                        else
                        {
                            inFolderColour = lastDrawerBird.folderColour;
                        }
                    }

                    blendingCaseFolder.Initialize(queuedFolderMap[queuedCaseIndex].round, chainData.drawings[drawingRound1], chainData.drawings[drawingRound2], chainData.currentTaskModifiers, ForceCaseExpirySubmit);
                    if (GameManager.Instance.playerFlowManager.HasStoreItem(StoreItem.StoreItemType.category_preview))
                    {
                        blendingCaseFolder.ShowCategory(queuedFolderMap[queuedCaseIndex].wordCategory);
                    }
                    blendingCaseFolder.Show(inFolderColour, currentTaskTime, chainData.currentScoreModifier, chainData.maxScoreModifier, scoreDecrement);
                    break;
                case CaseState.guessing:
                    guessingCaseFolder.Initialize(queuedFolderMap[queuedCaseIndex].round, chainData.identifier, chainData.possibleWordsMap, chainData.drawings[lastRoundIndex], chainData.currentTaskModifiers, ForceCaseExpirySubmit);
                    if(GameManager.Instance.playerFlowManager.HasStoreItem(StoreItem.StoreItemType.category_preview))
                    {
                        guessingCaseFolder.ShowCategory(queuedFolderMap[queuedCaseIndex].wordCategory);
                    }
                    lastDrawer = chainData.drawings[lastRoundIndex].author;
                    lastDrawerBird = GameDataManager.Instance.GetBird(lastDrawer);
                    if (lastDrawerBird == null)
                    {
                        Debug.LogError("Could not update folder colour because last drawer bird[" + lastDrawer.ToString() + "] was not mapped in the Colour Manager.");
                    }
                    else
                    {
                        inFolderColour = lastDrawerBird.folderColour;
                    }

                    guessingCaseFolder.Show(inFolderColour, currentTaskTime, chainData.currentScoreModifier, chainData.maxScoreModifier, scoreDecrement);
                    break;
                case CaseState.binary_guessing:
                    bool caseState = GameManager.Instance.playerFlowManager.slidesRound.GetBinaryCaseState(chainData.identifier);
                    string possiblePrefix = "";
                    string possibleNoun = "";
                    CaseWordData correctPrefixWord = GameDataManager.Instance.GetWord(chainData.correctWordIdentifierMap[1]);
                    CaseWordData correctNounWord = GameDataManager.Instance.GetWord(chainData.correctWordIdentifierMap[2]);
                    if (caseState)
                    {
                        //use the correct prompt
                        if (correctPrefixWord != null)
                        {
                            possiblePrefix = correctPrefixWord.value;
                        }

                        if (correctNounWord != null)
                        {
                            possibleNoun = correctNounWord.value;
                        }
                    }
                    else
                    {
                        //use the fake prompt
                        if (correctPrefixWord != null)
                        {
                            foreach (string prefix in chainData.possibleWordsMap[1])
                            {
                                if (prefix != correctPrefixWord.value)
                                {
                                    possiblePrefix = prefix;
                                    break;
                                }
                            }
                        }
                        if (correctNounWord != null)
                        {
                            foreach (string noun in chainData.possibleWordsMap[2])
                            {
                                if (noun != correctNounWord.value)
                                {
                                    possibleNoun = noun;
                                    break;
                                }
                            }
                        }
                    }
                    binaryGuessingCaseFolder.Initialize(queuedFolderMap[queuedCaseIndex].round, chainData.identifier, possiblePrefix, possibleNoun, chainData.drawings[lastRoundIndex], chainData.currentTaskModifiers, ForceCaseExpirySubmit);
                    if (GameManager.Instance.playerFlowManager.HasStoreItem(StoreItem.StoreItemType.category_preview))
                    {
                        binaryGuessingCaseFolder.ShowCategory(queuedFolderMap[queuedCaseIndex].wordCategory);
                    }
                    lastDrawer = chainData.drawings[lastRoundIndex].author;
                    lastDrawerBird = GameDataManager.Instance.GetBird(lastDrawer);
                    if (lastDrawerBird == null)
                    {
                        Debug.LogError("Could not update folder colour because last drawer bird[" + lastDrawer.ToString() + "] was not mapped in the Colour Manager.");
                    }
                    else
                    {
                        inFolderColour = lastDrawerBird.folderColour;
                    }

                    binaryGuessingCaseFolder.Show(inFolderColour, currentTaskTime, chainData.currentScoreModifier, chainData.maxScoreModifier, scoreDecrement);
                    break;
                case CaseState.morph_guessing:
                    foreach(int requiredTask in chainData.requiredTasks)
                    {
                        if(chainData.drawings.ContainsKey(requiredTask))
                        {
                            if(firstDrawingRound == -1)
                            {
                                firstDrawingRound = requiredTask;
                            }
                            else if(secondDrawingRound == -1)
                            {
                                secondDrawingRound = requiredTask;
                                break;
                            }
                        }
                    }
                    if(firstDrawingRound == -1 || secondDrawingRound == -1)
                    {
                        Debug.LogError("Could not isolate the two required drawing tasks for the morph guess task.");
                        return;
                    }
                    morphGuessCaseFolder.Initialize(queuedFolderMap[queuedCaseIndex].round, chainData.identifier, chainData.possibleWordsMap, chainData.drawings[firstDrawingRound], chainData.drawings[secondDrawingRound], chainData.currentTaskModifiers, ForceCaseExpirySubmit);
                    if (GameManager.Instance.playerFlowManager.HasStoreItem(StoreItem.StoreItemType.category_preview))
                    {
                        morphGuessCaseFolder.ShowCategory(queuedFolderMap[queuedCaseIndex].wordCategory);
                    }
                    lastDrawer = chainData.drawings[lastRoundIndex].author;
                    lastDrawerBird = GameDataManager.Instance.GetBird(lastDrawer);
                    if (lastDrawerBird == null)
                    {
                        Debug.LogError("Could not update folder colour because last drawer bird[" + lastDrawer.ToString() + "] was not mapped in the Colour Manager.");
                    }
                    else
                    {
                        inFolderColour = lastDrawerBird.folderColour;
                    }

                    morphGuessCaseFolder.Show(inFolderColour, currentTaskTime, chainData.currentScoreModifier, chainData.maxScoreModifier, scoreDecrement);
                    break;
                case CaseState.competition_guessing:
                    
                    foreach (int requiredTask in chainData.requiredTasks)
                    {
                        if (chainData.drawings.ContainsKey(requiredTask))
                        {
                            if (firstDrawingRound == -1)
                            {
                                firstDrawingRound = requiredTask;
                            }
                            else if (secondDrawingRound == -1)
                            {
                                secondDrawingRound = requiredTask;
                            }
                            else if (thirdDrawingRound == -1)
                            {
                                thirdDrawingRound = requiredTask;
                                break;
                            }
                        }
                    }
                    if (firstDrawingRound == -1 || secondDrawingRound == -1)
                    {
                        Debug.LogError("Could not isolate the two required drawing tasks for the competition guess task.");
                        return;
                    }
                    List<DrawingData> drawings = new List<DrawingData>() { chainData.drawings[firstDrawingRound], chainData.drawings[secondDrawingRound] };
                    if(thirdDrawingRound != -1)
                    {
                        drawings.Add(chainData.drawings[thirdDrawingRound]);
                    }
                    competingCaseFolder.Initialize(queuedFolderMap[queuedCaseIndex].round, chainData.identifier, chainData.possibleWordsMap, drawings, chainData.currentTaskModifiers, ForceCaseExpirySubmit);
                    if (GameManager.Instance.playerFlowManager.HasStoreItem(StoreItem.StoreItemType.category_preview))
                    {
                        competingCaseFolder.ShowCategory(queuedFolderMap[queuedCaseIndex].wordCategory);
                    }
                    lastDrawer = chainData.drawings[lastRoundIndex].author;
                    lastDrawerBird = GameDataManager.Instance.GetBird(lastDrawer);
                    if (lastDrawerBird == null)
                    {
                        Debug.LogError("Could not update folder colour because last drawer bird[" + lastDrawer.ToString() + "] was not mapped in the Colour Manager.");
                    }
                    else
                    {
                        inFolderColour = lastDrawerBird.folderColour;
                    }

                    competingCaseFolder.Show(inFolderColour, currentTaskTime, chainData.currentScoreModifier, chainData.maxScoreModifier, scoreDecrement);
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
            if(cabinetDrawerMap.ContainsKey(inIndex))
            {
                return cabinetDrawerMap[inIndex];
            }
            return null;
        }

        public BirdArm GetBirdArm(BirdName inBirdName)
        {
            if(birdArmMap.ContainsKey(inBirdName))
            {
                return birdArmMap[inBirdName];
            }
            return null;
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
            GameManager.Instance.gameDataHandler.CmdSetPlayerCabinetIdle(SettingsManager.Instance.birdName);
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
                case CaseState.blender_drawing:
                    if (!cabinetDrawerMap[playerCabinetIndex].currentChainData.drawings.ContainsKey(currentRoundIndex))
                    {
                        newDrawing = new DrawingData(selectedCase.identifier, currentRoundIndex, SettingsManager.Instance.birdName);
                        selectedCase.drawings.Add(currentRoundIndex, newDrawing);
                    }

                    if (!ReleaseDeskBlendingFolder(selectedCase, timeHasExpired))
                    {
                        Debug.LogError("Failed to release desk blending folder.");
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
                        if(!guessingCaseFolder.ChooseGuess(selectedCase.identifier))
                        {
                            return;
                        }
                    }
                    
                    break;
                case CaseState.binary_guessing:
                    if (timeHasExpired)
                    {
                        binaryGuessingCaseFolder.ForceGuess(selectedCase.identifier);
                    }
                    else
                    {
                        if (!binaryGuessingCaseFolder.ChooseGuess(selectedCase.identifier))
                        {
                            return;
                        }
                    }

                    break;
                case CaseState.morph_guessing:
                    if (timeHasExpired)
                    {
                        morphGuessCaseFolder.ForceGuess(selectedCase.identifier);
                    }
                    else
                    {
                        if (!morphGuessCaseFolder.ChooseGuess(selectedCase.identifier))
                        {
                            return;
                        }
                    }
                    break;
                case CaseState.competition_guessing:
                    if (timeHasExpired)
                    {
                        competingCaseFolder.ForceGuess(selectedCase.identifier);
                    }
                    else
                    {
                        if (!competingCaseFolder.ChooseGuess(selectedCase.identifier))
                        {
                            return;
                        }
                    }
                    break;
            }

            PressSubmit();
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

            float timeUsed = GameManager.Instance.playerFlowManager.GetTimeInDay() - GameManager.Instance.playerFlowManager.currentTimeInRound;

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

            float timeUsed = GameManager.Instance.playerFlowManager.GetTimeInDay() - GameManager.Instance.playerFlowManager.currentTimeInRound;

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

        private bool ReleaseDeskBlendingFolder(ChainData currentChain, bool force)
        {
            if (!blendingCaseFolder.HasStarted() && !force)
            {
                Debug.LogError("Cannot release the blending folder.");
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

            float timeUsed = GameManager.Instance.playerFlowManager.GetTimeInDay() - GameManager.Instance.playerFlowManager.currentTimeInRound;

            int currentRoundIndex = currentRound;
            if (currentRoundIndex == -1)
            {
                Debug.LogError("Error[ReleaseDeskBlendingFolder]: Could not isolate currentRoundIndex for case[" + currentChain.identifier.ToString() + "]");
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
            newDrawing.visuals = blendingCaseFolder.GetVisuals();

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

            float timeUsed = GameManager.Instance.playerFlowManager.GetTimeInDay() - GameManager.Instance.playerFlowManager.currentTimeInRound;

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
            prompt.author = SettingsManager.Instance.birdName;
            
            return true;
        }

        public void PressSubmit()
        {
            switch (currentState)
            {
                case CaseState.drawing:
                    drawingCaseFolder.RegisterToStampComplete(OnSubmitted);
                    drawingCaseFolder.Submit();
                    break;
                case CaseState.prompting:
                    promptingCaseFolder.RegisterToStampComplete(OnSubmitted);
                    promptingCaseFolder.Submit();
                    break;
                case CaseState.guessing:
                    guessingCaseFolder.RegisterToStampComplete(OnSubmitted);
                    guessingCaseFolder.Submit();
                    break;
                case CaseState.binary_guessing:
                    binaryGuessingCaseFolder.RegisterToStampComplete(OnSubmitted);
                    binaryGuessingCaseFolder.Submit();
                    break;
                case CaseState.morph_guessing:
                    morphGuessCaseFolder.RegisterToStampComplete(OnSubmitted);
                    morphGuessCaseFolder.Submit();
                    break;
                case CaseState.competition_guessing:
                    competingCaseFolder.RegisterToStampComplete(OnSubmitted);
                    competingCaseFolder.Submit();
                    break;
                case CaseState.copy_drawing:
                    copyingCaseFolder.RegisterToStampComplete(OnSubmitted);
                    copyingCaseFolder.Submit();
                    break;
                case CaseState.blender_drawing:
                    blendingCaseFolder.RegisterToStampComplete(OnSubmitted);
                    blendingCaseFolder.Submit();
                    break;
                case CaseState.add_drawing:
                    addingCaseFolder.RegisterToStampComplete(OnSubmitted);
                    addingCaseFolder.Submit();
                   
                    break;
            }
        }

        public void OnSubmitted()
        {
            Submit(false);
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
            bool hasCollaborationCertification = GameManager.Instance.playerFlowManager.CaseHasCertification(cabinetDrawerMap[playerCabinetIndex].currentChainData.caseTypeName, "Collaboration");
            bool crossedTimeThreshold = false;
            switch (currentState)
            {
                case CaseState.drawing:
                    if(force)
                    {
                        drawingCaseFolder.PullDownStamp();
                        ReleaseDeskDrawingFolder(cabinetDrawerMap[playerCabinetIndex].currentChainData, true);
                    }

                    SubmitDrawing();
                    drawingCaseFolder.DeregisterToStampComplete(OnSubmitted);
                    drawingCaseFolder.Hide();
                    currentScoreModifier = drawingCaseFolder.GetScoreModifier();
                    crossedTimeThreshold = drawingCaseFolder.HasCrossedThreshold();
                    break;
                case CaseState.prompting:
                    if(force)
                    {
                        promptingCaseFolder.PullDownStamp();
                    }
                    SubmitPrompt(cabinetDrawerMap[playerCabinetIndex], force);
                    promptingCaseFolder.DeregisterToStampComplete(OnSubmitted);
                    promptingCaseFolder.Hide();
                    currentScoreModifier = promptingCaseFolder.GetScoreModifier();
                    crossedTimeThreshold = promptingCaseFolder.HasCrossedThreshold();
                    break;
                case CaseState.guessing:
                    if(force)
                    {
                        guessingCaseFolder.PullDownStamp();
                        guessingCaseFolder.ForceGuess(cabinetDrawerMap[playerCabinetIndex].currentChainData.identifier);
                    }
                    
                    guessingCaseFolder.DeregisterToStampComplete(OnSubmitted);
                    guessingCaseFolder.Hide();
                    currentScoreModifier = guessingCaseFolder.GetScoreModifier();
                    crossedTimeThreshold = guessingCaseFolder.HasCrossedThreshold();
                    break;
                case CaseState.binary_guessing:
                    if (force)
                    {
                        binaryGuessingCaseFolder.PullDownStamp();
                        binaryGuessingCaseFolder.ForceGuess(cabinetDrawerMap[playerCabinetIndex].currentChainData.identifier);
                    }

                    binaryGuessingCaseFolder.DeregisterToStampComplete(OnSubmitted);
                    binaryGuessingCaseFolder.Hide();
                    currentScoreModifier = binaryGuessingCaseFolder.GetScoreModifier();
                    crossedTimeThreshold = binaryGuessingCaseFolder.HasCrossedThreshold();
                    break;
                case CaseState.morph_guessing:
                    if (force)
                    {
                        morphGuessCaseFolder.PullDownStamp();
                        morphGuessCaseFolder.ForceGuess(cabinetDrawerMap[playerCabinetIndex].currentChainData.identifier);
                    }
                    morphGuessCaseFolder.DeregisterToStampComplete(OnSubmitted);
                    morphGuessCaseFolder.Hide();
                    currentScoreModifier = morphGuessCaseFolder.GetScoreModifier();
                    crossedTimeThreshold = morphGuessCaseFolder.HasCrossedThreshold();
                    break;
                case CaseState.competition_guessing:
                    if (force)
                    {
                        competingCaseFolder.PullDownStamp();
                        competingCaseFolder.ForceGuess(cabinetDrawerMap[playerCabinetIndex].currentChainData.identifier);
                    }
                    competingCaseFolder.DeregisterToStampComplete(OnSubmitted);
                    competingCaseFolder.Hide();
                    currentScoreModifier = competingCaseFolder.GetScoreModifier();
                    crossedTimeThreshold = competingCaseFolder.HasCrossedThreshold();
                    break;
                case CaseState.copy_drawing:
                    if(force)
                    {
                        copyingCaseFolder.PullDownStamp();
                        ReleaseDeskCopyingFolder(cabinetDrawerMap[playerCabinetIndex].currentChainData, true);
                    }
                    copyingCaseFolder.DeregisterToStampComplete(OnSubmitted);
                    copyingCaseFolder.Hide();
                    SubmitDrawing();
                    currentScoreModifier = copyingCaseFolder.GetScoreModifier();
                    crossedTimeThreshold = copyingCaseFolder.HasCrossedThreshold();
                    break;
                case CaseState.blender_drawing:
                    if(force)
                    {
                        blendingCaseFolder.PullDownStamp();
                        ReleaseDeskBlendingFolder(cabinetDrawerMap[playerCabinetIndex].currentChainData, true);
                    }
                    blendingCaseFolder.DeregisterToStampComplete(OnSubmitted);
                    blendingCaseFolder.Hide();
                    SubmitDrawing();
                    currentScoreModifier = blendingCaseFolder.GetScoreModifier();
                    crossedTimeThreshold = blendingCaseFolder.HasCrossedThreshold();
                    break;
                case CaseState.add_drawing:
                    if(force)
                    {
                        addingCaseFolder.PullDownStamp();
                        ReleaseDeskAddingFolder(cabinetDrawerMap[playerCabinetIndex].currentChainData, true);
                    }
                    addingCaseFolder.DeregisterToStampComplete(OnSubmitted);
                    addingCaseFolder.Hide();

                    SubmitDrawing();
                    currentScoreModifier = addingCaseFolder.GetScoreModifier();
                    crossedTimeThreshold = addingCaseFolder.HasCrossedThreshold();
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
                if (currentState != CaseState.guessing && currentState != CaseState.morph_guessing && currentState != CaseState.competition_guessing && currentState != CaseState.binary_guessing)
                {
                    GameManager.Instance.gameDataHandler.CmdTransitionCase(caseID);
                }
                else
                {
                    GameManager.Instance.gameDataHandler.CmdSendPointsToScoreTrackerPlayers(caseID);
                }
                GameManager.Instance.gameDataHandler.CmdRequestNextCase(SettingsManager.Instance.birdName);
            }

            if(!crossedTimeThreshold && hasCollaborationCertification)
            {
                int increment = ((IntCertificationData)GameDataManager.Instance.GetCertification("Collaboration")).value;
                GameManager.Instance.gameDataHandler.CmdIncreaseCaseBirdbucks(caseID, increment);
            }
            if (SettingsManager.Instance.GetSetting("stickies"))
            {
                TutorialSticky modifierSticky = GameManager.Instance.playerFlowManager.instructionRound.modifierSticky;
                if(!modifierSticky.hasBeenClicked)
                {
                    modifierSticky.Click();
                }
            }

        }



        public void SendNextInQueue(BirdName birdToSendTo)
        {
            if(!GameManager.Instance.gameFlowManager.playerCabinetMap.ContainsKey(birdToSendTo))
            {
                Debug.LogError("ERROR[SendNextInQueue]: Could not find cabinet associated with player[" + birdToSendTo.ToString() + "]");
                return;
            }
            int cabinetIndex = GameManager.Instance.gameFlowManager.playerCabinetMap[birdToSendTo];
            if(!cabinetDrawerMap.ContainsKey(cabinetIndex))
            {
                Debug.LogError("ERROR[SendNextInQueue]: Could not find cabinet with identifier[" + cabinetIndex.ToString() + "]");
                return;
            }
            
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

                if(currentCase.taskQueue.Count <= currentRound - 1)
                {
                    Debug.LogError("ERROR[SendNextInQueue]: Task queue["+ currentCase.taskQueue.Count.ToString()+"] is not big enough to access previous round["+(currentRound-1).ToString()+"]");
                    return;
                }
                TaskData queuedTaskData = currentCase.taskQueue[currentRound-1];
                NetworkConnectionToClient birdConnection = SettingsManager.Instance.GetConnection(birdToSendTo);

                BirdName lastBird;
                if (currentCase.playerOrder.ContainsKey(currentRound-1))
                {
                    lastBird = currentCase.playerOrder[currentRound - 1];
                }
                else
                {
                    lastBird = birdToSendTo;
                }
                if(lastBird == BirdName.none)
                {
                    Debug.LogError("ERROR[SendNextInQueue]: Previous bird has not been set.");
                    return;
                }

                //Randomly pick between prefix and noun then create the word category for the scanner
                if(!currentCase.correctWordIdentifierMap.ContainsKey(1))
                {
                    Debug.LogError("ERROR[SendNextInQueue]: Correct word identifier map did not contain a prefix.");
                    return;
                }
                if(!currentCase.correctWordIdentifierMap.ContainsKey(2))
                {
                    Debug.LogError("ERROR[SendNextInQueue]: Correct word identifier map did not contain a noun.");
                    return;
                }
                WordCategoryData wordCategory = new WordCategoryData();
                CaseWordData prefix = GameDataManager.Instance.GetWord(currentCase.correctWordIdentifierMap[1]);
                if(prefix == null)
                {
                    Debug.LogError("ERROR[SendNextInQueue]: Prefix could not be isolated for identifier[" + currentCase.correctWordIdentifierMap[1] + "]");
                    return;
                }
                CaseWordData noun = GameDataManager.Instance.GetWord(currentCase.correctWordIdentifierMap[2]);
                if(noun == null)
                {
                    Debug.LogError("ERROR[SendNextInQueue]: Noun could not be isolated for identifier[" + currentCase.correctWordIdentifierMap[2] + "]");
                    return;
                }
                wordCategory.prefixCategory = prefix.category;
                wordCategory.nounCategory = noun.category;

                FolderUpdateData folderUpdateData = new FolderUpdateData()
                {
                    cabinetIndex = cabinetIndex,
                    currentState = queuedTaskData.GetCaseState(),
                    roundNumber = currentRound,
                    caseID = currentCaseIndex,
                    player = birdToSendTo,
                    taskTime = queuedTaskData.duration,
                    currentScoreModifier = currentCase.currentScoreModifier,
                    scoreModifierDecrement = currentCase.scoreModifierDecrement,
                    maxScoreModifier = currentCase.maxScoreModifier,
                    taskModifiers = currentCase.taskQueue[currentRound-1].modifiers,
                    taskType = currentCase.taskQueue[currentRound-1].taskType,
                    lastPlayer = lastBird,
                    wordCategory = wordCategory,
                    caseTypeName = currentCase.caseTypeName,
                    requiredTasks = currentCase.taskQueue[currentRound-1].requiredRounds
                };
                for (int i = 0; i < currentCase.playerOrder.Count; i++)
                {
                    folderUpdateData.playerOrder.Add(currentCase.playerOrder[i + 1]);
                }
                //Send all required contents to client, once they've confirmed to have all of them, then open the cabinet
                GameManager.Instance.gameDataHandler.TargetPrepareForTask(SettingsManager.Instance.GetConnection(birdToSendTo), folderUpdateData);
            }
            else
            {
                //If there are no queued cases then close the drawer
                GameManager.Instance.gameDataHandler.RpcCloseCabinetDrawer(cabinetIndex);
            }
        }

        public void StartChoiceCaseDrawing(int cabinetIndex, int caseID, string prompt, float taskTime, float currentModifierValue, float maxModifierValue, float modifierDecrement, List<TaskModifier> taskModifiers, string caseTypeName, List<BirdName> playerOrder)
        {
            currentState = CaseState.drawing;
            GameManager.Instance.gameDataHandler.CmdSetPlayerCabinetTask(SettingsManager.Instance.birdName, currentState);
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
            for(int i = 0; i < playerOrder.Count; i++)
            {
                if(!newChain.playerOrder.ContainsKey(i+1))
                {
                    newChain.playerOrder.Add(i + 1, playerOrder[i]);
                }
            }
            newChain.identifier = caseID;
            newChain.caseTypeName = caseTypeName;
            
            if(!cabinetDrawerMap.ContainsKey(cabinetIndex))
            {
                Debug.LogError("ERROR[StartChoiceCaseDrawing]: Could not isolate cabinet index["+cabinetIndex.ToString()+"]");
                return;
            }
            cabinetDrawerMap[cabinetIndex].currentChainData = newChain;
            BirdData playerBird = GameDataManager.Instance.GetBird(SettingsManager.Instance.birdName);
            if(playerBird == null)
            {
                Debug.LogError("Could not start choice case drawing because player bird["+SettingsManager.Instance.birdName.ToString()+"] has not been mapped in the Colour Manager.");
                return;
            }
            Color folderColour = playerBird.folderColour;
            SetCurrentDrawingRound(1);
            drawingCaseFolder.Initialize(newChain.identifier, 1, prompt, taskModifiers, ForceCaseExpirySubmit);
            drawingCaseFolder.Show(folderColour, taskTime, currentModifierValue, maxModifierValue, modifierDecrement);

            if (SettingsManager.Instance.GetSetting("stickies"))
            {
                TutorialSticky modifierSticky = GameManager.Instance.playerFlowManager.instructionRound.modifierSticky;
                if (!modifierSticky.hasBeenPlaced)
                {
                    modifierSticky.Queue(true);
                }
            }
        }

        private void ForceCaseExpirySubmit()
        {
            timeHasExpired = true;
            ReleaseDeskFolder();
        }

        private void SubmitDrawing()
        {
            if(!cabinetDrawerMap.ContainsKey(playerCabinetIndex))
            {
                Debug.LogError("ERROR[SubmitDrawing]: Could not find matching cabinet["+playerCabinetIndex.ToString()+"]");
                return;
            }
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
            GameManager.Instance.gameDataHandler.CmdSetPlayerCabinetChoosing(SettingsManager.Instance.birdName);
            if (choices.Count < 3)
            {
                Debug.LogError("ERROR[ShowCaseChoices]: Not enough choices["+choices.Count.ToString()+"] were provided.");
                return;
            }
            caseChoicePanel.SetChoices(choices[0], choices[1], choices[2]);

            if (SettingsManager.Instance.GetSetting("stickies"))
            {
                TutorialSticky choiceSticky = GameManager.Instance.playerFlowManager.instructionRound.choicesSticky2;
                if (!choiceSticky.hasBeenPlaced)
                {
                    choiceSticky.Queue(true);
                    GameManager.Instance.playerFlowManager.instructionRound.choicesSticky3.Queue(true);
                }
            }
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
            GameManager.Instance.playerFlowManager.casesRemaining = numberOfCases;
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
                    case CaseState.morph_guessing:
                    case CaseState.competition_guessing:
                    case CaseState.binary_guessing:
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
            BirdArm birdArm = GetBirdArm(inBirdName);
            if(birdArm != null)
            {
                
                birdArm.targetPosition = inPosition;
            }
        }

        public void SendEmptyDrawingToClient(int caseID, int round, BirdName nextPlayer, BirdName author)
        {
            GameManager.Instance.gameDataHandler.TargetEmptyDrawing(SettingsManager.Instance.GetConnection(nextPlayer), caseID, round, author);
        }

        public void SetInitialPromptForTask(int caseID, int round, string prompt, Dictionary<int,string> correctWordIdentifiersMap)
        {
            if(!cabinetDrawerMap.ContainsKey(playerCabinetIndex))
            {
                Debug.LogError("ERROR[SetInitialPrompt]: Could not find matching cabinet["+playerCabinetIndex.ToString()+"]");
                return;
            }
            CabinetDrawer selectedCabinet = cabinetDrawerMap[playerCabinetIndex];
            ChainData selectedCabinetData = new ChainData();

            if (!caseMap.ContainsKey(caseID))
            {
                caseMap.Add(caseID, selectedCabinet.currentChainData);
            }
            else
            {
                selectedCabinetData = caseMap[caseID];
                if(selectedCabinetData.waitingOnTasks.Contains(0))
                {
                    selectedCabinetData.waitingOnTasks.Remove(0);
                    if(selectedCabinetData.waitingOnTasks.Count == 0)
                    {
                        GameManager.Instance.gameDataHandler.CmdTaskIsReady(SettingsManager.Instance.birdName, caseID, round);
                    }
                }

            }
            selectedCabinetData.identifier = caseID;
            selectedCabinetData.correctPrompt = prompt;
            selectedCabinetData.correctWordIdentifierMap = correctWordIdentifiersMap;
        }

        public void UpdateQueuedFolder(int caseID, int roundNumber, CaseState currentState, WordCategoryData wordCategory, List<BirdName> playerOrder)
        {
            QueuedFolderData queuedFolderData = new QueuedFolderData();
            queuedFolderData.round = roundNumber;
            queuedFolderData.queuedState = currentState;
            queuedFolderData.wordCategory = wordCategory;
            queuedFolderData.playerOrder = playerOrder;
            if(!queuedFolderMap.ContainsKey(caseID))
            {
                queuedFolderMap.Add(caseID, queuedFolderData);
            }
            else
            {
                queuedFolderMap[caseID] = queuedFolderData;
            }
            if(!queuedCaseFolderIdentifiers.Contains(caseID))
            {
                queuedCaseFolderIdentifiers.Add(caseID);
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
            if(!caseMap.ContainsKey(caseID))
            {
                Debug.LogError("ERROR[HandleEmptyDrawingToServer]: Missing case["+caseID.ToString()+"] from case map.");
                return;
            }
            if (caseMap[caseID].playerOrder.Count <= (round+1))
            {
                Debug.LogError("ERROR[HandleEmptyDrawingToServer]: Player order[" + caseMap[caseID].playerOrder.Count.ToString() +"] for case["+caseID.ToString()+"] is too small for round["+(round+1).ToString()+"]");
            }
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

        public void UpdateScoreTrackerPoints(int trackerPoints)
        {
            scoreTracker.IncreaseEarnedBonusBucks(trackerPoints);
        }
    }
}