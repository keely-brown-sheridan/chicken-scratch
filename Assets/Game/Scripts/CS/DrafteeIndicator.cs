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
            BirdData drafteeBird = GameDataManager.Instance.GetBird(inBirdName);
            if (drafteeBird == null)
            {
                Debug.LogError("Could not set draftee indicator bird["+inBirdName.ToString()+"] because it is not mapped in the ColourManager.");
                return;
            }
            birdFaceImage.sprite = drafteeBird.faceSprite;
        }
    }
}