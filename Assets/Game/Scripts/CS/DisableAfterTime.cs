using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class DisableAfterTime : MonoBehaviour
    {
        [SerializeField]
        private float disableTime;

        private float timeActive = 0.0f;

        void OnEnable()
        {
            timeActive = 0.0f;
        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            timeActive += Time.deltaTime;
            if (timeActive > disableTime)
            {
                gameObject.SetActive(false);
                timeActive = 0.0f;
            }
        }
    }
}