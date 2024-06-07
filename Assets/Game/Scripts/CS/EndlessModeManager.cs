
using UnityEngine;


namespace ChickenScratch
{
    public class EndlessModeManager : MonoBehaviour
    {
        public bool hasStartedOverflow = false;
        public bool isInOverflow = false;

        public float totalOverflowTime = 5.0f;

        private float timeRemainingInOverflow = 0.0f;

        void Update()
        {
            if (isInOverflow)
            {
                timeRemainingInOverflow -= Time.deltaTime;
                if (timeRemainingInOverflow <= 0)
                {
                    isInOverflow = false;
                }
            }
        }


    }
}