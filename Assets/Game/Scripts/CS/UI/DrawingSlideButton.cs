using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ChickenScratch.ColourManager;

namespace ChickenScratch
{
    public class DrawingSlideButton : MonoBehaviour
    {
        public List<BirdTag> allDrawings;
        public Dictionary<BirdName, GameObject> drawingObjectMap = new Dictionary<BirdName, GameObject>();
        public Dictionary<int, BirdName> birdNameIndexMap = new Dictionary<int, BirdName>();
        public Canvas drawingCanvas;
    }
}