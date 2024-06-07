using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class ScaleToScreenSize : MonoBehaviour
    {
        [SerializeField]
        private float baseScreenWidth = 1280;
        [SerializeField]
        private float baseScreenHeight = 720;

        [System.Serializable]
        public class ScreenResolutionAdjustment
        {
            [SerializeField]
            private float screenWidth = 0;

            [SerializeField]
            private float screenHeight = 0;

            [SerializeField]
            private Vector3 positionAdjustment = Vector3.zero;
        }

        // Start is called before the first frame update
        void Start()
        {
            float widthRatio = baseScreenWidth / Screen.width;
            float heightRatio = baseScreenHeight / Screen.height;
            transform.localScale = new Vector3(transform.localScale.x * widthRatio, transform.localScale.y * heightRatio, transform.localScale.z);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}