using ChickenScratch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlideCaseScoreVisualization : MonoBehaviour
{
    [SerializeField]
    private Transform progressBarImageTransform;

    [SerializeField]
    private TMPro.TMP_Text currentScoreText;

    [SerializeField]
    private GameObject scoreModifierObject;

    [SerializeField]
    private TMPro.TMP_Text scoreModifierText;

    [SerializeField]
    private Transform modifierScoreStartingDock, modifierScoreEndingDock;


    private float timeToReachTime;
    private float maximumPossibleScore;
    private float startingScore;
    private float currentScore;
    private float targetScore;

    private float currentTimeReachingTarget;
    private float scoreModifier;

    public void Initialize(float inMaxScore, float inScoreModifier)
    {
        maximumPossibleScore = inMaxScore;
        startingScore = 0f;
        currentScore = 0f;
        scoreModifier = inScoreModifier;
        scoreModifierText.text = inScoreModifier.ToString() + "x";
    }

    public void SetTarget(float inTargetScore, float inTimeToReach)
    {
        if(targetScore != 0)
        {
            //Set the bar to instantly jump to the target
            float previousTargetRatio = targetScore / maximumPossibleScore;
            progressBarImageTransform.localScale = new Vector3(previousTargetRatio, 1f, 1f);
            currentScore = targetScore;
        }
        currentTimeReachingTarget = Time.deltaTime;
        startingScore = currentScore;
        targetScore = inTargetScore;
        timeToReachTime = inTimeToReach;
    }

    public void ShowScoreModifier()
    {
        if(scoreModifier > 1f)
        {
            AudioManager.Instance.PlaySound("increase-score");
        }
        scoreModifierObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        bool finished = false;
        if(currentTimeReachingTarget > 0)
        {
            currentTimeReachingTarget += Time.deltaTime * GameManager.Instance.playerFlowManager.slidesRound.slideSpeed;
            if(currentTimeReachingTarget > timeToReachTime)
            {
                currentTimeReachingTarget = timeToReachTime;
                finished = true;
            }
            float currentTimeRatio = currentTimeReachingTarget / timeToReachTime;
            currentScore = Mathf.Lerp(startingScore, targetScore, currentTimeRatio);

            float scoreRatio = currentScore / maximumPossibleScore;
            progressBarImageTransform.localScale = new Vector3(scoreRatio, 1f, 1f);

            currentScoreText.text = "Points Earned: " + ((int)currentScore).ToString();

            if(scoreModifierObject.activeSelf)
            {
                scoreModifierObject.transform.position = Vector3.Lerp(modifierScoreStartingDock.position, modifierScoreEndingDock.position, scoreRatio);
            }

            if (finished)
            {
                currentTimeReachingTarget = 0f;
            }

        }
    }
}
