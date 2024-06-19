using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideTimeModifierDecrementVisual : MonoBehaviour
{
    [SerializeField]
    private float totalExpandingTime;

    [SerializeField]
    private TMPro.TMP_Text timeModifierDecrementText;

    [SerializeField]
    private float finalScale = 1f;

    private float timeExpanding = 0f;

    public void Initialize(float timeModifierDecrement)
    {
        if(timeModifierDecrement != 0)
        {
            timeModifierDecrementText.text = timeModifierDecrement.ToString() + " Score Modifier";
            gameObject.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(timeExpanding > totalExpandingTime)
        {
            enabled = false;
        }
        timeExpanding += Time.deltaTime;
        
        float timeRatio = finalScale * timeExpanding / totalExpandingTime;
        transform.localScale = Vector3.one * timeRatio;
    }
}
