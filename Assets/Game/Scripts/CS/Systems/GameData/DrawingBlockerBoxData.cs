using System;
using UnityEngine;

namespace ChickenScratch
{
    [Serializable]
    public class DrawingBlockerBoxData
    {
        public Vector3 startingDimensions = Vector3.zero, targetDimensions = Vector3.zero;
        public Vector3 startingPosition = Vector3.zero, targetPosition = Vector3.zero;
        public float startingTimeRatio = 0f, endingTimeRatio = 0f;
    }
}
