using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class ResultsCasesRow : MonoBehaviour
    {
        [SerializeField]
        private BirdImage birdImage;

        [SerializeField]
        private GameObject caseFolderPrefab;

        [SerializeField]
        private Transform caseFolderParent;
        
        public void Initialize(ColourManager.BirdName bird, List<Color> folderColours)
        {
            birdImage.Initialize(bird, GameManager.Instance.playerFlowManager.GetBirdHatType(bird));
            foreach(Color folderColour in folderColours)
            {
                GameObject folderObject = Instantiate(caseFolderPrefab, caseFolderParent);
                Image folderImage = folderObject.GetComponent<Image>();
                folderImage.color = folderColour;
            }
        }
    }
}

