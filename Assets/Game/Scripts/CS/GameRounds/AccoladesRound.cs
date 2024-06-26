
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;
using static ChickenScratch.ColourManager;

namespace ChickenScratch
{
    public class AccoladesRound : PlayerRound
    {
        public TMPro.TextMeshProUGUI employeeOfTheMonthPlayerText, upForReviewPlayerText;
        public Image employeeOfTheMonthPlayerImage, upForReviewPlayerImage;
        public GameObject winMarkerObject, loseMarkerObject, resultsContainerObject, cabinetResultsObject;

        public RectTransform accoladesObjectRect;
        public ColourManager.BirdName upForReview;
        public List<AccoladesBirdRow> allAccoladeBirdRows;
        public List<IndexMap> allSuccessTokens, allFailTokens;
        public List<GameObject> leftTrashObjects, rightTrashObjects;
        public bool isActive = false;

        public List<CameraDock> cameraDocks;
        public CameraDock.CameraState currentCameraState;
        public CameraDock currentCameraDock, nextCameraDock;

        public GameObject accoladesSpriteContainer;
        public Image accoladesBackground;
        public Text workerWinText;
        public GameObject extraVoteRewardTipObject;

        [SerializeField]
        private Animator eotmAnimator;

        [SerializeField]
        private Image eotmLeftBirdArm;
        [SerializeField]
        private Image eotmRightBirdArm;

        [SerializeField]
        private float cardPlacementWaitVariance;

        private Dictionary<CameraDock.CameraState, CameraDock> cameraDockMap;
        private Dictionary<int, GameObject> successTokenMap, failTokenMap;
        private float timeSoFar = 0.0f, cameraStateTime = 0.0f;
        public float timeShowingEOTM = 6.0f;
        private bool hasShownResults = false, hasShownCabinetResults = false;
        private int numberOfRoundResultsShown = 0, totalFails = 0, totalSuccesses = 0, totalPoints = 0;
        private bool hasEOTMRisen = false;

        public AccoladesStatManager playerStatsManager;

        private void Start()
        {
            //Test();
            
        }

        private void Test()
        {
            EndgameCaseData testCase = new EndgameCaseData();

            //create task queue
            EndgameTaskData testTask = new EndgameTaskData();
            testTask.taskType = TaskData.TaskType.base_guessing;
            testTask.ratingData = new PlayerRatingData() { likeCount = 1, target = BirdName.red };
            testTask.assignedPlayer = BirdName.red;
            GameManager.Instance.playerFlowManager.playerNameMap.Add(BirdName.red, "beebodeebo");
            testCase.taskDataMap.Add(1, testTask);
            testCase.correctWordIdentifierMap = new Dictionary<int, string>() { { 1, "prefixes-DRAGGING" },{ 2, "nouns-AARDVARK" } };

            GameManager.Instance.playerFlowManager.slidesRound.caseDataMap.Add(1, testCase);
            StartRound();
        }

        public override void StartRound()
        {
            base.StartRound();

            initializeAccoladesRound();
        }

        private void Update()
        {
            if (isActive)
            {
                if (currentCameraState != CameraDock.CameraState.reset &&
                    currentCameraState != CameraDock.CameraState.accs_rest)
                {
                    cameraUpdate();
                    if (currentCameraDock.state == CameraDock.CameraState.accs_rest && !currentCameraDock.restingFinished)
                    {
                        if (!hasEOTMRisen)
                        {
                            AudioManager.Instance.PlaySound("sfx_vote_env_employee_react");
                            eotmAnimator.SetTrigger("Rise");
                            hasEOTMRisen = true;
                        }

                    }
                }
                else
                {
                    timeSoFar += Time.deltaTime;

                    if (timeSoFar > timeShowingEOTM)
                    {
                        isActive = false;
                    }
                }

            }

        }

        public void SetPlayerAccoladeCards(Dictionary<BirdName, AccoladesStatManager.StatRole> statRoleMap)
        {
            foreach (KeyValuePair<BirdName, AccoladesStatManager.StatRole> statRole in statRoleMap)
            {
                AccoladesBirdRow currentCard = allAccoladeBirdRows.Where(br => br.birdName == statRole.Key).ToList()[0];
                currentCard.statRoleText.text = statRole.Value.name;
                currentCard.statDescriptionText.text = statRole.Value.description;
            }
        }

        private void cameraUpdate()
        {
            cameraStateTime += Time.deltaTime;

            if (!currentCameraDock.restingFinished)
            {
                if (currentCameraDock.restingTime < cameraStateTime)
                {
                    currentCameraDock.restingFinished = true;
                    cameraStateTime = 0.0f;
                }
            }
            else
            {
                //Transition
                float transitionRatio = cameraStateTime / currentCameraDock.transitionDuration;
                accoladesObjectRect.anchoredPosition = Vector3.Lerp(currentCameraDock.position, nextCameraDock.position, transitionRatio);
                accoladesObjectRect.localScale = Vector3.Lerp(currentCameraDock.zoom, nextCameraDock.zoom, transitionRatio);

                if (transitionRatio > 1)
                {
                    cameraStateTime = 0.0f;
                    currentCameraState = currentCameraDock.nextState;
                    currentCameraDock = cameraDockMap[currentCameraState];
                    nextCameraDock = cameraDockMap[currentCameraDock.nextState];
                }
            }
        }

        public void initializeAccoladeBirdRow(int index, BirdName birdName)
        {
            allAccoladeBirdRows[index].birdName = birdName;
            Bird accoladeBird = ColourManager.Instance.GetBird(birdName);
            if(accoladeBird == null)
            {
                Debug.LogError("Could not initialize accolade bird row colours because bird[] has not been mapped in the ColourManager.");
            }
            else
            {
                allAccoladeBirdRows[index].pinImage.color = accoladeBird.colour;
                allAccoladeBirdRows[index].birdHeadImage.sprite = accoladeBird.faceSprite;
            }
            
            allAccoladeBirdRows[index].gameObject.SetActive(true);
            allAccoladeBirdRows[index].isInitialized = true;
        }

        private void initializeAccoladesRound()
        {
            cameraDockMap = new Dictionary<CameraDock.CameraState, CameraDock>();
            foreach (CameraDock cameraDock in cameraDocks)
            {
                cameraDockMap.Add(cameraDock.state, cameraDock);
            }
            currentCameraDock = cameraDockMap[currentCameraState];
            nextCameraDock = cameraDockMap[currentCameraDock.nextState];

            successTokenMap = new Dictionary<int, GameObject>();
            failTokenMap = new Dictionary<int, GameObject>();

            Dictionary<ColourManager.BirdName, PlayerReviewStatus> playerReviewStatusMap = new Dictionary<ColourManager.BirdName, PlayerReviewStatus>();
            PlayerReviewStatus bestEmployeeCandidate = null;

            foreach (IndexMap successToken in allSuccessTokens)
            {
                successTokenMap.Add(successToken.index, successToken.gameObject);
            }
            foreach (IndexMap failToken in allFailTokens)
            {
                failTokenMap.Add(failToken.index, failToken.gameObject);
            }

            foreach (KeyValuePair<ColourManager.BirdName, string> player in GameManager.Instance.playerFlowManager.playerNameMap)
            {
                playerReviewStatusMap.Add(player.Key, new PlayerReviewStatus());
                playerReviewStatusMap[player.Key].birdName = player.Key;
                playerReviewStatusMap[player.Key].playerName = player.Value;

            }
            foreach (EndgameCaseData currentCase in GameManager.Instance.playerFlowManager.slidesRound.caseDataMap.Values)
            {
                foreach(EndgameTaskData currentTask in currentCase.taskDataMap.Values)
                {
                    PlayerRatingData rating = currentTask.ratingData;
                    if (playerReviewStatusMap.ContainsKey(rating.target))
                    {
                        playerReviewStatusMap[rating.target].likeCount += rating.likeCount;
                        playerReviewStatusMap[rating.target].netRating += rating.likeCount;
                    }
                }
            }

            workerWinText.text = "\n" + (GameManager.Instance.playerFlowManager.playerNameMap.Count * 2) + " points";

            if ((totalSuccesses) == GameManager.Instance.playerFlowManager.playerNameMap.Count)
            {
                winMarkerObject.SetActive(true);
            }
            else
            {
                loseMarkerObject.SetActive(true);
            }

            Bird currentBird;
            //Iterate over each player to determine candidates for eotm and ufr
            foreach (KeyValuePair<ColourManager.BirdName, PlayerReviewStatus> playerReviewStatus in playerReviewStatusMap)
            {
                if (bestEmployeeCandidate == null)
                {
                    bestEmployeeCandidate = playerReviewStatus.Value;
                }
                else
                {
                    if (bestEmployeeCandidate.netRating == playerReviewStatus.Value.netRating)
                    {
                        if (bestEmployeeCandidate.totalTimeTaken > playerReviewStatus.Value.totalTimeTaken)
                        {
                            bestEmployeeCandidate = playerReviewStatus.Value;
                        }
                    }
                    else if (bestEmployeeCandidate.netRating < playerReviewStatus.Value.netRating)
                    {
                        bestEmployeeCandidate = playerReviewStatus.Value;
                    }
                }

                AccoladesBirdRow currentRow;
                if (allAccoladeBirdRows.Where(abr => abr.isInitialized).Any(abr => abr.birdName == playerReviewStatus.Key))
                {
                    currentRow = allAccoladeBirdRows.Where(abr => abr.isInitialized).Single(abr => abr.birdName == playerReviewStatus.Key);

                    //Set the stats for the corkboard
                    currentRow.gameObject.SetActive(true);
                    currentBird = ColourManager.Instance.GetBird(playerReviewStatus.Key);
                    if(currentBird == null)
                    {
                        Debug.LogError("Could not set stats for the review bird[" + playerReviewStatus.Key.ToString() + "] because it has not been mapped in the Colour Manager.");
                    }
                    else
                    {
                        currentRow.playerNameText.color = currentBird.colour;
                    }
                    
                    currentRow.playerNameText.text = playerReviewStatus.Value.playerName;
                    float randomizedPlacementWait = Random.Range(0, cardPlacementWaitVariance);

                    currentRow.StartPlacing(randomizedPlacementWait);
                }
            }

            //Set the accolades
            setAccolades(bestEmployeeCandidate.birdName, bestEmployeeCandidate.playerName);
            isActive = true;
        }


        public void setAccolades(ColourManager.BirdName bestBirdName, string bestPlayerName)
        {
            PlayerFlowManager.employeeOfTheMonth = bestBirdName;
            employeeOfTheMonthPlayerText.text = bestPlayerName;
            Bird bestBird = ColourManager.Instance.GetBird(bestBirdName);
            if(bestBird == null)
            {
                Debug.LogError("Could not map colour for the best bird[] because it has not been initialized in the Colour Manager.");
            }
            else
            {
                employeeOfTheMonthPlayerText.color = bestBird.colour;
                employeeOfTheMonthPlayerImage.sprite = bestBird.faceSprite;
            }
            

            //Choose a random bird for the bird arms lifting the award
            int birdIndex = UnityEngine.Random.Range(0, ColourManager.Instance.allBirds.Count);
            Bird randomBird = ColourManager.Instance.allBirds[birdIndex];

            eotmLeftBirdArm.sprite = randomBird.armSprite;
            eotmRightBirdArm.sprite = randomBird.armSprite;
        }
    }

    class PlayerReviewStatus
    {
        public int netRating = 0;
        public int likeCount = 0, dislikeCount = 0;
        public float totalTimeTaken = 0.0f;
        public int totalTimesFailed = 0;
        public ColourManager.BirdName birdName = ColourManager.BirdName.none;
        public string playerName = "";
    }

    [System.Serializable]
    public class CameraDock
    {
        public enum CameraState
        {
            stats_rest, stats_to_accs, accs_rest, accs_to_result, result, reset
        }
        public CameraState state;
        public CameraState nextState;

        public Vector3 position;
        public Vector3 zoom;

        public float transitionDuration;

        public float restingTime;
        public bool restingFinished = false;

        public void setRelativePositions()
        {
            position = Camera.main.ViewportToWorldPoint(position);
            zoom = Camera.main.ViewportToWorldPoint(position);
        }
    }
}