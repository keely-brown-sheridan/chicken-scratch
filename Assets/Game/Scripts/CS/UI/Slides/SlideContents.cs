
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class SlideContents : MonoBehaviour
    {
        public bool active = false;
        public bool isComplete = false;
        public float currentSpeedupFactor = 1.0f;
        private bool isInitialized = false;

        public virtual void Show()
        {
            gameObject.SetActive(true);
            active = true;
        }
    }
}