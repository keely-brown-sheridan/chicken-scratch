using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordManagerWarningMessage : MonoBehaviour
{

    [SerializeField]
    private TMPro.TMP_Text warningMessageText;

    [SerializeField]
    private float showDuration = 2.0f;

    private float timeShowing = 0f;
    private Color startingColour;

    public void ShowMessage(string message, Color messageColour)
    {
        gameObject.SetActive(true);
        startingColour = messageColour;
        timeShowing = Time.deltaTime;
        warningMessageText.text = message;
        warningMessageText.color = messageColour;
    }

    // Update is called once per frame
    void Update()
    {
        if(timeShowing > 0.0f)
        {
            float timeRatio = timeShowing / showDuration;
            warningMessageText.color = new Color(startingColour.r, startingColour.g, startingColour.b, 1-timeRatio);
            timeShowing += Time.deltaTime;
            
            if(timeShowing > showDuration)
            {
                timeShowing = 0.0f;
                gameObject.SetActive(false);
            }
        }
    }
}
