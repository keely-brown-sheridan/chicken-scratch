using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ChickenScratch.DrawingController;

namespace ChickenScratch
{
    public abstract class DrawingTool : MonoBehaviour
    {
        public DrawingToolType type;
        public DrawingController controller;

        [SerializeField]
        protected GameObject glowOutlineObject;
        [SerializeField]
        protected GameObject glowFillObject;

        public float currentSizeRatio => _currentSizeRatio;

        [SerializeField]
        protected float _currentSizeRatio = 0.5f;

        public abstract DrawingAction drawingUpdate();
        public abstract void use();
        public abstract void release();
        public abstract void changeSize(float changeDirection);
        public abstract void setSize(float ratio);
        public virtual int getSizeIndex()
        {
            return -1;
        }
        public virtual int getMaxSizeIndex()
        {
            return -1;
        }
    }
}