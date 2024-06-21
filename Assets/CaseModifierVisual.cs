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

    private float timeForTask;
    private float timeRemaining;
    private float maxModifierValue;
    private float startingModifierValue;
    private float modifierDecrement;

    private bool hasCrossedThreshold = false;

    public UnityEvent onTimeComplete;

    public void Initialize(float inTimeForTask, float currentModifierValue, float inMaxModifierValue, float inModifierDecrement)
    {
        timeForTask = inTimeForTask;
        timeRemaining = inTimeForTask;
        startingModifierValue = currentModifierValue;
        modifierDecrement = inModifierDecrement;

        modifierText.text = currentModifierValue.ToString() + "x";
        maxModifierValue = inMaxModifierValue;
        Color currentModifierColour = SettingsManager.Instance.GetModifierColour(currentModifierValue / maxModifierValue);

        modifierImage.color = currentModifierColour;
        scoreFillImage.color = currentModifierColour;
        scoreFillImage.transform.localScale = Vector3.one;
        gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        timeRemaining -= Time.deltaTime;
        float ratio = timeRemaining / timeForTask;
        
        if(ratio < 0)
        {
            Debug.LogError("Out of time.");
            startingModifierValue -= modifierDecrement;
            gameObject.SetActive(false);
            onTimeComplete.Invoke();
            
        }
        else if (ratio < 0.5f && !hasCrossedThreshold)
        {
            AudioManager.Instance.PlaySound(thresholdCrossSFXName);
            startingModifierValue -= modifierDecrement;
            modifierText.text = startingModifierValue.ToString() + "x";
            hasCrossedThreshold = true;
            Color currentModifierColour = SettingsManager.Instance.GetModifierColour(startingModifierValue / maxModifierValue);
            modifierImage.color = currentModifierColour;
            scoreFillImage.color = currentModifierColour;
        }
        scoreFillImage.transform.localScale = new Vector3(1, ratio, 1);
    }

    public float GetFinalModifierValue()
    {
        return startingModifierValue;
    }
}
