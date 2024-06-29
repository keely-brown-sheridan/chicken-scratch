
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static ChickenScratch.ColourManager;
using static ChickenScratch.GameFlowManager;
using static ChickenScratch.ReactionIndex;
using static GameDataHandler;

namespace ChickenScratch
{
    public class SlidesRound : PlayerRound
    {
        public bool inProgress = false;

        public SlideContents currentSlideContents;

        public List<PeanutBird> allChatBirds = new List<PeanutBird>();

        public Dictionary<int, EndgameCaseData> caseDataMap = new Dictionary<int, EndgameCaseData>();


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

        [SerializeField]
        private EndlessModeResultForm dayResultForm;
        [SerializeField]
        private FinalEndgameResultManager finalResultManager;

        [SerializeField]
        private GameObject caseDetailsHolder;

        [SerializeField]
        private Image authorImage;

        [SerializeField]
        private TMPro.TMP_Text authorNameText;

        [SerializeField]
        private TMPro.TMP_Text casesProgressReminderText;

        [SerializeField]
        private CaseTypeSlideVisualizer caseTypeSlideVisualizer;

        [SerializeField]
        private TMPro.TMP_Text originalPromptText;

        [SerializeField]
        private GameObject birdHolder;

        public SlidesProgressTracker slidesProgressTracker;
        public QuickplaySummarySlide quickplaySummarySlide;

        public int currentBirdBuckTotal = 0;

        public GamePhase phaseToTransitionTo => _phaseToTransitionTo;

        private GamePhase _phaseToTransitionTo;

        private bool hasSentRatings = false;
        private bool hasStartedShowingDayResult = false;
        private bool hasSpedUp = false;
        private int numPlayersSpedUp = 0;
        private Dictionary<BirdName, PeanutBird> chatBirdMap = new Dictionary<BirdName, PeanutBird>();
        public int currentSlideContentIndex = 0;

        private int totalCasesInSlidesRound = 0;
        private int startingCaseIndex = 0;
        public override void StartRound()
        {
            startingCaseIndex = currentSlideContentIndex;
            totalCasesInSlidesRound = caseDataMap.Count - currentSlideContentIndex;
            currentBirdBuckTotal = 0;
            hasStartedShowingDayResult = false;
            inProgress = true;

            if (SettingsManager.Instance.isHost)
            {
                GameManager.Instance.gameFlowManager.addTransitionCondition("slides_complete");
            }
            slideTypeMap.Clear();
            foreach (SlideTypeData slideType in slideTypes)
            {
                slideTypeMap.Add(slideType.slideType, slideType);
            }
            chatBirdMap.Clear();
            foreach(PeanutBird chatBird in allChatBirds)
            {
                if(!chatBird.isInitialized || chatBirdMap.ContainsKey(chatBird.birdName))
                {
                    continue;
                }
                chatBirdMap.Add(chatBird.birdName, chatBird);
            }
            base.StartRound();

            int numberOfPlayers = GameManager.Instance.playerFlowManager.playerNameMap.Count;

            if (SettingsManager.Instance.GetSetting("stickies") &&
                !GameManager.Instance.playerFlowManager.instructionRound.slidesChatSticky.hasBeenPlaced)
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

        public PeanutBird GetChatBird(BirdName birdName)
        {
            if(chatBirdMap.ContainsKey(birdName))
            {
                return chatBirdMap[birdName];
            }
            return null;
        }

        public void GenerateEndgameData()
        {
            Dictionary<int, ChainData> gameCaseMap = GameManager.Instance.playerFlowManager.drawingRound.caseMap;
            List<EndgameCaseData> endgameCases = new List<EndgameCaseData>();
            foreach (KeyValuePair<int, ChainData> currentCase in gameCaseMap)
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
                if (currentSlideContents == null || !currentSlideContents.active)
                {
                    if (hasStartedShowingDayResult)
                    {
                        return;
                    }
                    //Get the next active case which has content which can be shown
                    currentSlideCaseIndex++;

                    //If there are no more cases left then it's time to move on, so send the ratings
                    if (!caseDataMap.ContainsKey(currentSlideCaseIndex))
                    {
                        GameManager.Instance.gameDataHandler.RpcShowDayResult();
                        hasStartedShowingDayResult = true;
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
                    if (currentSlideContents.isComplete)
                    {
                        //Move to the next slide in the queue
                        if (queuedSlides.Count - 1 > currentSlideContentIndex)
                        {
                            GameManager.Instance.gameDataHandler.RpcShowNextSlide();
                        }
                        else
                        {
                            //Or destroy the current set of slides to move on to the next
                            currentSlideContents = null;
                        }
                    }
                }
            }
        }

        public void CreateSlidesFromCase(EndgameCaseData caseData)
        {
            int caseIndex = caseData.identifier - startingCaseIndex;
            string caseReminderText = "Case " + caseIndex.ToString() + " of " + totalCasesInSlidesRound.ToString();
            casesProgressReminderText.text = caseReminderText;

            string correctPrefix = GameDataManager.Instance.GetWord(caseData.correctWordIdentifierMap[1]).value;
            string correctNoun = GameDataManager.Instance.GetWord(caseData.correctWordIdentifierMap[2]).value;
            goldStarManager.Restock();
            ClearPreviousSlides();

            //Check to make sure the slide type map contains all of the required slides
            if(!slideTypeMap.ContainsKey(SlideTypeData.SlideType.intro) ||
                !slideTypeMap.ContainsKey(SlideTypeData.SlideType.drawing) ||
                !slideTypeMap.ContainsKey(SlideTypeData.SlideType.prompt) ||
                !slideTypeMap.ContainsKey(SlideTypeData.SlideType.guess))
            {
                Debug.LogError("ERROR[CreateSlidesFromCase]: Missing slide types from the slide type map.");
                return;
            }

            //Create the intro slide
            SlideTypeData currentSlideType = slideTypeMap[SlideTypeData.SlideType.intro];
            GameObject introSlideObject = Instantiate(currentSlideType.prefab, slidesParent);
            IntroSlideContents introSlide = introSlideObject.GetComponent<IntroSlideContents>();
            introSlide.Initialize(correctPrefix, correctNoun, caseData.identifier, currentSlideType.showDuration, caseReminderText);
            queuedSlides.Add(introSlide);

            //Create summary slide
            currentSlideType = slideTypeMap[SlideTypeData.SlideType.summary];
            GameObject summarySlideObject = Instantiate(currentSlideType.prefab, slidesParent);
            SummarySlideContents summarySlide = summarySlideObject.GetComponent<SummarySlideContents>();
            summarySlide.Initialize(correctPrefix, correctNoun, caseData.identifier, currentSlideType.showDuration, caseData.scoringData.GetTotalPoints());

            //Create task slides
            foreach (KeyValuePair<int,EndgameTaskData> taskData in caseData.taskDataMap)
            {
                if (taskData.Value.assignedPlayer == BirdName.none) continue;
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
                        drawingSlide.Initialize(taskData.Value.drawingData, taskData.Key, caseData.identifier, currentSlideType.showDuration, taskData.Value.timeModifierDecrement);
                        queuedSlides.Add(drawingSlide);
                        summarySlide.AddDrawing(taskData.Value.drawingData, taskData.Key, caseData.identifier, summarySlide.transform, taskData.Value.timeModifierDecrement);
                        break;
                    case TaskData.TaskType.prompting:
                        currentSlideType = slideTypeMap[SlideTypeData.SlideType.prompt];
                        GameObject promptingSlideObject = Instantiate(currentSlideType.prefab, slidesParent);
                        PromptSlideContents promptingSlide = promptingSlideObject.GetComponent<PromptSlideContents>();
                        promptingSlide.Initialize(taskData.Value.promptData, taskData.Key, caseData.identifier, currentSlideType.showDuration, taskData.Value.timeModifierDecrement);
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

            caseTypeSlideVisualizer.Initialize(caseData.caseTypeColour, caseData.caseTypeName);
            originalPromptText.text = SettingsManager.Instance.CreatePromptText(correctPrefix, correctNoun);

            summarySlide.LoadSections();
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
            if(currentSlideContents != null && currentSlideContents.gameObject != null)
            {
                currentSlideContents.gameObject.SetActive(false);
            }
            
            currentSlideContentIndex++;
            
            if(queuedSlides.Count <= currentSlideContentIndex)
            {
                Debug.LogError("Could not display slide["+currentSlideContentIndex.ToString()+"] because there aren't enough queued slides["+queuedSlides.Count.ToString()+"].");
                return;
            }
            currentSlideContents = queuedSlides[currentSlideContentIndex];
            currentSlideContents.Show();
            currentSlideContents.gameObject.SetActive(true);
        }

        public void ShowCaseDetails()
        {
            caseDetailsHolder.SetActive(true);
        }

        public void HideCaseDetails()
        {
            caseDetailsHolder.SetActive(false);
        }

        public void UpdatePreviewBird(BirdName author)
        {
            if(author == BirdName.none)
            {
                birdHolder.SetActive(false);
                return;
            }
            birdHolder.SetActive(true);
            BirdData authorBird = GameDataManager.Instance.GetBird(author);
            if (authorBird == null)
            {
                Debug.LogError("Could not update preview bird visuals because bird[" + author.ToString() + "] was not mapped in the Colour Manager.");
                return;
            }
            authorImage.sprite = authorBird.faceSprite;
            authorNameText.color = authorBird.colour;
            authorNameText.text = SettingsManager.Instance.GetPlayerName(author);
        }

        public void initializeGalleryBird(int index, ColourManager.BirdName inBirdName)
        {
            PeanutBird peanutBird = allChatBirds[index];
            peanutBird.Initialize(inBirdName);

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


        public void UpdateCaseRatings(List<EndgameCaseData> inCaseData)
        {
            PlayerRatingData selectedRatingData;
            foreach (EndgameCaseData serverCase in inCaseData)
            {
                if(!caseDataMap.ContainsKey(serverCase.identifier))
                {
                    Debug.LogError("ERROR[UpdateCaseRatings]: Could not find matching case["+ serverCase.identifier+"] on client.");
                }
                EndgameCaseData matchingCase = caseDataMap[serverCase.identifier];
                foreach(KeyValuePair<int,EndgameTaskData> serverTask in serverCase.taskDataMap)
                {
                    if (!matchingCase.taskDataMap.ContainsKey(serverTask.Key))
                    {
                        Debug.LogError("ERROR[UpdateCaseRatings]: Could not access task[" + serverTask.Key.ToString() + "] for case because it wasn't in the queue.");
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
            if(numberOfPlayers == 0)
            {
                return;
            }
            float newSlideSpeed = numPlayersSpedUp == numberOfPlayers ? 4.0f : 4.0f - (4.0f * (numberOfPlayers - numPlayersSpedUp) / numberOfPlayers);

            setSlideSpeed(newSlideSpeed);
            GameManager.Instance.gameDataHandler.RpcSetSlideSpeed(slideSpeed);
        }

        public void showReaction(BirdName birdName, Reaction reaction)
        {
            PeanutBird matchingBird = GetChatBird(birdName);
            if(matchingBird == null)
            {
                Debug.LogError("Could not show reaction for bird["+birdName.ToString()+"] because it does not exist in the chatBirdMap.");
                return;
            }
            matchingBird.ShowReaction(reaction);
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
            PeanutBird targetBird = GetChatBird(receiver);
            if(targetBird == null)
            {
                Debug.LogError("Could not show player rating visual for receiver["+receiver.ToString()+"] because it doesn't exist in the chatBirdMap.");
                return;
            }
            PeanutBird senderBird = GetChatBird(sender);
            if(senderBird == null)
            {
                Debug.LogError("Could not show player rating visual for sender["+sender.ToString()+"] because it doesn't exist in the chatBirdMap.");
                return;
            }
            Vector3 startingPosition = senderBird.transform.position;
            Vector3 endingPosition = targetBird.transform.position;
            GameObject playerRatingVisualObject = Instantiate(playerRatingVisualPrefab, playerRatingVisualParent);
            PlayerRatingVisual playerRatingVisual = playerRatingVisualObject.GetComponent<PlayerRatingVisual>();
            if(playerRatingVisual == null)
            {
                Debug.LogError("Could not initialize the player rating visual because the component was missing from the instantiated object.");
                return;
            }
            playerRatingVisual.initialize(startingPosition, endingPosition, playerRatingMaxHeightTransform.position.y, targetBird);

        }

        public void InitializeSlidesProgressTracker()
        {
            List<GoalData> goals = new List<GoalData>();

            foreach (ResultData result in SettingsManager.Instance.resultPossibilities)
            {
                int requiredPoints = (int)(result.getRequiredPointThreshold(SettingsManager.Instance.gameMode.title));
                goals.Add(new GoalData(requiredPoints, result.resultName));
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

        public void ShowDayResult()
        {
            if(dayResultForm == null)
            {
                Debug.LogError("Could not show day result because day result form is missing.");
                return;
            }
            if(finalResultManager == null)
            {
                Debug.LogError("Could not show day result because final result manager is missing.");
                return;
            }
            ResultData gameResult = SettingsManager.Instance.GetDayResult();
            HideCaseDetails();
            ClearPreviousSlides();
            dayResultForm.resultMessageText.text = gameResult.bossMessage;
            dayResultForm.resultNameText.color = gameResult.resultTextColour;
            dayResultForm.resultNameText.text = gameResult.resultName;
            dayResultForm.gameObject.SetActive(true);
            finalResultManager.chosenReactionState = gameResult.finalFaceState;
            finalResultManager.responseSoundClip = gameResult.sfxToPlay;
            finalResultManager.Play();
        }

        public void ResolveDay()
        {
            inProgress = false;
            GameManager.Instance.gameFlowManager.timeRemainingInPhase = 0.0f;

            if(SettingsManager.Instance.DidPlayersPassDay())
            {
                if(GameManager.Instance.playerFlowManager.currentDay + 1 >= SettingsManager.Instance.gameMode.maxDays)
                {
                    foreach (BirdName bird in GameManager.Instance.gameFlowManager.gamePlayers.Keys)
                    {
                        if (!GameManager.Instance.gameFlowManager.disconnectedPlayers.Contains(bird) && bird != SettingsManager.Instance.birdName)
                        {
                            GameManager.Instance.gameFlowManager.addTransitionCondition("ratings_loaded:" + bird);
                            GameManager.Instance.gameFlowManager.addTransitionCondition("stats_loaded:" + bird);
                        }
                    }
                    //Send broadcast to players for ratings
                    GameManager.Instance.gameDataHandler.RpcSendStats();
                    StatTracker.Instance.SetServerPlayerStats();
                    sendRatingsToClients();
                    _phaseToTransitionTo = GamePhase.accolades;
                }
                else
                {
                    currentSlideContents = null;
                    currentSlideCaseIndex--;
                    GameManager.Instance.gameDataHandler.RpcUpdateGameDay(GameManager.Instance.playerFlowManager.currentDay + 1);
                    foreach (BirdName bird in GameManager.Instance.gameFlowManager.gamePlayers.Keys)
                    {
                        if (!GameManager.Instance.gameFlowManager.disconnectedPlayers.Contains(bird) && bird != SettingsManager.Instance.birdName)
                        {
                            GameManager.Instance.gameFlowManager.addTransitionCondition("day_loaded:" + bird);
                        }
                    }
                    
                    _phaseToTransitionTo = GamePhase.store;
                }
            }
            else
            {
                foreach (BirdName bird in GameManager.Instance.gameFlowManager.gamePlayers.Keys)
                {
                    if (!GameManager.Instance.gameFlowManager.disconnectedPlayers.Contains(bird) && bird != SettingsManager.Instance.birdName)
                    {
                        GameManager.Instance.gameFlowManager.addTransitionCondition("ratings_loaded:" + bird);
                        GameManager.Instance.gameFlowManager.addTransitionCondition("stats_loaded:" + bird);
                    }
                }

                //Send broadcast to players for ratings
                GameManager.Instance.gameDataHandler.RpcSendStats();
                StatTracker.Instance.SetServerPlayerStats();
                sendRatingsToClients();
                _phaseToTransitionTo = GamePhase.accolades;
            }
            GameManager.Instance.gameFlowManager.resolveTransitionCondition("slides_complete");
        }
    }
}