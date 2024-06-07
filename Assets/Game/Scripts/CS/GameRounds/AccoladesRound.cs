﻿
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
        public WorkingGoalsManager workingGoalsManager;
        public Text workerWinText;
        public GameObject extraVoteRewardTipObject;

        [SerializeField]
        private Animator eotmAnimator;

        [SerializeField]
        private Image eotmLeftBirdArm;
        [SerializeField]
        private Image eotmRightBirdArm;

        private Dictionary<CameraDock.CameraState, CameraDock> cameraDockMap;
        private Dictionary<int, GameObject> successTokenMap, failTokenMap;
        private float timeSoFar = 0.0f, cameraStateTime = 0.0f;
        public float timeToShowCabinetResults = 2.0f, timeToShowResults = 4.0f, timeShowingResults = 8.0f;
        private bool hasShownResults = false, hasShownCabinetResults = false;
        private int numberOfRoundResultsShown = 0, totalFails = 0, totalSuccesses = 0, totalPoints = 0;
        private bool hasEOTMRisen = false;

        public AccoladesStatManager playerStatsManager;

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
                    currentCameraState != CameraDock.CameraState.result)
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
                else if (workingGoalsManager.active)
                {
                    return;
                }
                else
                {
                    timeSoFar += Time.deltaTime;

                    if (timeSoFar > timeToShowCabinetResults &&
                        !hasShownCabinetResults)
                    {
                        accoladesBackground.color = new Color(0, 0, 0, 0);
                        accoladesSpriteContainer.SetActive(true);

                        workingGoalsManager.startShowingWorkingGoals(SettingsManager.Instance.gameMode.goalPointsPerCharacter < totalPoints);
                        hasShownCabinetResults = true;
                        timeSoFar = 0.0f;
                    }
                    else if (timeSoFar > timeToShowResults &&
                        !hasShownResults)
                    {
                        resultsContainerObject.SetActive(true);
                        hasShownResults = true;
                        timeSoFar = 0.0f;
                    }
                    else if (timeSoFar > timeShowingResults &&
                        hasShownResults)
                    {
                        //workingGoalsManager.gameObject.SetActive(false);
                        //accoladesSpriteContainer.SetActive(false);
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
                accoladesObjectRect.anchoredPosition = Vector3.Lerp(currentCameraDock.position, nextCameraDock.position, cameraStateTime * currentCameraDock.transitionMoveSpeed);
                accoladesObjectRect.localScale = Vector3.Lerp(currentCameraDock.zoom, nextCameraDock.zoom, cameraStateTime * currentCameraDock.transitionZoomSpeed);

                if (cameraStateTime * currentCameraDock.transitionMoveSpeed > 1)
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
            allAccoladeBirdRows[index].pinImage.color = ColourManager.Instance.birdMap[birdName].colour;
            allAccoladeBirdRows[index].birdHeadImage.sprite = ColourManager.Instance.birdMap[birdName].faceSprite;
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
                    currentBird = ColourManager.Instance.birdMap[playerReviewStatus.Key];
                    currentRow.playerNameText.color = currentBird.colour;
                    currentRow.playerNameText.text = playerReviewStatus.Value.playerName;
                    currentRow.StartPlacing();
                }
            }

            //Set the accolades
            setAccolades(bestEmployeeCandidate.birdName, bestEmployeeCandidate.playerName);
            isActive = true;

            List<WorkingGoalsManager.Goal> goals = new List<WorkingGoalsManager.Goal>();
            
            foreach (SettingsManager.EndgameResult result in SettingsManager.Instance.resultPossibilities)
            {
                int requiredPoints = (int)(result.getRequiredPointThreshold(SettingsManager.Instance.gameMode.name) * GameManager.Instance.playerFlowManager.playerNameMap.Count);
                goals.Add(new WorkingGoalsManager.Goal(result.goal, requiredPoints, result.resultName));
            }
            
            List<WorkingGoalsManager.PlayerPoints> casePoints = new List<WorkingGoalsManager.PlayerPoints>();
            foreach (KeyValuePair<int, EndgameCaseData> caseData in GameManager.Instance.playerFlowManager.slidesRound.caseDataMap)
            {
                int points = caseData.Value.GetTotalPoints();
                casePoints.Add(new WorkingGoalsManager.PlayerPoints(caseData.Value.GetGuesser(), points, caseData.Value));
            }

            workingGoalsManager.initializeWorkingGoals(goals, casePoints);

            //leftTrashObjects[Random.Range(0, leftTrashObjects.Count)].SetActive(true);
            rightTrashObjects[Random.Range(0, rightTrashObjects.Count)].SetActive(true);
        }


        public void setAccolades(ColourManager.BirdName bestBird, string bestPlayerName)
        {
            PlayerFlowManager.employeeOfTheMonth = bestBird;
            employeeOfTheMonthPlayerText.text = bestPlayerName;
            employeeOfTheMonthPlayerText.color = ColourManager.Instance.birdMap[bestBird].colour;
            employeeOfTheMonthPlayerImage.sprite = ColourManager.Instance.birdMap[bestBird].faceSprite;

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

        public float transitionMoveSpeed;
        public float transitionZoomSpeed;

        public float restingTime;
        public bool restingFinished = false;

        public void setRelativePositions()
        {
            position = Camera.main.ViewportToWorldPoint(position);
            zoom = Camera.main.ViewportToWorldPoint(position);
        }
    }
}