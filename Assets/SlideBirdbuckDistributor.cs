using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class SlideBirdbuckDistributor : MonoBehaviour
    {
        [SerializeField]
        private GameObject distributedBirdbuckPrefab;

        [SerializeField]
        private Transform birdBucksRemainingImageTransform;

        [SerializeField]
        private TMPro.TMP_Text birdBucksRemainingText;

        [SerializeField]
        private float distributionTime;

        [SerializeField]
        private float delayTime;

        [SerializeField]
        private float stealReachingTime;

        [SerializeField]
        private float stealPullingTime;

        [SerializeField]
        private string distributeSFX;

        [SerializeField]
        private Transform stealingBirdArmTransform;

        [SerializeField]
        private Transform stolenBirdbuckTransform;

        [SerializeField]
        private TMPro.TMP_Text stolenBirdbuckText;

        [SerializeField]
        private Transform stealingReachingTarget;

        [SerializeField]
        private Transform stealingHandTransform;

        [SerializeField]
        private CertificationEffectIndicator shareholdersEffectIndicator;

        private enum State
        {
            delay, distribute, stealing_reach, stealing_pull, inactive
        }
        private State currentState = State.delay;

        private float timeActive = 0f;
        private float timeSinceLastDistribution = 0f;
        private float distributionFrequency = 0f;
        private int birdBucksRemaining = 0;
        private int totalBirdBucksToDistribute = 0;
        private CaseScoringData caseScoringData;
        private List<SummarySlideSection> sections = new List<SummarySlideSection>();
        private Vector3 spawnPosition;
        private int birdBucksToSteal = 0;
        private Vector3 stealingArmStartingPosition;

        private void OnEnable()
        {
            stealingArmStartingPosition = stealingBirdArmTransform.position;
        }

        public void Initialize(CaseScoringData inCaseScoringData, List<SummarySlideSection> inSummarySlideSections)
        {
            caseScoringData = inCaseScoringData;
            sections = inSummarySlideSections;
            //Do not steal birdbucks from players if the total goes below 0 - it can affect the total but not the money they've earned to this point
            totalBirdBucksToDistribute = Mathf.Max(0,caseScoringData.GetTotalPoints());
            birdBucksRemaining = totalBirdBucksToDistribute;

            EndgameCaseData caseData = GameManager.Instance.playerFlowManager.slidesRound.caseDataMap[inCaseScoringData.caseID];


            if (inSummarySlideSections.Count != 0)
            {
                birdBucksToSteal = birdBucksRemaining % inSummarySlideSections.Count;
                distributionFrequency = Mathf.Min((distributionTime - delayTime) / birdBucksRemaining * inSummarySlideSections.Count, 0.25f);
            }
            else
            {
                birdBucksToSteal = 0;
                distributionFrequency = 0.25f;
            }

            bool caseHasShareholdersCertification = GameManager.Instance.playerFlowManager.CaseHasCertification(caseData.caseTypeName, "Shareholders");
            if (caseHasShareholdersCertification)
            {
                FloatCertificationData shareholderCertification = (FloatCertificationData)GameDataManager.Instance.GetCertification("Shareholders");
                if (shareholderCertification != null)
                {
                    shareholdersEffectIndicator.Show(shareholderCertification, shareholderCertification.value.ToString()+  " birdbucks distributed to shareholders");
                    birdBucksToSteal = totalBirdBucksToDistribute;
                    totalBirdBucksToDistribute = (int)(totalBirdBucksToDistribute * (1-shareholderCertification.value));
                    birdBucksToSteal -= totalBirdBucksToDistribute;
                }

            }

            birdBucksRemainingText.text = birdBucksRemaining.ToString();

            if(birdBucksToSteal > 0)
            {
                stolenBirdbuckText.text = birdBucksToSteal.ToString();
                currentState = State.stealing_reach;
            }
            else
            {
                currentState = State.delay;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if(currentState != State.inactive)
            {
                timeActive += Time.deltaTime * GameManager.Instance.playerFlowManager.slidesRound.slideSpeed;
            }
            float timeRatio;
            switch (currentState)
            {
                case State.stealing_reach:
                    if(timeActive > stealReachingTime)
                    {
                        timeActive = 0f;
                        birdBucksRemaining -= birdBucksToSteal;
                        birdBucksRemainingText.text = birdBucksRemaining.ToString();
                        stolenBirdbuckTransform.gameObject.SetActive(true);
                        currentState = State.stealing_pull;
                        AudioManager.Instance.PlaySound(distributeSFX, true);
                    }
                    timeRatio = timeActive / stealReachingTime;
                    stealingBirdArmTransform.position = Vector3.Lerp(stealingArmStartingPosition, stealingReachingTarget.position, timeRatio);
                    break;
                case State.stealing_pull:
                    if(timeActive > stealPullingTime)
                    {
                        timeActive = 0f;
                        currentState = State.delay;
                        return;
                    }
                    timeRatio = timeActive / (stealPullingTime);
                    stealingBirdArmTransform.position = Vector3.Lerp(stealingReachingTarget.position, stealingArmStartingPosition, timeRatio);
                    stolenBirdbuckTransform.position = stealingHandTransform.position;
                    break;
                case State.delay:
                    if (timeActive > delayTime)
                    {
                        spawnPosition = birdBucksRemainingImageTransform.position;
                        if(birdBucksRemaining == 0)
                        {
                            currentState = State.inactive;
                        }
                        else
                        {
                            currentState = State.distribute;
                        }
                        
                    }
                    break;
                case State.distribute:
                    timeSinceLastDistribution += Time.deltaTime * GameManager.Instance.playerFlowManager.slidesRound.slideSpeed;
                    if(timeSinceLastDistribution >= distributionFrequency)
                    {
                        timeSinceLastDistribution = 0f;
                        foreach (SummarySlideSection section in sections)
                        {
                            GameObject distributedBirdBuckObject = Instantiate(distributedBirdbuckPrefab, spawnPosition, Quaternion.identity, transform);
                            SlideDistributedBirdbuck distributedBirdBuck = distributedBirdBuckObject.GetComponent<SlideDistributedBirdbuck>();
                            distributedBirdBuck.Initialize(section);
                            birdBucksRemaining -= 1;
                            birdBucksRemainingText.text = birdBucksRemaining.ToString();

                            //Reduce the size
                            timeRatio = (timeActive - delayTime) / (distributionTime);
                            birdBucksRemainingImageTransform.localScale = Vector3.one * (1 - timeRatio);

                            AudioManager.Instance.PlaySound(distributeSFX, true);
                            timeSinceLastDistribution = 0f;

                            if (birdBucksRemaining == 0)
                            {
                                currentState = State.inactive;
                                birdBucksRemainingImageTransform.localScale = Vector3.zero;
                                break;
                            }
                        }
                    }
                    break;
                case State.inactive:
                    break;
            }
        }
    }

}
