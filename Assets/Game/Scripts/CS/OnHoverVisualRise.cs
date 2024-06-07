using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ChickenScratch
{
    public class OnHoverVisualRise : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private enum State
        {
            lowered, rising, raised, lowering, invalid
        }
        [SerializeField]
        private State currentState = State.lowered;

        [SerializeField]
        private Transform raisedAnchorTransform;
        [SerializeField]
        private Transform loweredAnchorTransform;
        [SerializeField]
        private float speed;
        [SerializeField]
        private float stoppingDistance;
        [SerializeField]
        private Transform visualsHolderTransform;

        void Update()
        {
            switch (currentState)
            {
                case State.rising:
                    visualsHolderTransform.position = Vector3.MoveTowards(visualsHolderTransform.position, raisedAnchorTransform.position, speed * Time.deltaTime);

                    if (Vector3.Distance(visualsHolderTransform.position, raisedAnchorTransform.position) < stoppingDistance)
                    {
                        visualsHolderTransform.position = raisedAnchorTransform.position;
                        currentState = State.raised;
                    }
                    break;
                case State.lowering:
                    visualsHolderTransform.position = Vector3.MoveTowards(visualsHolderTransform.position, loweredAnchorTransform.position, speed * Time.deltaTime);

                    if (Vector3.Distance(visualsHolderTransform.position, loweredAnchorTransform.position) < stoppingDistance)
                    {
                        visualsHolderTransform.position = loweredAnchorTransform.position;
                        currentState = State.lowered;
                    }
                    break;
            }
        }

        public void Hover()
        {
            if (currentState == State.lowered ||
                currentState == State.lowering)
            {
                currentState = State.rising;
            }
        }

        public void Unhover()
        {
            if (currentState == State.rising ||
                currentState == State.raised)
            {
                currentState = State.lowering;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Hover();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Unhover();
        }
    }
}