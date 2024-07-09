using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class GuessCaseEmailSection : CaseEmailSection
    {
        [SerializeField]
        private BirdImage playerImage;

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
            BirdData guessingBird = GameDataManager.Instance.GetBird(guessData.author);
            if(guessingBird == null)
            {
                Debug.LogError("Could not initialize the guess case email section because the guess bird["+ guessData.author.ToString()+"] was not mapped in the Colour Manager.");
                return;
            }
            BirdHatData.HatType birdHat = GameManager.Instance.playerFlowManager.GetBirdHatType(guessData.author);
            playerImage.Initialize(guessData.author, birdHat);

            playerNameText.text = SettingsManager.Instance.GetPlayerName(guessData.author);
            playerNameText.color = guessingBird.colour;

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
