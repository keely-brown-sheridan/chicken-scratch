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
    private TMPro.TMP_Text goalScoreText;

    [SerializeField]
    private GameObject scoreModifierObject;

    [SerializeField]
    private Image scoreModifierImage;

    [SerializeField]
    private TMPro.TMP_Text scoreModifierText;

    [SerializeField]
    private Transform modifierScoreStartingDock, modifierScoreEndingDock;
    [SerializeField]
    private GameObject birdBuckPrefabObject;

    [SerializeField]
    private Transform birdBuckHolderParent;


    private float timeToReachTime;
    private float maximumPossibleScore;
    private float startingScore;
    private float targetScore;

    private float currentTimeReachingTarget;
    private float scoreModifier;
    private float currentScore;

    private int lastScore;

    public void Initialize(float inScoreModifier, float maxScoreModifier)
    {
        maximumPossibleScore = GameManager.Instance.playerFlowManager.GetCurrentGoal();
        goalScoreText.text = "Goal:\n" + maximumPossibleScore.ToString() + " Birdbucks";
        startingScore = GameManager.Instance.playerFlowManager.slidesRound.currentBirdBuckTotal;
        currentScore = startingScore;
        lastScore = (int)startingScore;
        currentScoreText.text = "Current Total:\n" + ((int)currentScore).ToString() + " Birdbucks";

        scoreModifier = inScoreModifier;
        scoreModifierText.text = inScoreModifier.ToString("F2") + "x";

        scoreModifierImage.color = SettingsManager.Instance.GetModifierColour(inScoreModifier / maxScoreModifier);

        float scoreRatio = Mathf.Clamp(currentScore / maximumPossibleScore, 0f, 1f);
        progressBarImageTransform.localScale = new Vector3(scoreRatio, 1f, 1f);
    }

    public void SetTarget(float inTargetScore, float inTimeToReach)
    {
        if(targetScore != 0)
        {
            //Set the bar to instantly jump to the target
            float previousTargetRatio = targetScore / maximumPossibleScore;
            progressBarImageTransform.localScale = new Vector3(previousTargetRatio, 1f, 1f);
            currentScore = targetScore;
            GameManager.Instance.playerFlowManager.slidesRound.currentBirdBuckTotal = (int)currentScore;
            lastScore = (int)targetScore;
        }
        currentTimeReachingTarget = Time.deltaTime;
        startingScore = currentScore;
        targetScore = inTargetScore;
        timeToReachTime = inTimeToReach;
    }

    public void ShowScoreModifier()
    {
        if(scoreModifier > 1f && GameManager.Instance.playerFlowManager.slidesRound.currentBirdBuckTotal != 0)
        {
            AudioManager.Instance.PlaySound("TimeBonus");
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
            if(lastScore < (int)currentScore)
            {
                //Spawn bird buck
                Instantiate(birdBuckPrefabObject, scoreModifierObject.transform.position, Quaternion.identity, birdBuckHolderParent);
                lastScore = (int)currentScore;
                GameManager.Instance.playerFlowManager.slidesRound.currentBirdBuckTotal = (int)currentScore;
            }

            float scoreRatio = Mathf.Clamp(currentScore / maximumPossibleScore, 0f, 1f);
            progressBarImageTransform.localScale = new Vector3(scoreRatio, 1f, 1f);

            currentScoreText.text = "Current Total:\n" + ((int)currentScore).ToString() + " Birdbucks";

            scoreModifierObject.transform.position = Vector3.Lerp(modifierScoreStartingDock.position, modifierScoreEndingDock.position, scoreRatio);

            if (finished)
            {
                currentTimeReachingTarget = 0f;
            }

        }
    }
}
