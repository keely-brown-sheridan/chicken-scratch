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
    private Color fullColour, halfColour;

    [SerializeField]
    private string thresholdCrossSFXName;

    private float timeForTask;
    private float timeRemaining;
    private float startingModifierValue;
    private float modifierDecrement;

    private bool hasCrossedThreshold = false;

    public UnityEvent onTimeComplete;

    public void Initialize(float inTimeForTask, float currentModifierValue, float inModifierDecrement)
    {
        timeForTask = inTimeForTask;
        timeRemaining = inTimeForTask;
        startingModifierValue = currentModifierValue;
        modifierDecrement = inModifierDecrement;

        modifierText.text = currentModifierValue.ToString() + "x";
        modifierImage.color = fullColour;
        scoreFillImage.color = fullColour;
        scoreFillImage.transform.localScale = Vector3.one;
    }

    // Update is called once per frame
    void Update()
    {
        timeRemaining -= Time.deltaTime;
        float ratio = timeRemaining / timeForTask;
        
        if(ratio < 0)
        {
            gameObject.SetActive(false);
            onTimeComplete.Invoke();
            startingModifierValue -= modifierDecrement;
        }
        else if (ratio < 0.5f && !hasCrossedThreshold)
        {
            modifierImage.color = halfColour;
            scoreFillImage.color = halfColour;
            AudioManager.Instance.PlaySound(thresholdCrossSFXName);
            startingModifierValue -= modifierDecrement;
            modifierText.text = startingModifierValue.ToString() + "x";
            hasCrossedThreshold = true;
        }
        scoreFillImage.transform.localScale = new Vector3(1, ratio, 1);
    }

    public float GetFinalModifierValue()
    {
        return startingModifierValue;
    }
}
