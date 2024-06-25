using ChickenScratch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class SlideDistributedBirdbuck : MonoBehaviour
    {
        [SerializeField]
        private float arrivalTime;
        [SerializeField]
        private string arrivalSFX;

        private float timeActive = 0f;
        private SummarySlideSection section;
        private Vector3 startingPosition;

        public void Initialize(SummarySlideSection inSection)
        {
            startingPosition = transform.position;
            section = inSection;
        }

        // Update is called once per frame
        void Update()
        {
            timeActive += Time.deltaTime;
            float timeRatio = timeActive / arrivalTime;
            Vector3 currentPosition = Vector3.Lerp(startingPosition, section.birdBuckArrivalTransform.position, timeRatio);
            transform.position = currentPosition;

            if(timeActive > arrivalTime)
            {
                AudioManager.Instance.PlaySound(arrivalSFX, true);
                section.IncreaseBirdbucks();
                Destroy(gameObject);
            }
        }
    }
}

