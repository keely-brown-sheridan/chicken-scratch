
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ChickenScratch.ColourManager;
using static ChickenScratch.ReactionIndex;
using static GameDataHandler;

namespace ChickenScratch
{
    public class SlidesRound : PlayerRound
    {
        public bool inProgress = false;

        public SlideContents currentSlideContents;

        public List<PeanutBird> allChatBirds = new List<PeanutBird>();

        public Dictionary<int,EndgameCaseData> caseDataMap = new Dictionary<int,EndgameCaseData>();


        private float currentTimeOnSlide;
        public int currentSlideCaseIndex = 0;
        public float slideSpeed = 1.0f;
        public float baseSlideSpeed = 1.0f;
        

        public float timeToShowSlideSpeedIncreaseIndicator = 1.0f;
        public GameObject slideSpeedIncreaseIndicator;
        private float timeShowingSlideSpeedIncreaseIndicator = 0.0f;
        public bool hasReceivedRatings = false;

        public ReactionButtonManager reactionButtonManager;
        public GoldStarDetectionArea currentHoveredGoldStarDetectionArea = null;

        private List<SlideContents> queuedSlides = new List<SlideContents>();

        [SerializeField]
        private List<SlideTypeData> slideTypes;

        [SerializeField]
        private Transform slidesParent;

        private Dictionary<SlideTypeData.SlideType, SlideTypeData> slideTypeMap = new Dictionary<SlideTypeData.SlideType, SlideTypeData>();

        [SerializeField]
        private GameObject playerRatingVisualPrefab;
        [SerializeField]
        private Transform playerRatingVisualParent;
        [SerializeField]
        public Transform playerRatingMaxHeightTransform;
        [SerializeField]
        private GoldStarManager goldStarManager;

        public SlidesProgressTracker slidesProgressTracker;
        public QuickplaySummarySlide quickplaySummarySlide;

        


        private bool hasSentRatings = false;
        private bool hasSpedUp = false;
        private int numPlayersSpedUp = 0;
        public int currentSlideContentIndex = 0;

        public override void StartRound()
        {
            inProgress = true;
            slideTypeMap.Clear();
            foreach(SlideTypeData slideType in slideTypes)
            {
                slideTypeMap.Add(slideType.slideType, slideType);
            }
            base.StartRound();

            int numberOfPlayers = GameManager.Instance.playerFlowManager.playerNameMap.Count;
            //slideSpeed = numPlayersSpedUp == numberOfPlayers ? 4.0f : 4.0f / (4.0f - (numberOfPlayers - numPlayersSpedUp) / numberOfPlayers);

            //GameManager.Instance.playerFlowManager.screenshotter.clearOldTempData();

            if (SettingsManager.Instance.GetSetting("stickies"))
            {
                GameManager.Instance.playerFlowManager.instructionRound.slidesChatSticky.Queue(true);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (timeShowingSlideSpeedIncreaseIndicator > 0.0f)
            {
                timeShowingSlideSpeedIncreaseIndicator += Time.deltaTime;
                if (timeShowingSlideSpeedIncreaseIndicator > timeToShowSlideSpeedIncreaseIndicator)
                {
                    timeShowingSlideSpeedIncreaseIndicator = 0.0f;
                    slideSpeedIncreaseIndicator.SetActive(false);
                }
            }

            if (SettingsManager.Instance.isHost)
            {
                updateSlideFlow();
            }
        }

        public void GenerateEndgameData()
        {
            Dictionary<int, ChainData> gameCaseMap = GameManager.Instance.playerFlowManager.drawingRound.caseMap;
            List<EndgameCaseData> endgameCases = new List<EndgameCaseData>();
            foreach(KeyValuePair<int,ChainData> currentCase in gameCaseMap)
            {
                EndgameCaseData endgameCase = new EndgameCaseData(currentCase.Value);
                endgameCases.Add(endgameCase);
            }
            GameManager.Instance.gameDataHandler.RpcSendEndgameCaseDataWrapper(endgameCases);
        }

        private void updateSlideFlow()
        {
            if (inProgress)
            {
                if(currentSlideContents == null || !currentSlideContents.active)
                {
                    
                    //Get the next active case which has content which can be shown
                    currentSlideCaseIndex++;

                    //If there are no more cases left then it's time to move on, so send the ratings
                    if (!caseDataMap.ContainsKey(currentSlideCaseIndex))
                    {
                        if (!hasSentRatings)
                        {
                            GameManager.Instance.gameFlowManager.timeRemainingInPhase = 0.0f;

                            //Send broadcast to players for ratings
                            GameManager.Instance.gameDataHandler.RpcSendStats();
                            StatTracker.Instance.SetServerPlayerStats();
                            sendRatingsToClients();
                            inProgress = false;
                            return;
                        }
                    }
                    else
                    {
                        //Create the slides for the next case and start showing them
                        GameManager.Instance.gameDataHandler.RpcCreateSlidesFromCaseWrapper(caseDataMap[currentSlideCaseIndex]);
                    }
                }
                //Manage the execution of the current slide
                else
                {
                    //If the current slide is finished then we need to either
                    if(currentSlideContents.isComplete)
                    {
                        //Move to the next slide in the queue
                        if (queuedSlides.Count - 1 > currentSlideContentIndex)
                        {
                            GameManager.Instance.gameDataHandler.RpcShowNextSlide();
                        }
                        else
                        {
                            //Or destroy the current set of slides to move on to the next
                            for(int i = queuedSlides.Count - 1; i >=0; i--)
                            {
                                Destroy(queuedSlides[i].gameObject);
                            }
                            currentSlideContents = null;
                        }
                    }
                }
            }
        }

        public void CreateSlidesFromCase(EndgameCaseData caseData)
        {
            string correctPrefix = GameDataManager.Instance.GetWord(caseData.correctWordIdentifierMap[1]).value;
            string correctNoun = GameDataManager.Instance.GetWord(caseData.correctWordIdentifierMap[2]).value;
            goldStarManager.restock();
            ClearPreviousSlides();
            //Create the intro slide
            SlideTypeData currentSlideType = slideTypeMap[SlideTypeData.SlideType.intro];
            GameObject introSlideObject = Instantiate(currentSlideType.prefab, slidesParent);
            IntroSlideContents introSlide = introSlideObject.GetComponent<IntroSlideContents>();
            introSlide.Initialize(correctPrefix, correctNoun, caseData.identifier, currentSlideType.showDuration);
            queuedSlides.Add(introSlide);

            //Create summary slide
            currentSlideType = slideTypeMap[SlideTypeData.SlideType.summary];
            GameObject summarySlideObject = Instantiate(currentSlideType.prefab, slidesParent);
            SummarySlideContents summarySlide = summarySlideObject.GetComponent<SummarySlideContents>();
            summarySlide.Initialize(correctPrefix, correctNoun, caseData.identifier, currentSlideType.showDuration, caseData.GetTotalPoints());

            //Create task slides
            foreach (KeyValuePair<int,EndgameTaskData> taskData in caseData.taskDataMap)
            {
                switch(taskData.Value.taskType)
                {
                    case TaskData.TaskType.base_drawing:
                    case TaskData.TaskType.add_drawing:
                    case TaskData.TaskType.compile_drawing:
                    case TaskData.TaskType.copy_drawing:
                    case TaskData.TaskType.prompt_drawing:
                        currentSlideType = slideTypeMap[SlideTypeData.SlideType.drawing];
                        GameObject drawingSlideObject = Instantiate(currentSlideType.prefab, slidesParent);
                        DrawingSlideContents drawingSlide = drawingSlideObject.GetComponent<DrawingSlideContents>();
                        drawingSlide.Initialize(taskData.Value.drawingData, correctPrefix, correctNoun, taskData.Key, caseData.identifier, currentSlideType.showDuration, taskData.Value.timeModifierDecrement);
                        queuedSlides.Add(drawingSlide);
                        summarySlide.AddDrawing(taskData.Value.drawingData, taskData.Key, caseData.identifier, summarySlide.transform, taskData.Value.timeModifierDecrement);
                        break;
                    case TaskData.TaskType.prompting:
                        currentSlideType = slideTypeMap[SlideTypeData.SlideType.prompt];
                        GameObject promptingSlideObject = Instantiate(currentSlideType.prefab, slidesParent);
                        PromptSlideContents promptingSlide = promptingSlideObject.GetComponent<PromptSlideContents>();
                        promptingSlide.Initialize(taskData.Value.promptData, correctPrefix, correctNoun, taskData.Key, caseData.identifier, currentSlideType.showDuration, taskData.Value.timeModifierDecrement);
                        queuedSlides.Add(promptingSlide);
                        summarySlide.AddPrompt(taskData.Value.promptData, taskData.Key, caseData.identifier, taskData.Value.timeModifierDecrement);
                        break;
                    case TaskData.TaskType.base_guessing:
                        currentSlideType = slideTypeMap[SlideTypeData.SlideType.guess];
                        GameObject guessingSlideObject = Instantiate(currentSlideType.prefab, slidesParent);
                        GuessSlideContents guessingSlide = guessingSlideObject.GetComponent<GuessSlideContents>();
                        guessingSlide.Initialize(caseData, taskData.Value, taskData.Key, currentSlideType.showDuration);
                        queuedSlides.Add(guessingSlide);
                        summarySlide.AddGuess(caseData.guessData, caseData.correctWordIdentifierMap, taskData.Key, caseData.identifier, taskData.Value.timeModifierDecrement);
                        break;
                }
            }
            
            queuedSlides.Add(summarySlide);
            //Show the first slide
            currentSlideContents = introSlide;
            currentSlideContents.gameObject.SetActive(true);
            currentSlideContents.active = true;
            currentSlideContentIndex = 0;
        }

        private void ClearPreviousSlides()
        {
            for(int i = queuedSlides.Count -1; i >= 0; i--)
            {
                if (queuedSlides[i] != null)
                {
                    Destroy(queuedSlides[i].gameObject);
                }
                
            }
            queuedSlides.Clear();
        }

        public void ShowNextSlide()
        {
            currentSlideContents.gameObject.SetActive(false);
            currentSlideContentIndex++;
            
            currentSlideContents = queuedSlides[currentSlideContentIndex];
            currentSlideContents.Show();
            currentSlideContents.gameObject.SetActive(true);
        }

        public void initializeGalleryBird(int index, ColourManager.BirdName inBirdName)
        {
            PeanutBird peanutBird = allChatBirds[index];
            peanutBird.gameObject.SetActive(true);
            Instantiate(ColourManager.Instance.birdMap[inBirdName].slidesBirdPrefab, peanutBird.transform);
            peanutBird.Colourize(ColourManager.Instance.birdMap[inBirdName].colour);
            peanutBird.birdName = inBirdName;
            peanutBird.chatBubble.bubbleText.color = ColourManager.Instance.birdMap[inBirdName].colour;
            peanutBird.isInitialized = true;

            if (inBirdName == SettingsManager.Instance.birdName)
            {
                reactionButtonManager.Initialize();
            }
        }

        public bool hasAlreadyGivenLikeToRound(int caseID, int round)
        {
            bool hasAlreadyGivenLikeToRound = caseDataMap[caseID].taskDataMap[round].ratingData.target != BirdName.none;
            return hasAlreadyGivenLikeToRound;
        }

        private void sendRatingsToClients()
        {
            GameManager.Instance.gameDataHandler.RpcSlideRoundEndInfoWrapper(caseDataMap);
            hasSentRatings = true;
        }


        public void updateChainRatings(List<EndgameCaseData> inCaseData)
        {
            PlayerRatingData selectedRatingData;
            foreach (EndgameCaseData serverCase in inCaseData)
            {
                if(!caseDataMap.ContainsKey(serverCase.identifier))
                {
                    Debug.LogError("ERROR[updateChainRatings]: Could not find matching case["+ serverCase.identifier+"] on client.");
                }
                EndgameCaseData matchingCase = caseDataMap[serverCase.identifier];
                foreach(KeyValuePair<int,EndgameTaskData> serverTask in serverCase.taskDataMap)
                {
                    if (!matchingCase.taskDataMap.ContainsKey(serverTask.Key))
                    {
                        Debug.LogError("ERROR[updateChainRatings]: Could not access task[" + serverTask.Key.ToString() + "] for case because it wasn't in the queue.");
                    }
                    selectedRatingData = matchingCase.taskDataMap[serverTask.Key].ratingData;
                    selectedRatingData.likeCount = serverTask.Value.ratingData.likeCount;
                    selectedRatingData.target = serverTask.Value.ratingData.target;
                }
                
            }
        }

        

        public void SpeedUpSlidesClick()
        {
            if (!hasSpedUp)
            {
                if (SettingsManager.Instance.isHost)
                {
                    increaseSlideSpeed();
                }
                else
                {
                    GameManager.Instance.gameDataHandler.CmdSpeedUpSlides();
                }
                hasSpedUp = true;
            }
        }

        public void increaseSlideSpeed()
        {
            numPlayersSpedUp++;
            int numberOfPlayers = GameManager.Instance.playerFlowManager.playerNameMap.Count;
            float newSlideSpeed = numPlayersSpedUp == numberOfPlayers ? 4.0f : 4.0f - (4.0f * (numberOfPlayers - numPlayersSpedUp) / numberOfPlayers);

            setSlideSpeed(newSlideSpeed);
            GameManager.Instance.gameDataHandler.RpcSetSlideSpeed(slideSpeed);
        }

        public void showReaction(BirdName birdName, Reaction reaction)
        {
            allChatBirds.Where(pb => pb.isInitialized).Single(pb => pb.birdName == birdName).ShowReaction(reaction);
        }

        public void setSlideSpeed(float inSlideSpeed)
        {
            AudioManager.Instance.PlaySound("sfx_vote_int_skip_forward");
            slideSpeed = inSlideSpeed;

            slideSpeedIncreaseIndicator.SetActive(true);
            timeShowingSlideSpeedIncreaseIndicator = Time.deltaTime;
        }

        public void addLike(int caseIndex, int tab, BirdName birdName)
        {
            StatTracker.Instance.LikePlayer(birdName);
            EndgameCaseData selectedCase = caseDataMap[caseIndex];
            selectedCase.IncreaseRating(tab, birdName);
        }

        public void showPlayerRatingVisual(BirdName sender, BirdName receiver)
        {
            //Spawn liked object and initialize it
            PeanutBird targetBird = allChatBirds.Single(pb => pb.birdName == receiver);
            Vector3 startingPosition = allChatBirds.Single(pb => pb.birdName == sender).transform.position;
            Vector3 endingPosition = targetBird.transform.position;
            GameObject playerRatingVisualObject = Instantiate(playerRatingVisualPrefab, playerRatingVisualParent);
            PlayerRatingVisual playerRatingVisual = playerRatingVisualObject.GetComponent<PlayerRatingVisual>();
            playerRatingVisual.initialize(startingPosition, endingPosition, playerRatingMaxHeightTransform.position.y, targetBird);

        }

        public void InitializeSlidesProgressTracker()
        {
            List<WorkingGoalsManager.Goal> goals = new List<WorkingGoalsManager.Goal>();

            foreach (ResultData result in SettingsManager.Instance.resultPossibilities)
            {
                int requiredPoints = (int)(result.getRequiredPointThreshold(SettingsManager.Instance.gameMode.name));
                goals.Add(new WorkingGoalsManager.Goal(result.goal, requiredPoints, result.resultName));
            }
            
            slidesProgressTracker.SetGameGoals(goals);
        }

        
        public void ShowFastResults()
        {
            if (currentSlideContents)
            {
                currentSlideContents.gameObject.SetActive(false);
            }

            quickplaySummarySlide.Initialize();
            quickplaySummarySlide.Activate();
        }
    }
}