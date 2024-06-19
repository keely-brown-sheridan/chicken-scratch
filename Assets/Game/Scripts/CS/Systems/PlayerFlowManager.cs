using Mirror;
using System;
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
        public GameFlowManager.PlayerRole playerRole;
        public ScreenShotter screenshotter;
        public uGIF.ConvertToGif gifConverter;
        public static BirdName employeeOfTheMonth = BirdName.none;

        public List<Hourglass> cabinetHourglasses = new List<Hourglass>();

        private Dictionary<BirdName, BirdArm> birdArmMap = new Dictionary<BirdName, BirdArm>();
        private bool hasWarnedOnTime = false;
        public bool hasRunOutOfTime = false;
        public Dictionary<string, DrawingVisualsPackage> drawingVisualPackageQueue = new Dictionary<string, DrawingVisualsPackage>();
        public GameObject loadingCircleObject;

        public GameObject waitingForPlayersNotification;

        [SerializeField]
        private PauseMenu pauseMenu;
        private bool isInitialized = false;

        private void Start()
        {
            if (NetworkClient.isConnected)
            {
                NetworkClient.Ready();
            }

            if (!isInitialized)
            {
                initialize();
            }
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

            if (SettingsManager.Instance.gameMode.name != "Theater")
            {
                int iterator = 0;
                List<BirdName> allPlayerBirds = SettingsManager.Instance.GetAllActiveBirds();
                foreach (BirdName playerBird in allPlayerBirds)
                {
                    if (drawingRound.cabinetDrawerMap[iterator + 1].currentPlayer == BirdName.none)
                    {
                        cabinetHourglasses[iterator + 1].gameObject.SetActive(true);
                    }

                    iterator++;
                }
            }


            Texture2D selectedCursor = ColourManager.Instance.birdMap[SettingsManager.Instance.birdName].cursor;
            Cursor.SetCursor(selectedCursor, new Vector2(10, 80), CursorMode.Auto);

            drawingVisualPackageQueue = new Dictionary<string, DrawingVisualsPackage>();
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

            if (currentTimeInRound < 10.0f &&
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
                        int currentRound = drawingRound.currentRound;
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
                    Debug.LogError("Forcing player["+playerToSubmit.ToString()+"] to submit.");
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
            hasRunOutOfTime = false;

            switch (inPhaseName)
            {
                case GameFlowManager.GamePhase.instructions:
                    instructionRound.StartRound();
                    break;
                case GameFlowManager.GamePhase.game_tutorial:
                    GameManager.Instance.gameFlowManager.bossRushGameTutorialSequence.startSlides();
                    break;
                case GameFlowManager.GamePhase.drawing:
                    drawingRound.StartRound();
                    break;
                case GameFlowManager.GamePhase.slides_tutorial:
                    GameManager.Instance.gameFlowManager.bossRushSlidesTutorialSequence.startSlides();
                    break;
                case GameFlowManager.GamePhase.accolades:
                    accoladesRound.StartRound();
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
            EndgameCaseData selectedCase = slidesRound.caseDataMap[caseID];
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
            GameObject drawingPrefab = null;

            foreach (DrawingVisualData visual in drawingData.visuals)
            {
                if (visual is DrawingLineData)
                {
                    DrawingLineData line = (DrawingLineData)visual;
                    if (line.lineColour == DrawingLineData.LineColour.Base)
                    {
                        drawingMaterial = ColourManager.Instance.baseLineMaterial;
                        drawingPrefab = ColourManager.Instance.linePrefab;
                    }
                    else if (line.lineColour == DrawingLineData.LineColour.Colour)
                    {
                        drawingMaterial = ColourManager.Instance.birdMap[line.author].material;
                        drawingPrefab = ColourManager.Instance.linePrefab;
                    }
                    else if (line.lineColour == DrawingLineData.LineColour.Light)
                    {
                        drawingMaterial = ColourManager.Instance.birdMap[line.author].bgLineMaterial;
                        drawingPrefab = ColourManager.Instance.linePrefab;
                    }
                    else if (line.lineColour == DrawingLineData.LineColour.Erase)
                    {
                        drawingMaterial = ColourManager.Instance.eraseLineMaterial;
                        drawingPrefab = ColourManager.Instance.linePrefab;
                    }
                    else if (line.lineColour == DrawingLineData.LineColour.Clear)
                    {
                        drawingMaterial = ColourManager.Instance.clearLineMaterial;
                        drawingPrefab = ColourManager.Instance.linePrefab;
                    }
                    else
                    {
                        Debug.LogError("Invalid line type[" + line.lineColour + "] provided for line. Cannot create drawing.");
                        return;
                    }

                    GameObject newLine = Instantiate(drawingPrefab, position, Quaternion.identity, drawingContainer.transform);
                    LineRenderer newLineRenderer = newLine.GetComponent<LineRenderer>();

                    newLineRenderer.material = drawingMaterial;
                    newLineRenderer.positionCount = line.positions.Count;

                    newLineRenderer.SetPositions(line.GetTransformedPositions(position, scale).ToArray());
                    newLine.transform.position = new Vector3(newLine.transform.position.x, newLine.transform.position.y, line.zDepth);
                    newLineRenderer.startWidth = line.lineSize * lineThicknessReductionFactor;
                    newLineRenderer.endWidth = line.lineSize * lineThicknessReductionFactor;

                }
            }

            drawingContainer.transform.localScale = scale;
        }

        public void addGuessPrompt(GuessData inGuessData, int caseID, float timeTaken)
        {
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
    }
}