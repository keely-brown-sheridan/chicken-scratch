using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class WorkersOutcomeContents : MonoBehaviour
    {
        public List<BirdTag> traitorCrossoutImages;

        public Dictionary<ColourManager.BirdName, GameObject> traitorCrossoutImageMap = new Dictionary<ColourManager.BirdName, GameObject>();
    }
}