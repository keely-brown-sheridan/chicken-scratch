
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class SlideProgressIndicator : MonoBehaviour
    {
        public ColourManager.BirdName birdName;
        public int cabinetID;
        public int tab;
        public Image image;

        public List<GameObject> objectsToHideOnRate;
    }
}