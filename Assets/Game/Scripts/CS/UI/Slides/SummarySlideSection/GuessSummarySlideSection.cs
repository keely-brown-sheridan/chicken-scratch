using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class GuessSummarySlideSection : SummarySlideSection
    {
        [SerializeField]
        private TMPro.TMP_Text prefixText;

        [SerializeField]
        private TMPro.TMP_Text nounText;

        [SerializeField]
        private Color correctColour, incorrectColour;

        public void Initialize(ColourManager.BirdName guesser, Dictionary<int,string> guessesMap, Dictionary<int,CaseWordData> correctWordsMap)
        {
            Bird authorBird = ColourManager.Instance.birdMap[guesser];
            authorImage.sprite = authorBird.faceSprite;
            authorNameText.color = authorBird.colour;

            bool isWordCorrect;
            if (guessesMap.ContainsKey(1))
            {
                prefixText.text = guessesMap[1];
                isWordCorrect = guessesMap[1] == correctWordsMap[1].value;
                prefixText.color = isWordCorrect ? correctColour : incorrectColour;
            }
            if (guessesMap.ContainsKey(2))
            {
                nounText.text = guessesMap[2];
                isWordCorrect = guessesMap[2] == correctWordsMap[2].value;
                nounText.color = isWordCorrect ? correctColour : incorrectColour;
            }
        }
    }
}
