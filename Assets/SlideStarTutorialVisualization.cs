using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideStarTutorialVisualization : MonoBehaviour
{
    private enum State
    {
        initializing, reaching, assigning, departing, complete
    }
    private State currentState = State.initializing;

    [SerializeField]
    private float reachingTime;

    [SerializeField]
    private float assigningTime;

    [SerializeField]
    private float departingTime;

    [SerializeField]
    private Transform birdArmTransform;

    [SerializeField]
    private Transform starTransform;

    [SerializeField]
    private Transform guessStarHookTransform;


    private Vector3 startingPosition;
    private Vector3 starPosition;
    private float currentTimeInState = 0f;

    // Start is called before the first frame update
    void Start()
    {
        Invoke("Initialize", 1.0f);
    }

    private void Initialize()
    {
        startingPosition = birdArmTransform.position;
        starPosition = starTransform.position;
        currentState = State.reaching;
    }

    // Update is called once per frame
    void Update()
    {
        float timeRatio;
        switch(currentState)
        {
            case State.reaching:
                currentTimeInState += Time.deltaTime;
                timeRatio = currentTimeInState / reachingTime;
                birdArmTransform.position = Vector3.Lerp(startingPosition, starPosition, timeRatio);
                if(currentTimeInState > reachingTime)
                {
                    starTransform.parent = birdArmTransform;
                    currentTimeInState = 0.0f;
                    currentState = State.assigning;
                }
                break;
            case State.assigning:
                currentTimeInState += Time.deltaTime;
                timeRatio = currentTimeInState / assigningTime;
                birdArmTransform.position = Vector3.Lerp(starPosition, guessStarHookTransform.position, timeRatio);
                if (currentTimeInState > assigningTime)
                {
                    starTransform.parent = guessStarHookTransform;
                    currentTimeInState = 0.0f;
                    currentState = State.departing;
                }
                break;
            case State.departing:
                currentTimeInState += Time.deltaTime;
                timeRatio = currentTimeInState / departingTime;
                birdArmTransform.position = Vector3.Lerp(guessStarHookTransform.position, startingPosition, timeRatio);
                if (currentTimeInState > departingTime)
                {
                    currentTimeInState = 0.0f;
                    currentState = State.complete;
                }
                
                break;
        }
    }
}
