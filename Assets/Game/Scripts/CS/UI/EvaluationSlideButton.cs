using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ChickenScratch.ColourManager;

namespace ChickenScratch
{
    public class EvaluationSlideButton : MonoBehaviour
    {
        public List<IndexMap> allDrawings;
        public Dictionary<int, GameObject> drawingObjectMap = new Dictionary<int, GameObject>();
        public Dictionary<int, BirdName> drawingAuthorMap = new Dictionary<int, BirdName>();
        public Canvas drawingCanvas;
    }
}