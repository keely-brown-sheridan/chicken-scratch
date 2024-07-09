using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstructionsScoreVisualization : MonoBehaviour
{
    [SerializeField]
    private Transform birdBucksTrackerTransform;

    [SerializeField]
    private float scalingDuration;

    private float timeActive = 0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (timeActive >= scalingDuration)
        {
            return;
        }
        timeActive += Time.deltaTime;

        float timeRatio = Mathf.Clamp(timeActive / scalingDuration,0.0f,1.0f);

        birdBucksTrackerTransform.localScale = new Vector3(timeRatio, birdBucksTrackerTransform.localScale.y, birdBucksTrackerTransform.localScale.z);

        

    }
}
