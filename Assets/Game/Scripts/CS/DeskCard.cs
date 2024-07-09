using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ChickenScratch.ColourManager;

namespace ChickenScratch
{
    public class DeskCard : MonoBehaviour
    {
        [SerializeField]
        private Text playerNameText;

        [SerializeField]
        private Text roleNameText;

        [SerializeField]
        private BirdImage birdFaceImage;

        [SerializeField]
        private Image cardBackgroundImage;

        public void Initialize(string playerName, BirdName birdName, string roleName, Color roleColour)
        {
            BirdData playerBird = GameDataManager.Instance.GetBird(birdName);
            if(playerBird == null)
            {
                Debug.LogError("Could not initialize desk card because player bird["+birdName.ToString()+"] is not mapped to the ColourManager.");
                return;
            }
            playerNameText.text = playerName;
            playerNameText.color = playerBird.colour;
            roleNameText.text = roleName;
            roleNameText.color = roleColour;
            BirdHatData.HatType birdHat = GameManager.Instance.playerFlowManager.GetBirdHatType(birdName);
            birdFaceImage.Initialize(birdName, birdHat);
            cardBackgroundImage.color = playerBird.colour;
        }
    }
}