using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using static ChickenScratch.ColourManager;

namespace ChickenScratch
{
    public class PlayerFlowManager : MonoBehaviour
    {
        public InstructionsRound instructionRound;
        public DrawingRound drawingRound;
        public SlidesRound slidesRound;
        public StoreRound storeRound;
        public ReviewRound reviewRound;
        public AccusationRound accusationRound;
        public AccoladesRound accoladesRound;
        public ResultsRound resultsRound;
       
        public Text timeRemainingText;
        public GameObject failureNoticeBGObject, pointsTotalBGObject;
        public Text failureNoticeText, pointsTotalText, pointsModifierText;
        public float currentTimeInRound;
        public GameFlowManager.GamePhase currentPhaseName;
        public bool active = true;
        public Transform drawingsContainer;
        public Dictionary<BirdName, string> playerNameMap = new Dictionary<BirdName, string>();

        public bool serverIsReady = false, analyticsAvailable = false;
        public ScreenShotter screenshotter;
        public uGIF.ConvertToGif gifConverter;
        public static BirdName employeeOfTheMonth = BirdName.none;

        public List<Hourglass> cabinetHourglasses = new List<Hourglass>();

        private Dictionary<BirdName, BirdArm> birdArmMap = new Dictionary<BirdName, BirdArm>();
        private bool hasWarnedOnTime = false, hasUpdatedTimeColour = false;
        public bool hasRunOutOfTime = false;
        public Dictionary<string, DrawingVisualsPackage> drawingVisualPackageQueue = new Dictionary<string, DrawingVisualsPackage>();
        public GameObject loadingCircleObject;

        public GameObject waitingForPlayersNotification;
        public int currentDay = 0;

        [SerializeField]
        private PauseMenu pauseMenu;
        private bool isInitialized = false;
        private Dictionary<StoreItem.StoreItemType, StoreItemData> storeItemDataMap = new Dictionary<StoreItem.StoreItemType, StoreItemData>();

        private Dictionary<BirdName, List<GameObject>> lineObjectMap = new Dictionary<BirdName,List<GameObject>>();

        [HideInInspector]
        public List<string> unlockedCaseChoiceIdentifiers = new List<string>();
        [HideInInspector]
        public List<string> caseChoiceUnlockPool = new List<string>();

        [SerializeField]
        private Color baseTimerColour, warningTimerColour, expiringTimerColour;

        public float dailyTimeIncrease = 0f;
        public float baseTimeIncrease = 0f;
        public int baseCasesIncrease = 0;
        public int baseQuotaDecrement = 0;
        public int numberOfDrawingTools = 2;
        public int casesRemaining;

        public List<BirdImage> activeBirdImages = new List<BirdImage>();
        private Dictionary<ColourManager.BirdName, BirdHatData.HatType> birdHatMap = new Dictionary<BirdName, BirdHatData.HatType>();

        private void Start()
        {
            if (NetworkClient.isConnected)
            {
                NetworkClient.Ready();
            }
            //Screen.SetResolution(1280, 720, false);
            Screen.fullScreen = false;

            if (!isInitialized)
            {
                initialize();
            }
            //Test();
        }

        private void Test()
        {
            MarkerStoreItemData markerData = new MarkerStoreItemData() { markerColour = new Color(1f, 1f, 0f, 0.4f), itemType = StoreItem.StoreItemType.highlighter };
            ChargedStoreItemData stopwatchData = new ChargedStoreItemData() { itemType = StoreItem.StoreItemType.stopwatch, numberOfUses = 1 };
            ValueStoreItemData tabData = new ValueStoreItemData() { itemType = StoreItem.StoreItemType.case_tab, value = 0.5f };
            storeItemDataMap.Add(StoreItem.StoreItemType.highlighter, markerData);
            storeItemDataMap.Add(StoreItem.StoreItemType.stopwatch, stopwatchData);
            storeItemDataMap.Add(StoreItem.StoreItemType.case_tab, tabData);
        }

        private void initialize()
        {
            if (!serverIsReady)
            {
                return;
            }
            System.Random rng = new System.Random();
            string allCharacters = "abcdefghijklmnopqrstuvwxyz";
            GameManager.Instance.gameID = "";
            int randomIndex = 0;
            for (int i = 0; i < 5; i++)
            {
                randomIndex = rng.Next(allCharacters.Length);
                GameManager.Instance.gameID += (allCharacters[randomIndex]);
            }

            active = true;

            int iterator = 0;
            List<BirdName> allPlayerBirds = SettingsManager.Instance.GetAllActiveBirds();
            foreach (BirdName bird in allPlayerBirds)
            {
                if (drawingRound.cabinetDrawerMap[iterator + 1].currentPlayer == BirdName.none)
                {
                    cabinetHourglasses[iterator + 1].gameObject.SetActive(true);
                }

                iterator++;
            }
            GameManager.Instance.pauseModTools.Initialize();

            BirdData playerBird = GameDataManager.Instance.GetBird(SettingsManager.Instance.birdName);
            if(playerBird == null)
            {
                Debug.LogError("Could not update cursor for player because player bird["+SettingsManager.Instance.birdName.ToString()+"] is not mapped in the Colour Manager.");
            }
            else
            {
                Texture2D selectedCursor = playerBird.cursor;
                Cursor.SetCursor(selectedCursor, new Vector2(10, 80), CursorMode.Auto);
            }

            //Clear any old data from previous games
            GameDataManager.Instance.Reset();

            drawingVisualPackageQueue = new Dictionary<string, DrawingVisualsPackage>();

            unlockedCaseChoiceIdentifiers = new List<string>(SettingsManager.Instance.gameMode.baseUnlockedChoiceIdentifiers);
            caseChoiceUnlockPool = new List<string>(SettingsManager.Instance.gameMode.baseChoiceIdentifierPool);

            isInitialized = true;
        }


        private void Update()
        {
            if (currentPhaseName == GameFlowManager.GamePhase.instructions ||
                currentPhaseName == GameFlowManager.GamePhase.drawing)
            {
                updateCursorVisibility();
            }

            if (!isInitialized)
            {
                initialize();
            }
            if (!active || GameManager.Instance.currentGameScene == GameManager.GameScene.theater)
            {
                return;
            }
            currentTimeInRound -= Time.deltaTime;
            var ts = TimeSpan.FromSeconds(currentTimeInRound);

            if(currentTimeInRound < 60 & !hasUpdatedTimeColour && currentPhaseName == GameFlowManager.GamePhase.drawing)
            {
                timeRemainingText.color = warningTimerColour;
                hasUpdatedTimeColour = true;
            }
            else if (currentTimeInRound < 10.0f &&
                !hasWarnedOnTime &&
                (currentPhaseName != GameFlowManager.GamePhase.game_tutorial
                && currentPhaseName != GameFlowManager.GamePhase.loading
                && currentPhaseName != GameFlowManager.GamePhase.instructions
                && currentPhaseName != GameFlowManager.GamePhase.accolades
                && currentPhaseName != GameFlowManager.GamePhase.slides))
            {
                hasWarnedOnTime = true;
                if (currentTimeInRound > 0.1f)
                {
                    if(currentPhaseName == GameFlowManager.GamePhase.drawing)
                    {
                        timeRemainingText.color = expiringTimerColour;
                    }
                    AudioManager.Instance.PlaySound("TimeTick");
                }

            }
            else if (currentTimeInRound < 0.0f &&
                (currentPhaseName != GameFlowManager.GamePhase.loading
                && currentPhaseName != GameFlowManager.GamePhase.instructions
                && currentPhaseName != GameFlowManager.GamePhase.game_tutorial))
            {
                if (!hasRunOutOfTime)
                {
                    waitingForPlayersNotification.SetActive(false);
                    loadingCircleObject.SetActive(true);
                    hasRunOutOfTime = true;
                    timeRemainingText.text = "00:00";
                }

                return;
            }

            timeRemainingText.text = string.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds);
        }
        public void OnOutOfTime()
        {
            if (currentPhaseName == GameFlowManager.GamePhase.drawing)
            {
                //Send notification to players that aren't done yet to force them to submit what they have so far
                List<BirdName> playersToForceSubmit = new List<BirdName>();
                foreach (KeyValuePair<int, ChainData> selectedCase in drawingRound.caseMap)
                {
                    if (!selectedCase.Value.IsComplete())
                    {
                        //Queue the player who has the case to force submit
                        int currentRound = selectedCase.Value.currentRound;
                        if (currentRound == -1)
                        {
                            Debug.LogError("ERROR[OnOutOfTime]: Could not isolate currentRound for case["+selectedCase.Key.ToString()+"]");
                        }

                        if(selectedCase.Value.playerOrder.Count <= currentRound)
                        {
                            Debug.LogError("ERROR[OnOutOfTime]: currentRound is greater than the playerOrder for case["+selectedCase.Key.ToString()+"]");
                        }
                        BirdName playerToSubmit = selectedCase.Value.playerOrder[currentRound];
                        if(!playersToForceSubmit.Contains(playerToSubmit))
                        {
                            playersToForceSubmit.Add(playerToSubmit);
                        }
                    }
                }
                //Add transition conditions for each of these players

                foreach(BirdName playerToSubmit in playersToForceSubmit)
                {
                    GameManager.Instance.gameFlowManager.addTransitionCondition("force_submit:" + playerToSubmit.ToString());
                    GameManager.Instance.gameDataHandler.TargetForceSubmit(SettingsManager.Instance.GetConnection(playerToSubmit));
                }
                drawingRound.hasRequestedCaseDetails = true;
            }
        }

        private void updateCursorVisibility()
        {
            bool hideCursor = false;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction, 100.0f);
            foreach (RaycastHit2D hit in hits)
            {
                HideCursorOnHover hoverCursorHider = hit.collider.GetComponent<HideCursorOnHover>();
                if (!pauseMenu.isOpen && hoverCursorHider && hoverCursorHider.gameObject.activeSelf && hoverCursorHider.activated)
                {
                    if (hoverCursorHider.hiderName == "CabinetArea")
                    {
                        StatTracker.Instance.timeInBirdArea += Time.deltaTime;
                    }

                    hideCursor = true;
                }
            }
            Cursor.visible = !hideCursor;
        }

        public void UpdatePhase(GameFlowManager.GamePhase inPhaseName)
        {
            currentPhaseName = inPhaseName;
            hasWarnedOnTime = false;
            timeRemainingText.color = baseTimerColour;
            hasUpdatedTimeColour = false;
            hasRunOutOfTime = false;

            switch (inPhaseName)
            {
                case GameFlowManager.GamePhase.instructions:
                    instructionRound.StartRound();
                    break;
                case GameFlowManager.GamePhase.game_tutorial:
                    GameManager.Instance.gameFlowManager.deadlineTutorialSequence.startSlides();
                    break;
                case GameFlowManager.GamePhase.drawing:
                    drawingRound.StartRound();
                    break;
                case GameFlowManager.GamePhase.slides_tutorial:
                    GameManager.Instance.gameFlowManager.slideTutorialSequence.startSlides();
                    break;
                case GameFlowManager.GamePhase.accolades:
                    accoladesRound.StartRound();
                    break;
                case GameFlowManager.GamePhase.store:
                    storeRound.StartRound();
                    break;
                case GameFlowManager.GamePhase.slides:
                    Cursor.visible = true;
                    slidesRound.StartRound();
                    break;
                case GameFlowManager.GamePhase.results:
                    Cursor.visible = true;
                    resultsRound.StartRound();

                    break;
                default:
                    Debug.LogError("Could not update game phase to new phase["+inPhaseName.ToString()+"] because it has not been set up in the player flow manager.");
                    return;
            }
        }

        public void setChainPrompt(int caseID, int round, BirdName author, string prompt, float timeTaken)
        {
            ChainData chain;
            if (!drawingRound.caseMap.ContainsKey(caseID))
            {
                Debug.LogError("Could not set chain prompt, case[" + caseID.ToString() + "] does not exist.");
            }
            chain = drawingRound.caseMap[caseID];

            if (!chain.prompts.ContainsKey(round))
            {
                chain.prompts.Add(round, new PlayerTextInputData());
            }

            chain.prompts[round].text = prompt;

            chain.prompts[round].author = author;
            chain.prompts[round].timeTaken = timeTaken;
        }

        public void addToRating(int caseID, int tab, BirdName sender, BirdName receiver)
        {
            if(!slidesRound.caseDataMap.ContainsKey(caseID))
            {
                Debug.LogError("ERROR[addToRating]: Could not isolate matching case["+caseID.ToString()+"]");
                return;
            }
            
            EndgameCaseData selectedCase = slidesRound.caseDataMap[caseID];

            if(!selectedCase.taskDataMap.ContainsKey(tab))
            {
                Debug.LogError("ERROR[addToRating]: Could not isolate matching task["+tab.ToString()+"] for case["+caseID.ToString()+"]");
                return;
            }
            selectedCase.taskDataMap[tab].ratingData.likeCount++;
            selectedCase.taskDataMap[tab].ratingData.target = receiver;

            GameManager.Instance.gameDataHandler.RpcShowSlideRatingVisual(sender, receiver);
        }

        public void createDrawingVisuals(DrawingData drawingData, Transform drawingParent, Vector3 position, Vector3 scale, float lineThicknessReductionFactor)
        {
            GameObject drawingContainer = new GameObject();
            drawingContainer.transform.parent = drawingParent;
            drawingContainer.transform.position = drawingParent.position;
            Material drawingMaterial;
            GameObject drawingPrefab = ColourManager.Instance.linePrefab;
            if(!lineObjectMap.ContainsKey(drawingData.author))
            {
                lineObjectMap.Add(drawingData.author, new List<GameObject>());
            }

            foreach (DrawingVisualData visual in drawingData.visuals)
            {
                if (visual is DrawingLineData)
                {
                    DrawingLineData line = (DrawingLineData)visual;
                    drawingMaterial = ColourManager.Instance.baseLineMaterial;
                    
                    GameObject newLine = Instantiate(drawingPrefab, position, Quaternion.identity, drawingContainer.transform);
                    LineRenderer newLineRenderer = newLine.GetComponent<LineRenderer>();

                    lineObjectMap[drawingData.author].Add(newLine);
                    newLineRenderer.material = drawingMaterial;
                    newLineRenderer.material.color = line.lineColour;
                    newLineRenderer.positionCount = line.positions.Count;

                    newLineRenderer.SetPositions(line.GetTransformedPositions(position, scale, line.positions.Count).ToArray());
                    newLine.transform.position = new Vector3(newLine.transform.position.x, newLine.transform.position.y, line.zDepth);
                    newLineRenderer.startWidth = line.lineSize * lineThicknessReductionFactor;
                    newLineRenderer.endWidth = line.lineSize * lineThicknessReductionFactor;
                    newLine.SetActive(!SettingsManager.Instance.IsPlayerCovered(drawingData.author));
                }
            }

            drawingContainer.transform.localScale = scale;
        }

        public void createFlippedDrawingVisuals(DrawingData drawingData, Transform drawingParent, Vector3 position, Vector3 scale, float lineThicknessReductionFactor)
        {
            if(drawingData.visuals.Count == 0)
            {
                return;
            }
            GameObject drawingContainer = new GameObject();
            drawingContainer.transform.parent = drawingParent;
            drawingContainer.transform.position = drawingParent.position;
            Material drawingMaterial;
            GameObject drawingPrefab = ColourManager.Instance.linePrefab;
            if (!lineObjectMap.ContainsKey(drawingData.author))
            {
                lineObjectMap.Add(drawingData.author, new List<GameObject>());
            }

            float highestY = 0f;
            float lowestY = Mathf.Infinity;
            foreach (DrawingVisualData visual in drawingData.visuals)
            {
                if (visual is DrawingLineData)
                {
                    DrawingLineData line = (DrawingLineData)visual;
                    foreach (Vector3 linePosition in line.positions)
                    {
                        if(highestY < linePosition.y)
                        {
                            highestY = linePosition.y;
                        }
                        if(lowestY > linePosition.y)
                        {
                            lowestY = linePosition.y;
                        }
                    }
                }
            }

            float flipOffset = Screen.height / 720 * 2.5f;

            foreach (DrawingVisualData visual in drawingData.visuals)
            {
                if (visual is DrawingLineData)
                {
                    DrawingLineData line = (DrawingLineData)visual;
                    drawingMaterial = ColourManager.Instance.baseLineMaterial;

                    GameObject newLine = Instantiate(drawingPrefab, position, Quaternion.identity, drawingContainer.transform);
                    LineRenderer newLineRenderer = newLine.GetComponent<LineRenderer>();

                    lineObjectMap[drawingData.author].Add(newLine);
                    newLineRenderer.material = drawingMaterial;
                    newLineRenderer.material.color = line.lineColour;
                    newLineRenderer.positionCount = line.positions.Count;

                    

                    newLineRenderer.SetPositions(line.GetFlippedPositions(position + Vector3.up * flipOffset, scale, line.positions.Count).ToArray());
                    newLine.transform.position = new Vector3(newLine.transform.position.x, newLine.transform.position.y, line.zDepth);
                    newLineRenderer.startWidth = line.lineSize * lineThicknessReductionFactor;
                    newLineRenderer.endWidth = line.lineSize * lineThicknessReductionFactor;
                    newLine.SetActive(!SettingsManager.Instance.IsPlayerCovered(drawingData.author));
                }
            }

            drawingContainer.transform.localScale = scale;
        }

        public void AnimateDrawingVisuals(DrawingData drawingData, Transform drawingParent, Vector3 position, Vector3 scale, float lineThicknessReductionFactor, float duration)
        {
            GameObject drawingContainer = new GameObject();
            drawingContainer.transform.parent = drawingParent;
            drawingContainer.transform.position = drawingParent.position;
            Material drawingMaterial;
            GameObject drawingPrefab = ColourManager.Instance.linePrefab;
            drawingContainer.transform.localScale = scale;
            if (!lineObjectMap.ContainsKey(drawingData.author))
            {
                lineObjectMap.Add(drawingData.author, new List<GameObject>());
            }

            foreach (DrawingVisualData visual in drawingData.visuals)
            {
                if (visual is DrawingLineData)
                {
                    DrawingLineData line = (DrawingLineData)visual;
                    drawingMaterial = ColourManager.Instance.baseLineMaterial;
                    
                    GameObject newLine = Instantiate(drawingPrefab, position, Quaternion.identity, drawingContainer.transform);
                    LineRenderer newLineRenderer = newLine.GetComponent<LineRenderer>();
                    lineObjectMap[drawingData.author].Add(newLine);
                    newLineRenderer.material = drawingMaterial;
                    newLineRenderer.material.color = line.lineColour;
                    newLine.transform.position = new Vector3(newLine.transform.position.x, newLine.transform.position.y, line.zDepth);
                    newLineRenderer.startWidth = line.lineSize * lineThicknessReductionFactor;
                    newLineRenderer.endWidth = line.lineSize * lineThicknessReductionFactor;
                    newLine.SetActive(!SettingsManager.Instance.IsPlayerCovered(drawingData.author));
                    StartCoroutine(AnimateDrawingVisual(newLineRenderer, line, position, scale));
                }
            }       
        }

        private IEnumerator AnimateDrawingVisual(LineRenderer newLineRenderer, DrawingLineData line, Vector3 position, Vector3 scale)
        {
            float completionTime = 4f;
            float timePassed = 0f;
            int numberOfTimesItCrossedThreshold = 0;
            for (int i = 0; i < line.positions.Count; i++)
            {
                float timeRatio = timePassed / completionTime;
                float lineRatio = i / (float)line.positions.Count;

                if(timeRatio < lineRatio)
                {
                    numberOfTimesItCrossedThreshold++;
                    yield return new WaitForEndOfFrame();
                    timePassed += Time.deltaTime * slidesRound.slideSpeed;
                }
                newLineRenderer.positionCount = i+1;
                newLineRenderer.SetPositions(line.GetTransformedPositions(position, scale, i+1).ToArray());
            }

        }

        public void addGuessPrompt(GuessData inGuessData, int caseID, float timeTaken)
        {
            if(!drawingRound.caseMap.ContainsKey(caseID))
            {
                Debug.LogError("ERROR[addGuessPrompt]: Could not find matching case["+caseID.ToString()+"] in caseMap.");
                return;
            }
            ChainData chain = drawingRound.caseMap[caseID];
            chain.guessData = inGuessData;

            if (SettingsManager.Instance.isHost)
            {
                //If guess is received then update the total completed chains value
                GameManager.Instance.gameFlowManager.IncreaseNumberOfCompletedCases();
            }
        }

        public void TakeSlideScreenshot(int caseIndex, int slideIndex)
        {
            string folder = "Screenshots\\temp\\" + GameManager.Instance.gameID + "\\Merging\\" + caseIndex.ToString();
            string fileName = caseIndex.ToString() + "-" + slideIndex.ToString() + ".png";
            screenshotter.TakeScreenshot(folder, fileName);

        }

        public float GetTimeInDay()
        {
            float baseTimeInDay = SettingsManager.Instance.gameMode.baseGameTime;
            int currentDay = GameManager.Instance.playerFlowManager.currentDay;
            float baseDailyRamp = SettingsManager.Instance.gameMode.dailyGameTimeRamp * currentDay;
            baseTimeIncrease += dailyTimeIncrease;
            return baseTimeInDay + baseDailyRamp + baseTimeIncrease;
        }

        public int GetCasesForDay()
        {
            return (int)(SettingsManager.Instance.GetCaseCountForDay()) + baseCasesIncrease;
        }

        public int GetCurrentGoal()
        {
            return SettingsManager.Instance.GetCurrentGoal() - baseQuotaDecrement;
        }

        public string MergeSlideImages(int caseIndex)
        {
            string mergingFolder = "Screenshots\\temp\\" + GameManager.Instance.gameID + "\\Merging\\" + caseIndex.ToString();
            string savingFolder = "Screenshots\\";
            //string promptName = drawingRound.caseMap[caseIndex].correctWordsMap[1].value + "_" + drawingRound.caseMap[caseIndex].correctWordsMap[2].value;
            string finalPath = "";
            //Pull in the images as Texture2D data then feed them in to the GifConverter
            // List<Texture2D> loadedTextures = new List<Texture2D>();
            // string[] filesInFolder = Directory.GetFiles(Application.persistentDataPath + "\\" + mergingFolder);
            // foreach(string fileInFolder in filesInFolder)
            // {
            //     byte[] byteArray = File.ReadAllBytes(fileInFolder);
            //     Texture2D sampleTexture = new Texture2D(2,2);
            //     bool isLoaded = sampleTexture.LoadImage(byteArray);
            //     if(isLoaded)
            //     {
            //         loadedTextures.Add(sampleTexture);
            //     }
            // }

            //gifConverter.SaveGIF(loadedTextures, savingFolder + "\\" + promptName + ".gif");
            try
            {
                //finalPath = screenshotter.MergeImages(mergingFolder, promptName + ".png", savingFolder);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to merge images - " + e.Message);
                return "Failed to save.";
            }
            return finalPath;

        }

        public void saveGifOfChain(int caseIndex)
        {
            string mergingFolder = "Screenshots\\temp\\" + GameManager.Instance.gameID + "\\Merging";
            string savingFolder = Application.persistentDataPath + "\\Screenshots\\";
            //string promptName = drawingRound.caseMap[caseIndex].correctWordsMap[1] + "_" + drawingRound.caseMap[caseIndex].correctWordsMap[2];

            //Pull in the images as Texture2D data then feed them in to the GifConverter
            List<Texture2D> loadedTextures = new List<Texture2D>();
            string[] filesInFolder = Directory.GetFiles(Application.persistentDataPath + "\\" + mergingFolder);

            int totalPixelWidth = 0;
            int totalPixelHeight = 0;
            foreach (string fileInFolder in filesInFolder)
            {
                byte[] byteArray = File.ReadAllBytes(fileInFolder);
                Texture2D sampleTexture = new Texture2D(2, 2);
                bool isLoaded = sampleTexture.LoadImage(byteArray);
                if (isLoaded)
                {
                    loadedTextures.Add(sampleTexture);
                    totalPixelWidth += sampleTexture.width;
                    if (totalPixelHeight < sampleTexture.height)
                    {
                        totalPixelHeight = sampleTexture.height;
                    }
                }
            }

            //gifConverter.SaveGIF(loadedTextures, savingFolder + "\\" + promptName + ".gif");
        }

        public void AddStoreItem(StoreItemData inStoreItemData)
        {
            if(inStoreItemData.itemType == StoreItem.StoreItemType.case_unlock ||
                inStoreItemData.itemType == StoreItem.StoreItemType.case_upgrade ||
                inStoreItemData.itemType == StoreItem.StoreItemType.case_frequency ||
                inStoreItemData.itemType == StoreItem.StoreItemType.coffee_pot ||
                inStoreItemData.itemType == StoreItem.StoreItemType.advertisement ||
                inStoreItemData.itemType == StoreItem.StoreItemType.nest_feathering ||
                inStoreItemData.itemType == StoreItem.StoreItemType.coffee_mug ||
                inStoreItemData.itemType == StoreItem.StoreItemType.contract)
            {
                //These are handled on the server-side and propagated to everyone in the game
                return;
            }
            if(storeItemDataMap.ContainsKey(inStoreItemData.itemType))
            {
                Debug.LogError("Player already has store item["+inStoreItemData.itemType.ToString()+"].");
                return;
            }
            storeItemDataMap.Add(inStoreItemData.itemType,inStoreItemData);

            if(inStoreItemData.itemType == StoreItem.StoreItemType.score_tracker)
            {
                GameManager.Instance.gameDataHandler.CmdRegisterPlayerForScoreTracker(SettingsManager.Instance.birdName);
            }
        }

        public bool HasStoreItem(StoreItem.StoreItemType storeItemType)
        {
            return storeItemDataMap.ContainsKey(storeItemType);
        }

        public Color GetStoreItemMarkerColour(StoreItem.StoreItemType markerType)
        {
            return ((MarkerStoreItemData)storeItemDataMap[markerType]).markerColour;
        }

        public bool StoreItemHasCharges(StoreItem.StoreItemType itemType)
        {
            return HasStoreItem(itemType) && ((ChargedStoreItemData)(storeItemDataMap[itemType])).numberOfUses > 0;
        }

        public void UseChargedItem(StoreItem.StoreItemType itemType)
        {
            ((ChargedStoreItemData)(storeItemDataMap[itemType])).numberOfUses--;
        }

        public float GetStoreItemValue(StoreItem.StoreItemType itemType)
        {
            return ((ValueStoreItemData)storeItemDataMap[itemType]).value;
        }

        public void HideAuthorDrawingLines(BirdName author)
        {
            if(lineObjectMap.ContainsKey(author))
            {
                //Clear null lines
                ClearNullAuthorLines(author);

                foreach (GameObject lineObject in lineObjectMap[author])
                {
                    if(lineObject)
                    {
                        lineObject.SetActive(false);
                    }
                    
                }
            }
        }

        public void ShowAuthorDrawingLines(BirdName author)
        {
            if (lineObjectMap.ContainsKey(author))
            {
                //Clear null lines
                ClearNullAuthorLines(author);

                foreach (GameObject lineObject in lineObjectMap[author])
                {
                    if(lineObject)
                    {
                        lineObject.SetActive(true);
                    }
                    
                }
            }
        }

        private void ClearNullAuthorLines(BirdName author)
        {
            for(int i = lineObjectMap[author].Count -1; i >= 0; i--)
            {
                if (lineObjectMap[author][i] == null)
                {
                    lineObjectMap[author].RemoveAt(i);
                }
                
            }
        }

        public void AddAuthorDrawingLine(BirdName author, GameObject lineObject)
        {
            if(!lineObjectMap.ContainsKey(author))
            {
                lineObjectMap.Add(author, new List<GameObject>());
            }
            lineObjectMap[author].Add(lineObject);
        }

        public BirdHatData.HatType GetBirdHatType(BirdName bird)
        {
            if(birdHatMap.ContainsKey(bird))
            {
                return birdHatMap[bird];
            }
            return BirdHatData.HatType.none;
        }

        public void SetBirdHatType(BirdName bird, BirdHatData.HatType hat)
        {
            if(!birdHatMap.ContainsKey(bird))
            {
                birdHatMap.Add(bird, hat);
            }
            else
            {
                birdHatMap[bird] = hat;
            }

            foreach(BirdImage birdImage in activeBirdImages)
            {
                if(birdImage != null && birdImage.currentBird == bird)
                {
                    birdImage.UpdateImage(bird, hat);
                }
            }
        }
    }
}