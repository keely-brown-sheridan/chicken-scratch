using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ChickenScratch.ColourManager;

namespace ChickenScratch
{
    public class LobbyBirdArm : MonoBehaviour
    {
        [SerializeField]
        private Transform playerIdentificationTransform;

        [SerializeField]
        private Transform restPositionTransform;

        [SerializeField]
        private float translationSpeed;

        public BirdName birdName => _birdName;
        [SerializeField]
        private BirdName _birdName;

        public Transform fingerTransform => _fingerTransform;
        [SerializeField]
        private Transform _fingerTransform;

        [SerializeField]
        private Transform cardVisualsTransform;

        [SerializeField]
        private float restDistanceThreshold;

        [SerializeField]
        private float deliveryThreshold;

        [SerializeField]
        private float returnDelay;
        [SerializeField]
        private string grabSound;
        private float timeWaitingToReturn = 0.0f;

        public enum State
        {
            rest, slide_right, hover, slide_left, grab_card, approach_card, return_card, holding, invalid
        }
        public State currentState = State.rest;

        public bool returnRequested = false;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            float yPosition;
            switch (currentState)
            {
                case State.rest:
                    //Match the height of the card
                    yPosition = playerIdentificationTransform.position.y - fingerTransform.position.y + transform.position.y;
                    transform.position = new Vector3(restPositionTransform.position.x, yPosition, transform.position.z);
                    break;
                case State.holding:
                    //Match the height of the card
                    yPosition = playerIdentificationTransform.position.y - fingerTransform.position.y + transform.position.y;
                    transform.position = new Vector3(restPositionTransform.position.x, yPosition, transform.position.z);
                    if (returnRequested)
                    {
                        timeWaitingToReturn += Time.deltaTime;
                        if (timeWaitingToReturn > returnDelay)
                        {
                            timeWaitingToReturn = 0.0f;
                            currentState = State.return_card;
                        }

                    }
                    break;
                case State.slide_right:
                    //Move towards the card if it's being hovered over
                    transform.position = Vector3.MoveTowards(transform.position, playerIdentificationTransform.position + transform.position - fingerTransform.position, translationSpeed * Time.deltaTime);
                    break;
                case State.slide_left:
                    //Move back towards the rest position if it's no longer being hovered over
                    transform.position = Vector3.MoveTowards(transform.position, new Vector3(restPositionTransform.position.x, transform.position.y, transform.position.z), translationSpeed * Time.deltaTime);

                    //If we're close enough to the rest position then switch to rest
                    if (transform.position.x - restPositionTransform.position.x < restDistanceThreshold)
                    {
                        currentState = State.rest;
                    }
                    break;
                case State.grab_card:
                    //move it towards the resting position
                    transform.position = Vector3.MoveTowards(transform.position, new Vector3(restPositionTransform.position.x, transform.position.y, transform.position.z), translationSpeed * Time.deltaTime);

                    //If we're close enough to the rest position then switch to rest
                    if (transform.position.x - restPositionTransform.position.x < restDistanceThreshold)
                    {
                        currentState = State.holding;
                    }
                    break;
                case State.approach_card:
                    transform.position = Vector3.MoveTowards(transform.position, playerIdentificationTransform.position + transform.position - fingerTransform.position, translationSpeed * Time.deltaTime);
                    if (Vector3.Distance(fingerTransform.position, playerIdentificationTransform.position) < deliveryThreshold)
                    {
                        AudioManager.Instance.PlaySound(grabSound);
                        //transfer the visuals of the card to the bird arm
                        cardVisualsTransform.parent = fingerTransform;
                        currentState = State.grab_card;
                    }
                    break;
                case State.return_card:

                    //Move the card back to its resting position
                    transform.position = Vector3.MoveTowards(transform.position, playerIdentificationTransform.position + transform.position - fingerTransform.position, translationSpeed * Time.deltaTime);

                    //If the resting position has been reached then transfer the visuals back to the player identification transform
                    if (Vector3.Distance(fingerTransform.position, playerIdentificationTransform.position) < deliveryThreshold)
                    {
                        returnRequested = false;
                        cardVisualsTransform.parent = playerIdentificationTransform;
                        currentState = State.slide_left;
                    }
                    break;
            }
        }
    }
}