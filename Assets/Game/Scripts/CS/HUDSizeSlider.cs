using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class HUDSizeSlider : MonoBehaviour
    {
        [SerializeField]
        private float minKnobXPosition;
        [SerializeField]
        private float maxKnobXPosition;
        [SerializeField]
        private Transform knobTransform;
        [SerializeField]
        private DrawingController drawingController;

        //NO MORE DISCRETE SIZE INCREMENT REQUIREMENT!!!
        public void SetSliderByXPosition(float xPosition)
        {
            drawingController.setSliderKnobPosition(xPosition);
        }
        public void SetSliderByRatio(float ratio)
        {
            float newXPosition = (maxKnobXPosition - minKnobXPosition) * ratio + minKnobXPosition;
            knobTransform.localPosition = new Vector3(newXPosition, knobTransform.localPosition.y, knobTransform.localPosition.z);
        }
    }
}