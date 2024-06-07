using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class FileStamp : MonoBehaviour
    {
        [SerializeField]
        private Transform restingPositionTransform;

        [SerializeField]
        private Transform activePositionTransform;

        [SerializeField]
        private float speed;

        [SerializeField]
        private float arrivalThreshold;

        private enum State
        {
            rest, rise, active, lower, invalid
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
                case State.lower:
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
            currentState = State.rise;
        }

        public void SetAsResting()
        {
            currentState = State.lower;
        }
    }
}