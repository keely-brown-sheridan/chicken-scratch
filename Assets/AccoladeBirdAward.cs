using ChickenScratch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class AccoladeBirdAward : MonoBehaviour
    {
        [SerializeField]
        private Animator animator;

        [SerializeField]
        private Image leftArmImage, rightArmImage;

        [SerializeField]
        private BirdImage birdFaceImage;

        [SerializeField]
        private TMPro.TMP_Text playerNameText;

        [SerializeField]
        private TMPro.TMP_Text birdBuckText;

        public float width => _width;
        [SerializeField]
        private float _width;

        public int rank => _rank;
        [SerializeField]
        private int _rank;

        public float xOffset => _xOffset;

        private float _xOffset;

        public void Initialize(ColourManager.BirdName birdName, string playerName, int birdBucksEarned, float inXOffset)
        {
            BirdData birdData = GameDataManager.Instance.GetBird(birdName);
            if(birdData != null)
            {
                BirdHatData.HatType birdHat = GameManager.Instance.playerFlowManager.GetBirdHatType(birdName);
                birdFaceImage.Initialize(birdName, birdHat);
                playerNameText.text = playerName;
                playerNameText.color = birdData.colour;
            }

            //Randomize the arms
            //Choose a random bird for the bird arms lifting the award
            int birdIndex = UnityEngine.Random.Range(0, ColourManager.Instance.allBirds.Count);
            Bird randomBird = ColourManager.Instance.allBirds[birdIndex];

            leftArmImage.sprite = randomBird.armSprite;
            rightArmImage.sprite = randomBird.armSprite;
            birdBuckText.text = birdBucksEarned.ToString();
            _xOffset = inXOffset;
        }

        public void Lift()
        {
            animator.SetTrigger("Rise");
        }


    }
}

