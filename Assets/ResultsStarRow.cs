using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class ResultsStarRow : MonoBehaviour
    {
        [SerializeField]
        private BirdImage birdImage;

        [SerializeField]
        private GameObject starPrefab;

        [SerializeField]
        private Transform starHolder;

        public void Initialize(ColourManager.BirdName bird, int starCount)
        {
            birdImage.Initialize(bird, GameManager.Instance.playerFlowManager.GetBirdHatType(bird));
            for(int i = 0; i < starCount; i++)
            {
                Instantiate(starPrefab, starHolder);
            }
        }
    }
}

