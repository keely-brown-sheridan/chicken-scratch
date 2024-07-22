using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawingBlocker : MonoBehaviour
{
    [SerializeField]
    private Vector3 targetDimensions;

    [SerializeField]
    private Vector3 startingDimensions;

    [SerializeField]
    private Vector3 targetPosition;

    [SerializeField]
    private Vector3 startingPosition;

    private float adjustmentStartTime = 0f;
    private float adjustmentEndTime = 0f;

    private float timeActive = 0f;


    public void Initialize(float inStartTime, float inEndTime, Vector3 inStartingDimensions, Vector3 inEndingDimensions, Vector3 inStartingPosition, Vector3 inTargetPosition)
    {
        adjustmentStartTime = inStartTime;
        adjustmentEndTime = inEndTime;
        startingDimensions = inStartingDimensions;
        targetDimensions = inEndingDimensions;
        timeActive = 0f;
        startingPosition = inStartingPosition;
        targetPosition = inTargetPosition;
        transform.position = startingPosition;
        transform.localScale = startingDimensions;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(timeActive > adjustmentEndTime)
        {
            return;
        }
        timeActive += Time.deltaTime;
        if(timeActive > adjustmentStartTime)
        {
            float timeRatio = (timeActive - adjustmentStartTime) / (adjustmentEndTime - adjustmentStartTime);
            transform.localPosition = Vector3.Lerp(startingPosition, targetPosition, timeRatio);
            transform.localScale = Vector3.Lerp(startingDimensions, targetDimensions, timeRatio);
        }
        
    }
}
