using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class FinalEndgameResultManager : MonoBehaviour
    {
        public enum State
        {
            idle, rising, risen_rest, rotating_up, rotation_rest, happy, neutral, angry, preparing, invalid
        }

        public State currentState = State.idle;
        public State chosenReactionState = State.neutral;
        [SerializeField]
        private Transform risingHeightPositionTransform;

        [SerializeField]
        private Transform shoulderJointPositionTransform;

        [SerializeField]
        private Transform armTransform;

        [SerializeField]
        private float risingDuration;

        [SerializeField]
        private float rotationSpeed;

        [SerializeField]
        private float rotationDuration;
        [SerializeField]
        private float waitDuration;
        [SerializeField]
        private float preparationDuration;
        [SerializeField]
        private float reactionDuration;

        private float timeRotating = 0.0f;
        private float timeRising = 0.0f;
        private float timeWaiting = 0.0f;
        private float timePreparing = 0.0f;

        private float startingHeight;
        public string responseSoundClip;

        private Vector3 startingArmPosition;
        private Quaternion startingArmRotation;

        [SerializeField]
        private GameObject angryFaceObject;
        [SerializeField]
        private GameObject happyFaceObject;

        [SerializeField]
        private GameObject neutralFaceObject;

        // Start is called before the first frame update
        void Start()
        {
            startingArmPosition = armTransform.position;
            startingArmRotation = armTransform.rotation;
            startingHeight = transform.position.y;
            timeRotating = 0.0f;
        }

        // Update is called once per frame
        void Update()
        {
            switch (currentState)
            {
                case State.preparing:
                    timePreparing += Time.deltaTime;
                    if (timePreparing > preparationDuration)
                    {
                        AudioManager.Instance.PlaySound("sfx_game_env_boss_rise");
                        timePreparing = 0.0f;
                        currentState = State.rising;
                    }
                    break;
                case State.rising:
                    timeRising += Time.deltaTime;
                    float currentHeight = startingHeight + (risingHeightPositionTransform.position.y - startingHeight) * timeRising / risingDuration;
                    transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);
                    if (timeRising > risingDuration)
                    {
                        timeRising = 0.0f;
                        currentState = State.risen_rest;
                    }

                    break;
                case State.risen_rest:
                    timeWaiting += Time.deltaTime;
                    if (timeWaiting > waitDuration)
                    {
                        timeWaiting = 0.0f;
                        currentState = State.rotating_up;
                    }
                    break;
                case State.rotating_up:
                    timeRotating += Time.deltaTime;

                    if (timeRotating < rotationDuration)
                    {
                        armTransform.RotateAround(shoulderJointPositionTransform.position, Vector3.forward, rotationSpeed * Time.deltaTime);
                    }
                    else
                    {
                        timeRotating = 0.0f;
                        currentState = State.rotation_rest;
                    }
                    break;
                case State.rotation_rest:
                    timeWaiting += Time.deltaTime;
                    if (timeWaiting > waitDuration)
                    {
                        timeWaiting = 0.0f;
                        currentState = chosenReactionState;
                    }
                    break;
                case State.neutral:
                    timeWaiting += Time.deltaTime;
                    AudioManager.Instance.PlaySound(responseSoundClip);
                    if (timeWaiting > reactionDuration)
                    {
                        gameObject.SetActive(false);
                        timeWaiting = 0.0f;
                        currentState = State.idle;
                        if (SettingsManager.Instance.isHost)
                        {
                            GameManager.Instance.playerFlowManager.slidesRound.ResolveDay();
                        }
                    }
                    break;
                case State.angry:
                    timeWaiting += Time.deltaTime;
                    neutralFaceObject.SetActive(false);
                    angryFaceObject.SetActive(true);
                    AudioManager.Instance.PlaySound(responseSoundClip);
                    if (timeWaiting > reactionDuration)
                    {
                        gameObject.SetActive(false);
                        timeWaiting = 0.0f;
                        currentState = State.idle;
                        if(SettingsManager.Instance.isHost)
                        {
                            GameManager.Instance.playerFlowManager.slidesRound.ResolveDay();
                        }
                        
                    }
                    break;
                case State.happy:
                    timeWaiting += Time.deltaTime;
                    neutralFaceObject.SetActive(false);
                    happyFaceObject.SetActive(true);
                    AudioManager.Instance.PlaySound(responseSoundClip);
                    if (timeWaiting > reactionDuration)
                    {
                        gameObject.SetActive(false);
                        timeWaiting = 0.0f;
                        currentState = State.idle;
                        if (SettingsManager.Instance.isHost)
                        {
                            GameManager.Instance.playerFlowManager.slidesRound.ResolveDay();
                        }
                    }
                    break;
            }

        }

        public void Play()
        {
            transform.position = new Vector3(transform.position.x, startingHeight, transform.position.z);
            armTransform.position = startingArmPosition;
            armTransform.rotation = startingArmRotation;
            angryFaceObject.SetActive(false);
            happyFaceObject.SetActive(false);
            neutralFaceObject.SetActive(true);
            currentState = State.preparing;
            gameObject.SetActive(true);
        }
    }
}