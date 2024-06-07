using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class SpriteAssigner : MonoBehaviour
    {
        public Sprite sprite;
        public Vector3 initialPosition;

        private bool isInitialized = false;

        private void Start()
        {
            Debug.LogError("The pelican's arm position is: " + transform.position);
        }

        // Update is called once per frame
        void Update()
        {
            if (!isInitialized)
            {
                transform.position = initialPosition;
                GetComponent<SpriteRenderer>().sprite = sprite;
                isInitialized = true;

                Debug.LogError("The pelican's initialized arm position is: " + transform.position);
            }
        }
    }
}