using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class FileStamp : MonoBehaviour
    {
        public UnityEvent onStampComplete;

        [SerializeField]
        private Transform restingPositionTransform;

        [SerializeField]
        private Transform activePositionTransform;
        [SerializeField]
        private Transform stampingPositionTransform;
        [SerializeField]
        private Transform peekPositionTransform;

        [SerializeField]
        private float speed;

        [SerializeField]
        private float stampSpeed;

        [SerializeField]
        private float arrivalThreshold;

        [SerializeField]
        private float stampWaitDuration;

        [SerializeField]
        private GameObject stampedInkObject;

        [SerializeField]
        private Button stampButton;

        private float timeStamping = 0f;

        private enum State
        {
            rest, rise, active, leave, press, peek, wait, invalid
        }

        private State currentState = State.rest;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void FixedUpdate()
        {
            switch (currentState)
            {
                case State.rise:
                    transform.position = Vector3.MoveTowards(transform.position, activePositionTransform.position, speed);
                    if (Vector3.Distance(transform.position, activePositionTransform.position) < arrivalThreshold)
                    {
                        transform.position = activePositionTransform.position;
                        currentState = State.active;
                    }
                    break;
                case State.press:
                    AudioManager.Instance.PlaySound("Stamp");
                    transform.position = Vector3.MoveTowards(transform.position, stampingPositionTransform.position, stampSpeed);
                    if (Vector3.Distance(transform.position, stampingPositionTransform.position) < arrivalThreshold)
                    {
                        stampedInkObject.SetActive(true);
                        transform.position = stampingPositionTransform.position;
                        currentState = State.peek;
                    }
                    break;
                case State.peek:
                    transform.position = Vector3.MoveTowards(transform.position, peekPositionTransform.position, speed);
                    if (Vector3.Distance(transform.position, peekPositionTransform.position) < arrivalThreshold)
                    {
                        transform.position = peekPositionTransform.position;
                        currentState = State.wait;
                        timeStamping = 0f;
                    }
                    break;
                case State.wait:
                    
                    timeStamping += Time.deltaTime;
                    if(timeStamping > stampWaitDuration)
                    {
                        onStampComplete.Invoke();
                        currentState = State.leave;
                    }
                    break;
                case State.leave:
                    transform.position = Vector3.MoveTowards(transform.position, restingPositionTransform.position, speed);
                    if (Vector3.Distance(transform.position, restingPositionTransform.position) < arrivalThreshold)
                    {
                        transform.position = restingPositionTransform.position;
                        
                        currentState = State.rest;
                    }
                    break;
            }
        }

        public void SetAsActive()
        {
            stampButton.interactable = true;
            stampedInkObject.SetActive(false);
            currentState = State.rise;
        }

        public void StampFile()
        {
            stampButton.interactable = false;
            currentState = State.press;
        }
    }
}