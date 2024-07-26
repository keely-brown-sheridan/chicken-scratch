using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ChickenScratch.CaseEmail;
using static ChickenScratch.ColourManager;

namespace ChickenScratch
{
    public class ReviewRound : PlayerRound
    {
        public BirdName accuserBird => _accuserBird;
        private BirdName _accuserBird;
        public BirdName accusedBird => _accusedBird;
        private BirdName _accusedBird;

        [SerializeField]
        private List<GameObject> taskEmailSectionPrefabs;

        [SerializeField]
        private Image birdFaceImage;

        [SerializeField]
        private Image backgroundImage;

        [SerializeField]
        private TMPro.TMP_Text playerNameText;

        [SerializeField]
        private TMPro.TMP_Text starsText;

        [SerializeField]
        private TMPro.TMP_Text eyesText;

        [SerializeField]
        private TMPro.TMP_Text birdBucksText;

        [SerializeField]
        private Transform leftTaskVisualHolder, middleTaskVisualHolder, rightTaskVisualHolder;

        [SerializeField]
        private float drawingScale;

        [SerializeField]
        private GameObject waitingOnPlayerPrefab;

        [SerializeField]
        private Transform waitingOnPlayerHolder;

        [SerializeField]
        private GameObject accuseConfirmationObject;

        [SerializeField]
        private TMPro.TMP_Text accusationConfirmationText;

        [SerializeField]
        private Image accusationConfirmationBirdImage;

        private Dictionary<CaseEmailTaskType, GameObject> taskEmailSectionPrefabMap = new Dictionary<CaseEmailTaskType, GameObject>();
        private Dictionary<BirdName, GameObject> waitingOnPlayersObjectMap = new Dictionary<BirdName, GameObject>();
        private Dictionary<ColourManager.BirdName, PlayerReviewData> playerReviewMap = new Dictionary<ColourManager.BirdName, PlayerReviewData>();
        private ColourManager.BirdName currentBird;
        private List<ColourManager.BirdName> allOtherBirds = new List<ColourManager.BirdName>();
        private int currentBirdIndex;
        private List<ColourManager.BirdName> activePlayers = new List<BirdName>();
        

        private void Start()
        {
            //test();
        }

        private void test()
        {
            GameManager.Instance.GenerateTestingData();
            StartRound();
        }

        public override void StartRound()
        {
            base.StartRound();
            _accuserBird = BirdName.none;
            _accusedBird = BirdName.none;
            taskEmailSectionPrefabMap.Clear();
            foreach (GameObject taskEmailSectionPrefab in taskEmailSectionPrefabs)
            {
                CaseEmailSection caseEmailSection = taskEmailSectionPrefab.GetComponent<CaseEmailSection>();
                taskEmailSectionPrefabMap.Add(caseEmailSection.taskType, taskEmailSectionPrefab);
            }
            activePlayers = SettingsManager.Instance.GetAllActiveBirds();
            allOtherBirds = activePlayers;
            ClearPlayerWaitingVisuals();
            foreach (BirdName bird in allOtherBirds)
            {
                GameObject waitingOnPlayerObject = Instantiate(waitingOnPlayerPrefab, waitingOnPlayerHolder);
                waitingOnPlayersObjectMap.Add(bird, waitingOnPlayerObject);
                WaitingOnPlayerVisual waitingOnPlayerVisual = waitingOnPlayerObject.GetComponent<WaitingOnPlayerVisual>();
                waitingOnPlayerVisual.Initialize(bird);
            }
            allOtherBirds.Remove(SettingsManager.Instance.birdName);

            currentBirdIndex = 0;
            currentBird = allOtherBirds[0];

            PlayerReviewData currentPlayerData;
            playerReviewMap.Clear();
            foreach (EndgameCaseData caseData in GameManager.Instance.playerFlowManager.slidesRound.caseDataMap.Values)
            {
                
                foreach(KeyValuePair<int,EndgameTaskData> task in caseData.taskDataMap)
                {
                    if(!playerReviewMap.ContainsKey(task.Value.assignedPlayer))
                    {
                        playerReviewMap.Add(task.Value.assignedPlayer, new PlayerReviewData());
                    }
                    currentPlayerData = playerReviewMap[task.Value.assignedPlayer];
                    currentPlayerData.caseIndices.Add(caseData.identifier);
                    currentPlayerData.caseTaskMap.Add(caseData.identifier, task.Key);
                    currentPlayerData.birdBucksEarned += caseData.GetPointsForPlayerOnTask(task.Value.assignedPlayer);
                    currentPlayerData.numberOfStars += task.Value.ratingData.likeCount;
                    currentPlayerData.numberOfEyes += task.Value.ratingData.dislikeCount;
                }
            }

            foreach (ColourManager.BirdName bird in allOtherBirds)
            {
                if (!playerReviewMap.ContainsKey(bird))
                {
                    playerReviewMap.Add(bird, new PlayerReviewData());
                }
                currentPlayerData = playerReviewMap[bird];

                if(currentPlayerData.caseIndices.Count != 0)
                {
                    currentPlayerData.currentCaseIndex = 0;
                }

                currentPlayerData.playerName = GameManager.Instance.playerFlowManager.playerNameMap[bird];
                BirdData playerBird = GameDataManager.Instance.GetBird(bird);
                if(playerBird != null)
                {
                    currentPlayerData.playerColour = playerBird.colour;
                    currentPlayerData.faceSprite = playerBird.faceSprite;
                    currentPlayerData.bgColour = playerBird.bgColour;
                }
                
            }

            UpdateCurrentCase();
        }

        private void ClearPlayerWaitingVisuals()
        {
            List<Transform> transformsToDestroy = new List<Transform>();
            foreach (Transform child in waitingOnPlayerHolder)
            {
                transformsToDestroy.Add(child);
            }
            for (int i = transformsToDestroy.Count - 1; i >= 0; i--)
            {
                Destroy(transformsToDestroy[i].gameObject);
            }
        }

        private void UpdateCurrentCase()
        {
            //Clear previous task visualizations
            ClearTaskVisualizations();

            PlayerReviewData reviewData = playerReviewMap[currentBird];
            playerNameText.text = reviewData.playerName;
            playerNameText.color = reviewData.playerColour;
            

            birdFaceImage.sprite = reviewData.faceSprite;

            starsText.text = "x" + reviewData.numberOfStars.ToString();
            eyesText.text = "x" + reviewData.numberOfEyes.ToString();

            birdBucksText.text = reviewData.birdBucksEarned.ToString();
            backgroundImage.color = reviewData.bgColour;

            //Get the current task
            int caseIndex = reviewData.caseIndices[reviewData.currentCaseIndex];
            int taskIndex = reviewData.caseTaskMap[caseIndex];
            EndgameCaseData caseData = GameManager.Instance.playerFlowManager.slidesRound.caseDataMap[caseIndex];
            EndgameTaskData taskData = caseData.taskDataMap[taskIndex];
            CreateTaskVisualization(caseData, taskData, middleTaskVisualHolder);

            //Get the previous task if there was one
            if(caseData.taskDataMap.ContainsKey(taskIndex -1))
            {
                CreateTaskVisualization(caseData, caseData.taskDataMap[taskIndex - 1], leftTaskVisualHolder);
            }
            else
            {
                CreateIntroVisualization(caseData, leftTaskVisualHolder);
            }

            //Get the next task if there was one
            if(caseData.taskDataMap.ContainsKey(taskIndex + 1))
            {
                CreateTaskVisualization(caseData, caseData.taskDataMap[taskIndex + 1], rightTaskVisualHolder);
            }
        }

        private void CreateTaskVisualization(EndgameCaseData caseData, EndgameTaskData taskData, Transform parent)
        {
            switch (taskData.taskType)
            {
                case TaskData.TaskType.base_drawing:
                case TaskData.TaskType.copy_drawing:
                case TaskData.TaskType.add_drawing:
                case TaskData.TaskType.prompt_drawing:
                case TaskData.TaskType.compile_drawing:
                case TaskData.TaskType.blender_drawing:
                    GameObject drawingCaseEmailSectionObject = Instantiate(taskEmailSectionPrefabMap[CaseEmailTaskType.drawing], parent);
                    DrawingCaseEmailSection drawingCaseEmailSection = drawingCaseEmailSectionObject.GetComponent<DrawingCaseEmailSection>();
                    drawingCaseEmailSection.Initialize(taskData.drawingData, taskData.ratingData, drawingScale);
                    break;
                case TaskData.TaskType.prompting:
                    GameObject promptCaseEmailSectionObject = Instantiate(taskEmailSectionPrefabMap[CaseEmailTaskType.prompt], parent);
                    PromptCaseEmailSection promptCaseEmailSection = promptCaseEmailSectionObject.GetComponent<PromptCaseEmailSection>();
                    promptCaseEmailSection.Initialize(taskData.promptData, taskData.ratingData);
                    break;
                case TaskData.TaskType.morph_guessing:
                case TaskData.TaskType.base_guessing:
                case TaskData.TaskType.competition_guessing:
                    GameObject guessCaseEmailSectionObject = Instantiate(taskEmailSectionPrefabMap[CaseEmailTaskType.guess], parent);
                    GuessCaseEmailSection guessCaseEmailSection = guessCaseEmailSectionObject.GetComponent<GuessCaseEmailSection>();
                    guessCaseEmailSection.Initialize(caseData.correctWordIdentifierMap, caseData.guessData, taskData.ratingData);
                    break;

            }
        }

        private void CreateIntroVisualization(EndgameCaseData caseData, Transform parent)
        {
            GameObject originalCaseEmailSectionObject = Instantiate(taskEmailSectionPrefabMap[CaseEmailTaskType.original], parent);
            OriginalCaseEmailSection originalCaseEmailSection = originalCaseEmailSectionObject.GetComponent<OriginalCaseEmailSection>();
            CaseWordData correctPrefix = GameDataManager.Instance.GetWord(caseData.correctWordIdentifierMap[1]);
            CaseWordData correctNoun = GameDataManager.Instance.GetWord(caseData.correctWordIdentifierMap[2]);
            originalCaseEmailSection.Initialize(correctPrefix.value, correctNoun.value);
        }

        private void ClearTaskVisualizations()
        {
            List<Transform> transformsToDestroy = new List<Transform>();
            foreach (Transform child in leftTaskVisualHolder)
            {
                transformsToDestroy.Add(child);
            }
            foreach (Transform child in middleTaskVisualHolder)
            {
                transformsToDestroy.Add(child);
            }
            foreach (Transform child in rightTaskVisualHolder)
            {
                transformsToDestroy.Add(child);
            }
            for (int i = transformsToDestroy.Count - 1; i >= 0; i--)
            {
                Destroy(transformsToDestroy[i].gameObject);
            }
        }

        public void Accuse()
        {
            PlayerReviewData reviewData = playerReviewMap[currentBird];
            accusationConfirmationText.text = "Are you sure you want to accuse " + reviewData.playerName + "?";
            accusationConfirmationBirdImage.sprite = reviewData.faceSprite;

            //Open accuse confirmation prompt
            accuseConfirmationObject.SetActive(true);
        }

        public void NextBird()
        {
            currentBirdIndex++;
            if(currentBirdIndex >= allOtherBirds.Count)
            {
                currentBirdIndex = 0;
            }
            currentBird = allOtherBirds[currentBirdIndex];
            UpdateCurrentCase();
        }

        public void Skip()
        {
            GameManager.Instance.gameDataHandler.CmdFinishWithReview(SettingsManager.Instance.birdName);
        }

        public void NextCase()
        {
            PlayerReviewData reviewData = playerReviewMap[currentBird];
            reviewData.currentCaseIndex++;
            if (reviewData.currentCaseIndex >= reviewData.caseIndices.Count)
            {
                reviewData.currentCaseIndex = 0;
            }
            UpdateCurrentCase();
        }

        public void PreviousCase()
        {
            PlayerReviewData reviewData = playerReviewMap[currentBird];
            reviewData.currentCaseIndex--;
            if (reviewData.currentCaseIndex < 0)
            {
                reviewData.currentCaseIndex = reviewData.caseIndices.Count - 1;
            }
            UpdateCurrentCase();
        }

        public void FinishWithReviewForPlayer(ColourManager.BirdName player)
        {
            GameManager.Instance.gameDataHandler.RpcRemoveReviewWaitingForPlayerVisual(player);
            if (activePlayers.Contains(player))
            {
                activePlayers.Remove(player);
                if (activePlayers.Count == 0)
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

        public void ConfirmAccuse()
        {
            accuseConfirmationObject.SetActive(false);
            GameManager.Instance.gameDataHandler.CmdAccusePlayer(SettingsManager.Instance.birdName, currentBird);
        }

        public void CancelAccuse()
        {
            accuseConfirmationObject.SetActive(false);
        }

        public void SetAccusation(BirdName inAccusingPlayer, BirdName inAccusedPlayer)
        {
            _accusedBird = inAccusedPlayer;
            _accuserBird = inAccusingPlayer;
        }
    }
}

