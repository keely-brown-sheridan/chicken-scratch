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
        private Image birdFaceImage;

        [SerializeField]
        private Image cardBackgroundImage;

        public void Initialize(string playerName, BirdName birdName, string roleName, Color roleColour)
        {
            Bird playerBird = ColourManager.Instance.birdMap[birdName];
            playerNameText.text = playerName;
            playerNameText.color = playerBird.colour;
            roleNameText.text = roleName;
            roleNameText.color = roleColour;
            birdFaceImage.sprite = playerBird.borderedFaceSprite;
            cardBackgroundImage.color = playerBird.colour;
        }
    }
}