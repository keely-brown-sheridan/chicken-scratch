using ChickenScratch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CaseModifierVisual : MonoBehaviour
{
    [SerializeField]
    private Image scoreFillImage;

    [SerializeField]
    private Image modifierImage;

    [SerializeField]
    private TMPro.TMP_Text modifierText;

    [SerializeField]
    private string thresholdCrossSFXName;
    [SerializeField]
    private string resetTimeSFXName;
    [SerializeField]
    private GameObject stopWatchObject;

    [SerializeField]
    private CertificationEffectIndicator ballparkVisualIndicator;

    private float timeForTask;
    private float timeRemaining;
    private float maxModifierValue;
    private float startingModifierValue;
    private float currentModifierValue;
    private float modifierDecrement;

    public bool hasCrossedThreshold => _hasCrossedThreshold;
    private bool _hasCrossedThreshold = false;

    public UnityEvent onTimeComplete;

    private bool ballparked = false;
    private int caseID;

    public void Initialize(int inCaseID, float inTimeForTask, float inCurrentModifierValue, float inMaxModifierValue, float inModifierDecrement, bool inBallparked = false)
    {
        caseID = inCaseID;
        ballparked = inBallparked;
        timeForTask = inTimeForTask;
        timeRemaining = inTimeForTask;
        startingModifierValue = inCurrentModifierValue;
        currentModifierValue = inCurrentModifierValue;
        modifierDecrement = inModifierDecrement;

        if(ballparked)
        {
            modifierText.text = "??";
        }
        else
        {
            modifierText.text = currentModifierValue.ToString() + "x";
        }
        
        maxModifierValue = inMaxModifierValue;
        Color currentModifierColour = SettingsManager.Instance.GetModifierColour(currentModifierValue / maxModifierValue);

        if(ballparked)
        {
            CertificationData ballparkCertification = GameDataManager.Instance.GetCertification("Ballpark");
            if(ballparkCertification != null)
            {
                ballparkVisualIndicator.Show(ballparkCertification, "Reward and Modifier are hidden");
            }
            
            modifierImage.color = Color.grey;
            scoreFillImage.color = Color.grey;
        }
        else
        {
            modifierImage.color = currentModifierColour;
            scoreFillImage.color = currentModifierColour;
        }
        
        scoreFillImage.transform.localScale = Vector3.one;
        gameObject.SetActive(true);
        _hasCrossedThreshold = false;
        if (GameManager.Instance.playerFlowManager.HasStoreItem(StoreItem.StoreItemType.stopwatch) &&
            GameManager.Instance.playerFlowManager.StoreItemHasCharges(StoreItem.StoreItemType.stopwatch))
        {
            stopWatchObject.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        timeRemaining -= Time.deltaTime;
        float ratio = timeRemaining / timeForTask;
        
        if(ratio < 0)
        {
            currentModifierValue -= modifierDecrement;
            gameObject.SetActive(false);
            onTimeComplete.Invoke();
        }
        else if (ratio < 0.5f && !hasCrossedThreshold)
        {
            StatTracker.Instance.hasLostModifier = true;
            AudioManager.Instance.PlaySound(thresholdCrossSFXName);
            currentModifierValue -= modifierDecrement;
            Color currentModifierColour = SettingsManager.Instance.GetModifierColour(currentModifierValue / maxModifierValue);
            
            if (!ballparked)
            {
                modifierText.text = currentModifierValue.ToString("F2") + "x";
                modifierImage.color = currentModifierColour;
                scoreFillImage.color = currentModifierColour;
            }

            _hasCrossedThreshold = true;
            
        }
        scoreFillImage.transform.localScale = new Vector3(1, ratio, 1);
    }

    public float GetFinalModifierValue()
    {
        return currentModifierValue;
    }

    public void ResetTime()
    {
        stopWatchObject.SetActive(false);
        AudioManager.Instance.PlaySound(resetTimeSFXName);
        currentModifierValue = startingModifierValue;
        timeRemaining = timeForTask;
        Color currentModifierColour = SettingsManager.Instance.GetModifierColour(currentModifierValue / maxModifierValue);

        if(!ballparked)
        {
            modifierImage.color = currentModifierColour;
            scoreFillImage.color = currentModifierColour;
            modifierText.text = currentModifierValue.ToString("F2") + "x";
        }

        _hasCrossedThreshold = false;
        GameManager.Instance.playerFlowManager.UseChargedItem(StoreItem.StoreItemType.stopwatch);
    }
}
