using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ChickenScratch.ColourManager;

namespace ChickenScratch
{
    public class DrafteeIndicator : MonoBehaviour
    {
        [SerializeField]
        private Image birdFaceImage;

        public BirdName birdName;

        public void setBird(BirdName inBirdName)
        {
            birdName = inBirdName;
            birdFaceImage.sprite = ColourManager.Instance.birdMap[birdName].faceSprite;
        }
    }
}