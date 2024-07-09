using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class GuessSummarySlideSection : SummarySlideSection
    {
        [SerializeField]
        private GoldStarDetectionArea goldStarDetectionArea;

        [SerializeField]
        private TMPro.TMP_Text prefixText;

        [SerializeField]
        private TMPro.TMP_Text nounText;

        [SerializeField]
        private Color correctColour, incorrectColour;

        [SerializeField]
        private SlideTimeModifierDecrementVisual slideTimeModifierDecrementVisual;

        public void Initialize(GuessData guessData, Dictionary<int,string> correctWordIdentifiersMap, int round, int caseID, float timeModifierDecrement)
        {
            
            BirdData authorBird = GameDataManager.Instance.GetBird(guessData.author);
            if(authorBird == null)
            {
                Debug.LogError("Could not initialize guess sumamry slide section because guess bird["+guessData.author.ToString()+"] was not mapped in the Colour Manager.");
                return;
            }
            _author = guessData.author;
            BirdHatData.HatType birdHat = GameManager.Instance.playerFlowManager.GetBirdHatType(guessData.author);
            authorImage.Initialize(guessData.author, birdHat);

            authorNameText.color = authorBird.colour;
            authorNameText.text = SettingsManager.Instance.GetPlayerName(guessData.author);

            bool isWordCorrect;
            prefixText.text = guessData.prefix;
            CaseWordData correctPrefix = GameDataManager.Instance.GetWord(correctWordIdentifiersMap[1]);
            isWordCorrect = guessData.prefix == correctPrefix.value;
            prefixText.color = isWordCorrect ? correctColour : incorrectColour;
            nounText.text = guessData.noun;
            CaseWordData correctNoun = GameDataManager.Instance.GetWord(correctWordIdentifiersMap[2]);
            isWordCorrect = guessData.noun == correctNoun.value;
            nounText.color = isWordCorrect ? correctColour : incorrectColour;

            goldStarDetectionArea.Initialize(guessData.author, round, caseID);
            slideTimeModifierDecrementVisual.Initialize(timeModifierDecrement);
        }
    }
}
