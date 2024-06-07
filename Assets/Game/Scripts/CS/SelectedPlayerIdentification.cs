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
            Bird playerBird = ColourManager.Instance.birdMap[_cardBirdName];
            cardFaceImage.sprite = playerBird.faceSprite;
            cardColouredSectionImage.color = playerBird.colour;
            cardNameText.text = _playerName;
            cardNameText.color = playerBird.colour;
        }
    }
}