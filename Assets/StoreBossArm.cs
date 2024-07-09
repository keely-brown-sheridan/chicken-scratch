using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StoreBossArm : MonoBehaviour
{
    [SerializeField]
    private Transform handTransform;

    [SerializeField]
    private float retractingTime;

    private enum State
    {
        idle, reaching, returning
    }
    private State currentState = State.idle;

    private float reachingTime;
    private float timeInState = 0f;
    private Vector3 startPosition;
    private Vector3 targetPosition;

    public void StartReach(Vector3 inTargetPosition, float inReachingTime)
    {
        startPosition = handTransform.position;
        targetPosition = inTargetPosition;
        reachingTime = inReachingTime;
        currentState = State.reaching;
        timeInState = 0f;
    }

    public void CancelReach()
    {
        float totalDistance = Vector3.Distance(startPosition, targetPosition);
        float currentDistance = Vector3.Distance(startPosition, handTransform.position);
        timeInState = currentDistance / totalDistance * retractingTime;
        currentState = State.returning;
    }

    // Update is called once per frame
    void Update()
    {
        float timeRatio;
        switch(currentState)
        {
            case State.reaching:
                timeInState += Time.deltaTime;
                timeRatio = timeInState / reachingTime;
                if(timeRatio > 1)
                {
                    handTransform.position = targetPosition;
                    timeInState = 0f;
                    currentState = State.returning;
                    return;
                }
                handTransform.position = Vector3.Lerp(startPosition, targetPosition, timeRatio);
                break;
            case State.returning:
                timeInState += Time.deltaTime;
                timeRatio = timeInState / retractingTime;
                if(timeRatio > 1)
                {
                    handTransform.position = startPosition;
                    timeInState = 0f;
                    currentState = State.idle;
                    return;
                }
                handTransform.position = Vector3.Lerp(targetPosition, startPosition, timeRatio);
                break;
        }
    }
}
