using ChickenScratch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialProgressIndicator : MonoBehaviour
{
    [SerializeField]
    private BirdImage birdImage;

    [SerializeField]
    private float timeToReachHook;

    public int currentHookIndex => _currentHookIndex;

    private int _currentHookIndex = 0;
    private enum State
    {
        idle, moving
    }
    private State currentState = State.idle;

    private float timeTravelingToHook;
    private Vector3 startingPosition;
    private Vector3 hookPosition;

    public void Initialize(ColourManager.BirdName birdName, Vector3 startingHookPosition)
    {
        _currentHookIndex = 1;
        currentState = State.idle;
        BirdHatData.HatType birdHat = GameManager.Instance.playerFlowManager.GetBirdHatType(birdName);
        birdImage.Initialize(birdName, birdHat);
        transform.position = startingHookPosition;
    }

    public void SetTargetHook(Vector3 targetHookPosition)
    {
        timeTravelingToHook = 0f;
        startingPosition = transform.position;
        hookPosition = targetHookPosition;
        currentState = State.moving;
        _currentHookIndex++;
    }

    // Update is called once per frame
    void Update()
    {
        if(currentState == State.moving)
        {
            timeTravelingToHook += Time.deltaTime;
            float timeRatio = timeTravelingToHook / timeToReachHook;
            transform.position = Vector3.Lerp(startingPosition, hookPosition, timeRatio);
            if(timeRatio > 1)
            {
                currentState = State.idle;
            }
        }
    }
}
