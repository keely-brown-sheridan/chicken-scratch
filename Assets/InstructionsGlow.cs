using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstructionsGlow : MonoBehaviour
{
    [SerializeField]
    private GameObject glowObject;

    [SerializeField]
    private float glowShowDuration;

    [SerializeField]
    private float glowHideDuration;

    private enum State
    {
        show, hide
    }
    private State currentState = State.show;

    private float timeInState = 0f;

    // Update is called once per frame
    void Update()
    {
        timeInState += Time.deltaTime;
        switch(currentState)
        {
            case State.show:
                if(timeInState > glowShowDuration)
                {
                    timeInState = 0f;
                    currentState = State.hide;
                    glowObject.SetActive(false);
                }
                break;
            case State.hide:
                if(timeInState > glowHideDuration)
                {
                    timeInState = 0f;
                    currentState = State.show;
                    glowObject.SetActive(true);
                }
                break;
        }
    }
}
