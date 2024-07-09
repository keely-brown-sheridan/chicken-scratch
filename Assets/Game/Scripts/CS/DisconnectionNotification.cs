using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ChickenScratch.ColourManager;

namespace ChickenScratch
{
    public class DisconnectionNotification : MonoBehaviour
    {
        [SerializeField] private TMPro.TMP_Text disconnectionText;
        [SerializeField] private BirdImage disconnectedBirdImage;
        [SerializeField] private float showingDuration;
        [SerializeField] private Transform slidingTargetTransform;
        [SerializeField] private float slidingDuration;


        private float timeShowing = 0.0f;
        private float timeSliding = 0.0f;
        private float timeReturning = 0.0f;

        private enum State
        {
            idle, sliding, showing, returning
        }
        private State currentState = State.idle;

        private List<BirdName> disconnectedBirds = new List<BirdName>();
        private Dictionary<BirdName, string> disconnectedBirdMap = new Dictionary<BirdName, string>();
        private Vector3 startingPosition = Vector3.zero;

        void Start()
        {
            startingPosition = transform.position;
        }


        public void QueueDisconnection(BirdName disconnectedBird, string disconnectedName)
        {
            disconnectedBirds.Add(disconnectedBird);
            disconnectedBirdMap.Add(disconnectedBird, disconnectedName);

            if (currentState == State.idle)
            {
                //Show the next bird
                ShowNextBird();
                currentState = State.sliding;
                timeSliding = Time.deltaTime;
            }
        }


        // Update is called once per frame
        void Update()
        {
            switch (currentState)
            {
                case State.idle:
                    break;
                case State.returning:
                    timeReturning += Time.deltaTime;
                    transform.position = new Vector3(startingPosition.x + ((slidingTargetTransform.position.x - startingPosition.x) * (1 - timeReturning / slidingDuration)), startingPosition.y, startingPosition.z);

                    if (timeReturning > slidingDuration)
                    {
                        timeReturning = 0.0f;
                        if (disconnectedBirds.Count > 0)
                        {
                            currentState = State.sliding;
                        }
                        else
                        {
                            currentState = State.idle;
                        }


                    }
                    break;
                case State.showing:
                    timeShowing += Time.deltaTime;

                    if (timeShowing > showingDuration)
                    {
                        timeShowing = 0.0f;
                        if (disconnectedBirds.Count > 0)
                        {
                            //Show the next bird
                            ShowNextBird();
                        }
                        else
                        {
                            currentState = State.returning;
                        }
                    }
                    break;
                case State.sliding:
                    timeSliding += Time.deltaTime;
                    transform.position = new Vector3(startingPosition.x + ((slidingTargetTransform.position.x - startingPosition.x) * timeSliding / slidingDuration), startingPosition.y, startingPosition.z);

                    if (timeSliding > slidingDuration)
                    {
                        currentState = State.showing;
                        timeSliding = 0.0f;
                    }
                    break;
            }
        }

        private void ShowNextBird()
        {
            BirdName nextBirdName = disconnectedBirds[0];
            string nextPlayerName = disconnectedBirdMap[nextBirdName];

            disconnectedBirds.RemoveAt(0);
            disconnectedBirdMap.Remove(nextBirdName);

            disconnectionText.text = nextPlayerName + "\nhas been disconnected.";
            BirdData nextBird = GameDataManager.Instance.GetBird(nextBirdName);
            if(nextBird == null)
            {
                Debug.LogError("Could not show next bird["+nextBirdName.ToString()+"] because it is not mapped in the ColourManager.");
                return;
            }
            disconnectionText.color = nextBird.colour;

            BirdHatData.HatType birdHat = GameManager.Instance.playerFlowManager.GetBirdHatType(nextBirdName);
            disconnectedBirdImage.Initialize(nextBirdName, birdHat);
        }
    }
}