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
        private TMPro.TMP_Text playerNameText;

        [SerializeField]
        private Color correctColour, incorrectColour;
        public void Initialize(Dictionary<int,string> correctWordIdentifiersMap, GuessData guessData, PlayerRatingData ratingData)
        {
            Bird drawingBird = ColourManager.Instance.birdMap[guessData.author];
            playerImage.sprite = drawingBird.faceSprite;
            playerNameText.text = SettingsManager.Instance.GetPlayerName(guessData.author);
            playerNameText.color = drawingBird.colour;

            prefixText.text = guessData.prefix;
            CaseWordData correctPrefix = GameDataManager.Instance.GetWord(correctWordIdentifiersMap[1]);
            prefixText.color = guessData.prefix == correctPrefix.value ? correctColour : incorrectColour;
            nounText.text = guessData.noun;
            CaseWordData correctNoun = GameDataManager.Instance.GetWord(correctWordIdentifiersMap[2]);
            nounText.color = guessData.noun == correctNoun.value ? correctColour : incorrectColour;

            SetRating(ratingData.likeCount);
        }
    }
}
