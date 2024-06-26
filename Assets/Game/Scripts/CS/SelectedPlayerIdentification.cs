using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ChickenScratch.ColourManager;

namespace ChickenScratch
{
    public class SelectedPlayerIdentification : MonoBehaviour
    {
        [SerializeField]
        private Image cardColouredSectionImage;

        [SerializeField]
        private Image cardFaceImage;

        [SerializeField]
        private TMPro.TMP_Text cardNameText;

        public BirdName cardBirdName => _cardBirdName;

        private BirdName _cardBirdName = BirdName.none;

        public string playerName => _playerName;

        private string _playerName = "";

        public void Initialize(BirdName inBirdName, string inPlayerName)
        {
            _cardBirdName = inBirdName;
            _playerName = inPlayerName;
            Bird playerBird = ColourManager.Instance.GetBird(_cardBirdName);
            if(playerBird == null)
            {
                Debug.LogError("Could not initialize the selected player identification because the player bird["+_cardBirdName.ToString()+"] has not been mapped in the Colour Manager.");
            }
            else
            {
                cardFaceImage.sprite = playerBird.faceSprite;
                cardColouredSectionImage.color = playerBird.colour;
                cardNameText.color = playerBird.colour;
            }
            
            cardNameText.text = _playerName;
        }
    }
}