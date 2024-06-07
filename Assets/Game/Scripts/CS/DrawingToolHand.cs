using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class DrawingToolHand : MonoBehaviour
    {
        [SerializeField]
        private Transform currentTargetTransform;

        [SerializeField]
        private float switchSpeed;

        // Start is called before the first frame update
        void Start()
        {

        }

        public void SetTargetTransform(Transform inTargetTransform)
        {
            currentTargetTransform = inTargetTransform;
        }

        // Update is called once per frame
        void Update()
        {
            transform.position = Vector3.MoveTowards(transform.position, currentTargetTransform.position, Time.deltaTime * switchSpeed);
        }
    }
}