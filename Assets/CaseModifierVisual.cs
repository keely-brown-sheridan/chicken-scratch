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

    private float timeForTask;
    private float timeRemaining;
    private float maxModifierValue;
    private float startingModifierValue;
    private float currentModifierValue;
    private float modifierDecrement;

    private bool hasCrossedThreshold = false;

    public UnityEvent onTimeComplete;

    public void Initialize(float inTimeForTask, float inCurrentModifierValue, float inMaxModifierValue, float inModifierDecrement)
    {
        timeForTask = inTimeForTask;
        timeRemaining = inTimeForTask;
        startingModifierValue = inCurrentModifierValue;
        currentModifierValue = inCurrentModifierValue;
        modifierDecrement = inModifierDecrement;

        modifierText.text = currentModifierValue.ToString() + "x";
        maxModifierValue = inMaxModifierValue;
        Color currentModifierColour = SettingsManager.Instance.GetModifierColour(currentModifierValue / maxModifierValue);

        modifierImage.color = currentModifierColour;
        scoreFillImage.color = currentModifierColour;
        scoreFillImage.transform.localScale = Vector3.one;
        gameObject.SetActive(true);

        if(GameManager.Instance.playerFlowManager.HasStoreItem(StoreItem.StoreItemType.stopwatch) &&
            !GameManager.Instance.playerFlowManager.StoreItemHasCharges(StoreItem.StoreItemType.stopwatch))
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
            AudioManager.Instance.PlaySound(thresholdCrossSFXName);
            currentModifierValue -= modifierDecrement;
            modifierText.text = currentModifierValue.ToString() + "x";
            hasCrossedThreshold = true;
            Color currentModifierColour = SettingsManager.Instance.GetModifierColour(currentModifierValue / maxModifierValue);
            modifierImage.color = currentModifierColour;
            scoreFillImage.color = currentModifierColour;
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
        modifierImage.color = currentModifierColour;
        scoreFillImage.color = currentModifierColour;
        hasCrossedThreshold = false;
        modifierText.text = currentModifierValue.ToString() + "x";

        GameManager.Instance.playerFlowManager.UseChargedItem(StoreItem.StoreItemType.stopwatch);
    }
}
