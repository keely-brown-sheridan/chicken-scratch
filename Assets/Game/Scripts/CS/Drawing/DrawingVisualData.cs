using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ChickenScratch.DrawingLineData;

namespace ChickenScratch
{
    [System.Serializable]
    public class DrawingVisualData
    {

        public float zDepth = -1f;
        public int sortingOrder = 0;
        public Vector3 objectPosition = new Vector3(0f, 0f, 0f);

    }
}