using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class GuessCaseEmailSection : CaseEmailSection
    {
        [SerializeField]
        private Image playerImage;

        [SerializeField]
        private TMPro.TMP_Text prefixText;

        [SerializeField]
        private TMPro.TMP_Text nounText;

        [SerializeField]
        private Color correctColour, incorrectColour;
        public void Initialize(Dictionary<int,CaseWordData> correctWordsMap, Dictionary<int,string> guessWordsMap, ColourManager.BirdName author, PlayerRatingData ratingData)
        {
            Bird drawingBird = ColourManager.Instance.birdMap[author];
            playerImage.sprite = drawingBird.faceSprite;

            if(guessWordsMap.ContainsKey(1))
            {
                prefixText.text = guessWordsMap[1];
                prefixText.color = guessWordsMap[1] == correctWordsMap[1].value ? correctColour : incorrectColour;
            }
            if(guessWordsMap.ContainsKey(2))
            {
                nounText.text = guessWordsMap[2];
                nounText.color = guessWordsMap[2] == correctWordsMap[2].value ? correctColour : incorrectColour;
            }

            SetRating(ratingData.likeCount);
        }
    }
}
