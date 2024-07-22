using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayInstructions : MonoBehaviour
{
    [SerializeField]
    private TMPro.TMP_Text dayText;

    [SerializeField]
    private TMPro.TMP_Text goalText;

    [SerializeField]
    private float timeFadingIn, timeShowing, timeFadingOut;

    private float timeInState = 0f;

    private enum State
    {
        faded, fading_in, showing, fading_out
    }

    private State currentState = State.faded;

    public void Show(string currentDay, int currentGoal)
    {
        dayText.text = currentDay;
        goalText.text = "Earn " + currentGoal.ToString() + " Birdbucks";
        currentState = State.fading_in;
    }

    // Update is called once per frame
    void Update()
    {
        float timeRatio;
        switch(currentState)
        {
            case State.fading_in:
                timeInState+= Time.deltaTime;
                if (timeInState > timeFadingIn)
                {
                    dayText.color = new Color(dayText.color.r, dayText.color.g, dayText.color.b, 1f);
                    goalText.color = new Color(dayText.color.r, dayText.color.g, dayText.color.b, 1f);
                    currentState = State.showing;
                    timeInState = 0f;
                    return;
                }
                timeRatio = timeInState / timeFadingIn;
                dayText.color = new Color(dayText.color.r, dayText.color.g, dayText.color.b, timeRatio);
                goalText.color = new Color(dayText.color.r, dayText.color.g, dayText.color.b, timeRatio);
                break;
            case State.showing:
                timeInState+= Time.deltaTime;
                if (timeInState > timeShowing)
                {
                    currentState = State.fading_out;
                    timeInState = 0f;
                    return;
                }
                break;
            case State.fading_out:
                timeInState += Time.deltaTime;
                if (timeInState > timeFadingOut)
                {
                    dayText.color = new Color(dayText.color.r, dayText.color.g, dayText.color.b, 0f);
                    goalText.color = new Color(dayText.color.r, dayText.color.g, dayText.color.b, 0f);
                    currentState = State.faded;
                    timeInState = 0f;
                    return;
                }
                timeRatio = timeInState / timeFadingOut;

                dayText.color = new Color(dayText.color.r, dayText.color.g, dayText.color.b, 1-timeRatio);
                goalText.color = new Color(dayText.color.r, dayText.color.g, dayText.color.b, 1-timeRatio);

                break;
        }
    }
}
