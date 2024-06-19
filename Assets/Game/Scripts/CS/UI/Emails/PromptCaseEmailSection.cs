using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class PromptCaseEmailSection : CaseEmailSection
    {
        [SerializeField]
        private TMPro.TMP_Text playerPromptText;
        [SerializeField]
        private TMPro.TMP_Text playerNameText;

        [SerializeField]
        private Image playerImage;

        public void Initialize(PlayerTextInputData promptData, PlayerRatingData ratingData)
        {
            playerPromptText.text = promptData.text;
            playerNameText.text = SettingsManager.Instance.GetPlayerName(promptData.author);
            
            Bird promptBird = ColourManager.Instance.birdMap[promptData.author];
            playerNameText.color = promptBird.colour;
            playerImage.sprite = promptBird.faceSprite;
            playerPromptText.color = promptBird.colour;

            SetRating(ratingData.likeCount);
        }
    }
}
