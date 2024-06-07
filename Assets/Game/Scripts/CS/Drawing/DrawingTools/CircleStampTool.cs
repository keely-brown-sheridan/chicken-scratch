using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class CircleStampTool : DrawingTool
    {
        private bool _isActive = false;

        [SerializeField]
        private float selectionTranslationValue = 0.15f;

        [SerializeField]
        private string selectionSoundName = "";

        [SerializeField]
        private Transform toolVisualsHolder;

        public override DrawingAction drawingUpdate()
        {
            return null;
        }

        public override void release()
        {
            _isActive = false;
            toolVisualsHolder.position -= new Vector3(0, selectionTranslationValue, 0);
            glowOutlineObject.SetActive(false);
        }

        public override void changeSize(float changeDirection)
        {
            throw new System.NotImplementedException();
        }

        public override void setSize(float xPosition)
        {
            throw new System.NotImplementedException();
        }

        public override void use()
        {
            _isActive = true;
            toolVisualsHolder.position += new Vector3(0, selectionTranslationValue, 0);
            glowOutlineObject.SetActive(true);
            glowFillObject.SetActive(false);
        }
    }
}