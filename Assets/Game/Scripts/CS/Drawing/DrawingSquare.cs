using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class DrawingSquare : MonoBehaviour
    {
        public Vector3 position;
        public float radius;
        public float zDepth;
        public List<GameObject> lineObjects = new List<GameObject>();
    }
}