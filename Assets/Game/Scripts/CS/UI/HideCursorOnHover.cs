using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class HideCursorOnHover : MonoBehaviour
    {
        public bool activated = true;
        public LayerMask mouseDetectionLayer;
        public bool isHovered = false;
        public string hiderName;

        private void Update()
        {


        }
    }
}