using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class TraitorEmailContents : MonoBehaviour
    {
        public List<BirdTag> traitorImages;

        public Dictionary<ColourManager.BirdName, GameObject> traitorImageMap = new Dictionary<ColourManager.BirdName, GameObject>();

    }
}