using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class ConveyorBelt : MonoBehaviour
    {
        [SerializeField]
        private Transform treadHolderTransform;
        [SerializeField]
        private GameObject treadPrefab;
        [SerializeField]
        private Transform treadRespawnLimitTransform;
        [SerializeField]
        private float speed;
        [SerializeField]
        private float treadHeight;

        private List<GameObject> treads = new List<GameObject>();
        // Start is called before the first frame update
        void Start()
        {
            int iterations = 0;
            bool endOfTrackHasBeenReached = false;
            float currentTreadOffset = 0.0f;
            while (!endOfTrackHasBeenReached)
            {
                GameObject newTread = Instantiate(treadPrefab, treadHolderTransform.position + Vector3.up * currentTreadOffset, Quaternion.identity, treadHolderTransform);
                treads.Add(newTread);
                currentTreadOffset -= treadHeight;

                if (treadHolderTransform.position.y + currentTreadOffset < treadRespawnLimitTransform.position.y)
                {
                    endOfTrackHasBeenReached = true;
                }
                iterations++;
                if (iterations > 1000)
                {
                    break;
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            foreach (GameObject tread in treads)
            {
                //tread.transform.position -= speed * Vector3.up * Time.deltaTime;
            }
        }
    }
}