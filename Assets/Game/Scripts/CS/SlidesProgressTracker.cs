using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static ChickenScratch.ColourManager;

namespace ChickenScratch
{
    public class SlidesProgressTracker : MonoBehaviour
    {

        [SerializeField]
        private Image fillBackground;

        [SerializeField]
        private GameObject progressFillPrefab;
        [SerializeField]
        private GameObject progressGoalPrefab;

        [SerializeField]
        private float caseRiseDuration;

        private float timeRising = 0.0f;

        private List<SlideProgressFill> progressFills = new List<SlideProgressFill>();
        private List<float> heightThresholds = new List<float>();
        private List<SlideProgressGoal> goalVisuals = new List<SlideProgressGoal>();
        private int currentProgressFillIndex = 0;

        private float highestGoalPoints;

        private int currentPoints = 0;
        private int pointsToAdd = 0;
        private float targetHeight;
        private float startingHeight;
        private float currentHeight;
        private float bgTopHeight;
        private float bgBottomHeight;
        private bool isInitialized = false;

        void OnEnable()
        {
            if (!isInitialized)
            {
                GameManager.Instance.playerFlowManager.slidesRound.InitializeSlidesProgressTracker();

            }
        }

        // Start is called before the first frame update
        void Start()
        {
            bgTopHeight = fillBackground.rectTransform.offsetMax.x;
            bgBottomHeight = fillBackground.rectTransform.offsetMin.x + 3;
            currentHeight = bgBottomHeight;

            //test();
        }

        void test()
        {
            //Set game goals
            List<WorkingGoalsManager.Goal> goals = new List<WorkingGoalsManager.Goal>();
            foreach (ResultData result in SettingsManager.Instance.resultPossibilities)
            {
                WorkingGoalsManager.Goal goal = new WorkingGoalsManager.Goal(WorkingGoalsManager.GoalType.endgame_result_state, (int)result.getRequiredPointThreshold("standard"), result.resultName);
                goals.Add(goal);
            }
            SetGameGoals(goals);
        }

        // Update is called once per frame
        void Update()
        {
            if (timeRising > 0)
            {
                timeRising += Time.deltaTime * GameManager.Instance.playerFlowManager.slidesRound.slideSpeed;
                if (currentProgressFillIndex >= progressFills.Count)
                {
                    return;
                }
                currentHeight = (targetHeight - startingHeight) * timeRising / caseRiseDuration + startingHeight;

                //If the current height would push the current section over its max height then max the current section and
                //move on to the next section
                if (heightThresholds[currentProgressFillIndex] < currentHeight)
                {
                    progressFills[currentProgressFillIndex].UpdateFillPercentage(1.0f);
                    currentProgressFillIndex++;
                    goalVisuals[currentProgressFillIndex].SetAsReached();
                }
                if (currentProgressFillIndex >= progressFills.Count)
                {
                    return;
                }

                //Update the current section based on the increase in points for the time rising
                float fillPercentage;
                if (currentProgressFillIndex == 0)
                {
                    fillPercentage = (currentHeight - bgBottomHeight) / (heightThresholds[currentProgressFillIndex] - bgBottomHeight);
                }
                else
                {
                    fillPercentage = (currentHeight - heightThresholds[currentProgressFillIndex - 1]) / (heightThresholds[currentProgressFillIndex] - heightThresholds[currentProgressFillIndex - 1]);
                }
                progressFills[currentProgressFillIndex].UpdateFillPercentage(fillPercentage);


                //If time rising exceeds the duration then stop
                if (timeRising > caseRiseDuration)
                {
                    currentPoints += pointsToAdd;
                    timeRising = 0.0f;
                }
            }
        }

        public void AddToPointTotal(int pointsAdded)
        {
            pointsToAdd = pointsAdded;
            StartCoroutine(AddToPointTotalDelayed());
        }

        IEnumerator AddToPointTotalDelayed()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            timeRising = Time.deltaTime;
            startingHeight = currentHeight;
            targetHeight = (pointsToAdd + currentPoints) / highestGoalPoints * (bgTopHeight - bgBottomHeight) + bgBottomHeight;
        }

        public void SetGameGoals(List<WorkingGoalsManager.Goal> goals)
        {
            float bgTopHeight = fillBackground.rectTransform.offsetMax.x;
            float bgBottomHeight = fillBackground.rectTransform.offsetMin.x + 3;
            bool pointsArePlayerDependent = SettingsManager.Instance.gameMode.caseDeliveryMode == GameModeData.CaseDeliveryMode.queue;
            float currentGoalHeightOffset;
            //Get the matching working goal from the provided goals
            List<ResultData> endgameGoals = new List<ResultData>();
            foreach (WorkingGoalsManager.Goal goal in goals)
            {
                foreach (ResultData endgameGoal in SettingsManager.Instance.resultPossibilities)
                {
                    if (endgameGoal.resultName == goal.name)
                    {
                        endgameGoals.Add(endgameGoal);
                    }
                }
            }

            //Order the goals based on their required points
            endgameGoals = endgameGoals.OrderBy(g => g.getRequiredPointThreshold(SettingsManager.Instance.gameMode.name)).ToList();

            highestGoalPoints = endgameGoals[endgameGoals.Count - 1].getRequiredPointThreshold(SettingsManager.Instance.gameMode.name);
            if (pointsArePlayerDependent)
            {
                highestGoalPoints *= GameManager.Instance.playerFlowManager.playerNameMap.Count;
            }

            List<GameObject> newGoalObjects = new List<GameObject>();

            for (int i = 0; i < endgameGoals.Count; i++)
            {
                if (i == endgameGoals.Count - 1)
                {
                    //Different handling for the last element - just create the goal and that's it
                    GameObject newGoalObject = Instantiate(progressGoalPrefab, transform);
                    SlideProgressGoal newGoal = newGoalObject.GetComponent<SlideProgressGoal>();
                    newGoal.Initialize(endgameGoals[i].shortFormIdentifier, endgameGoals[i].slideProgressBGColour, endgameGoals[i].slideProgressFillColour);
                    float goalHeight = progressFills[i - 1].GetTop();
                    RectTransform newGoalRectTransform = newGoal.GetComponent<RectTransform>();
                    currentGoalHeightOffset = i % 2 == 0 ? fillBackground.rectTransform.offsetMax.y + newGoalRectTransform.rect.height / 2 : fillBackground.rectTransform.offsetMin.y - newGoalRectTransform.rect.height / 2;
                    newGoalRectTransform.transform.position = new Vector3(goalHeight, newGoalRectTransform.position.y + currentGoalHeightOffset);
                    goalVisuals.Add(newGoal);
                    newGoalObjects.Add(newGoalObject);
                }
                else
                {
                    GameObject newGoalFillObject = Instantiate(progressFillPrefab, transform);
                    newGoalFillObject.transform.position = transform.position;
                    SlideProgressFill newGoalFill = newGoalFillObject.GetComponent<SlideProgressFill>();
                    float bottom = 0;
                    float currentGoalRequiredPoints = endgameGoals[i].getRequiredPointThreshold(SettingsManager.Instance.gameMode.name);
                    if (i == 0)
                    {
                        bottom = bgBottomHeight;
                    }
                    else
                    {
                        if (pointsArePlayerDependent)
                        {
                            currentGoalRequiredPoints *= GameManager.Instance.playerFlowManager.playerNameMap.Count;
                        }
                        bottom = currentGoalRequiredPoints / highestGoalPoints * (bgTopHeight - bgBottomHeight) + bgBottomHeight;
                    }
                    float nextGoalRequiredPoints = endgameGoals[i + 1].getRequiredPointThreshold(SettingsManager.Instance.gameMode.name);
                    if (pointsArePlayerDependent)
                    {
                        nextGoalRequiredPoints *= GameManager.Instance.playerFlowManager.playerNameMap.Count;
                    }
                    float top = nextGoalRequiredPoints / highestGoalPoints * (bgTopHeight - bgBottomHeight) + bgBottomHeight;

                    newGoalFill.SetVerticalSize(bottom, top);
                    newGoalFill.requiredPoints = nextGoalRequiredPoints;
                    newGoalFill.SetColours(endgameGoals[i].slideProgressBGColour, endgameGoals[i].slideProgressFillColour);
                    progressFills.Add(newGoalFill);
                    heightThresholds.Add(top);


                    GameObject newGoalObject = Instantiate(progressGoalPrefab, transform);
                    SlideProgressGoal newGoal = newGoalObject.GetComponent<SlideProgressGoal>();
                    newGoal.Initialize(endgameGoals[i].shortFormIdentifier, endgameGoals[i].slideProgressBGColour, endgameGoals[i].slideProgressFillColour);
                    float goalHeight = newGoalFill.GetBottom();
                    RectTransform newGoalRectTransform = newGoal.GetComponent<RectTransform>();
                    currentGoalHeightOffset = i % 2 == 0 ? fillBackground.rectTransform.offsetMax.y + newGoalRectTransform.rect.height / 2 : fillBackground.rectTransform.offsetMin.y - newGoalRectTransform.rect.height / 2;

                    newGoalRectTransform.transform.position = new Vector3(goalHeight, newGoalRectTransform.position.y + currentGoalHeightOffset);
                    goalVisuals.Add(newGoal);
                    newGoalObjects.Add(newGoalObject);

                }
            }

            foreach (GameObject newGoalObject in newGoalObjects)
            {
                newGoalObject.transform.SetAsLastSibling();
            }

            goalVisuals[0].SetAsReached();
            isInitialized = true;
        }

    }
}