using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class Shimmy : MonoBehaviour
    {

        [SerializeField] private float angle;
        [SerializeField] private float speed;

        private bool isOn = true;
        private Vector3 initialEulerAngles;
        void Start()
        {
            initialEulerAngles = transform.eulerAngles;
        }

        // Update is called once per frame
        void Update()
        {
            if (isOn)
            {
                float updatedRotation = Mathf.PingPong(Time.time * speed, angle * 2) - angle;
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, updatedRotation);
            }
            else
            {
                //transform.eulerAngles = Vector3.RotateTowards(transform.eulerAngles, initialEulerAngles, speed, speed);
            }

        }

        public void Resume()
        {
            isOn = true;
        }

        public void Stop()
        {
            isOn = false;
            transform.eulerAngles = initialEulerAngles;
        }
    }
}