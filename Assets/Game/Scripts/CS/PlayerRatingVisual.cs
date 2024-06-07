using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ChickenScratch.ColourManager;

namespace ChickenScratch
{
    public class PlayerRatingVisual : MonoBehaviour
    {
        private enum State
        {
            rising, lowering, invalid
        }
        private State currentState = State.rising;

        [SerializeField]
        private float risingDuration;
        [SerializeField]
        private float loweringDuration;
        private bool isInitialized = false;
        private float timeRising = 0.0f;
        private float timeLowering = 0.0f;
        private Vector3 startingPosition;
        private Vector3 endingPosition;
        private float maxHeight;
        private PeanutBird targetBird;


        // Update is called once per frame
        void Update()
        {
            float progressRatio;
            Vector3 updatedPosition;
            float currentHeight;
            if (isInitialized)
            {
                switch (currentState)
                {
                    case State.rising:
                        timeRising += Time.deltaTime;
                        progressRatio = timeRising / risingDuration / 2;
                        updatedPosition = startingPosition + (endingPosition - startingPosition) * progressRatio;
                        currentHeight = startingPosition.y + (maxHeight - startingPosition.y) * progressRatio;

                        //Increase the scale 
                        transform.localScale = new Vector3(0.4f + 0.6f * progressRatio, 0.4f + 0.6f * progressRatio, 1.0f);
                        transform.position = new Vector3(updatedPosition.x, currentHeight, updatedPosition.z);

                        if (timeRising > risingDuration)
                        {
                            currentState = State.lowering;
                        }
                        break;
                    case State.lowering:
                        timeLowering += Time.deltaTime;
                        progressRatio = 0.5f + timeLowering / loweringDuration / 2;
                        updatedPosition = startingPosition + (endingPosition - startingPosition) * progressRatio;
                        currentHeight = maxHeight + (startingPosition.y - maxHeight) * progressRatio;

                        //Decrease the scale
                        transform.localScale = new Vector3(0.4f + 0.6f * (1 - progressRatio), 0.4f + 0.6f * (1 - progressRatio), 1.0f);
                        transform.position = new Vector3(updatedPosition.x, currentHeight, updatedPosition.z);

                        if (timeLowering > loweringDuration)
                        {
                            targetBird.AddLike();
                            Destroy(gameObject);
                        }
                        break;
                }
            }
        }

        public void initialize(Vector3 inStartingPosition, Vector3 inEndingPosition, float inMaxHeight, PeanutBird inTargetBird)
        {
            startingPosition = inStartingPosition;
            endingPosition = inEndingPosition;
            maxHeight = inMaxHeight;
            targetBird = inTargetBird;
            isInitialized = true;
        }
    }
}