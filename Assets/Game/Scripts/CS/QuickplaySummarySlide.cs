using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;

namespace ChickenScratch
{
    public class QuickplaySummarySlide : MonoBehaviour
    {
        [SerializeField]
        private GameObject resultsHolder;

        [SerializeField]
        private GameObject happyReactionObject;
        [SerializeField]
        private GameObject neutralReactionObject;
        [SerializeField]
        private GameObject angryReactionObject;

        [SerializeField]
        private TMPro.TMP_Text resultText;

        [SerializeField]
        private GameObject eotmHolder;

        [SerializeField]
        private Image eotmBirdFaceImage;
        [SerializeField]
        private TMPro.TMP_Text eotmPlayerName;

        [SerializeField]
        private GameObject buttonsHolder;

        [SerializeField]
        private GameObject hostButtonsHolder;

        [SerializeField]
        private GameObject waitingOnHostHolder;

        [SerializeField]
        private float resultShowDuration;

        [SerializeField]
        private float eotmShowDuration;

        public bool isActive => _isActive;
        private bool _isActive = false;
        private string resultsSFX = "";
        private string eotmSFX = "";
        private float timeShowingResults = 0.0f;
        private float timeShowingEOTM = 0.0f;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (isActive)
            {
                if (timeShowingResults > 0)
                {
                    timeShowingResults += Time.deltaTime;
                    if (timeShowingResults > resultShowDuration)
                    {
                        timeShowingResults = 0.0f;
                        timeShowingEOTM = Time.deltaTime;
                        eotmHolder.SetActive(true);
                        AudioManager.Instance.PlaySound(eotmSFX);
                    }
                }
                else if (timeShowingEOTM > 0)
                {
                    timeShowingEOTM += Time.deltaTime;
                    if (timeShowingEOTM > eotmShowDuration)
                    {
                        timeShowingEOTM = 0.0f;
                        buttonsHolder.SetActive(true);
                    }
                }
            }
        }

        public void Initialize()
        {
            SetGameResult();
            SetEmployeeOfTheMonth();
            SetEndgameButtons();
        }

        public void Activate()
        {
            _isActive = true;
            resultsHolder.SetActive(true);
            AudioManager.Instance.PlaySound(resultsSFX);
            timeShowingResults = Time.deltaTime;
        }

        private void SetGameResult()
        {
            int totalPoints = 0;
            foreach (EndgameCaseData caseData in GameManager.Instance.playerFlowManager.slidesRound.caseDataMap.Values)
            {
                totalPoints += caseData.scoringData.GetTotalPoints();
            }
            
            List<GoalData> goals = new List<GoalData>();

            foreach (ResultData result in SettingsManager.Instance.resultPossibilities)
            {
                int requiredPoints = (int)(result.getRequiredPointThreshold(SettingsManager.Instance.gameMode.title));
                goals.Add(new GoalData(requiredPoints, result.resultName));
            }

            GoalData highestGoal = null;

            foreach (GoalData goal in goals)
            {
                if (highestGoal == null && goal.requiredPoints <= totalPoints)
                {
                    highestGoal = goal;
                }
                else if (goal.requiredPoints <= totalPoints && goal.requiredPoints > highestGoal.requiredPoints)
                {
                    highestGoal = goal;
                }
            }

            ResultData matchingResult = null;
            foreach (ResultData result in SettingsManager.Instance.resultPossibilities)
            {
                if (result.resultName == highestGoal.name)
                {
                    matchingResult = result;
                }
            }

            if (matchingResult != null)
            {
                //Set the result
                resultText.text = matchingResult.bossMessage;
                resultText.color = matchingResult.resultTextColour;
                switch (matchingResult.finalFaceState)
                {
                    case FinalEndgameResultManager.State.angry:
                        angryReactionObject.SetActive(true);
                        break;
                    case FinalEndgameResultManager.State.happy:
                        happyReactionObject.SetActive(true);
                        break;
                    case FinalEndgameResultManager.State.neutral:
                        neutralReactionObject.SetActive(true);
                        break;
                }
                resultsSFX = matchingResult.sfxToPlay;
            }

        }

        private void SetEmployeeOfTheMonth()
        {

        }

        private void SetEndgameButtons()
        {
            if (SettingsManager.Instance.isHost)
            {
                hostButtonsHolder.SetActive(true);
                waitingOnHostHolder.SetActive(false);
            }
            else
            {
                hostButtonsHolder.SetActive(false);
                waitingOnHostHolder.SetActive(true);
            }
        }
    }
}