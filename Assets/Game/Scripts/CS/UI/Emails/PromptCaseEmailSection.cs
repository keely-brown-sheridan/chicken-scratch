using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class PromptCaseEmailSection : CaseEmailSection
    {
        [SerializeField]
        private TMPro.TMP_Text playerPromptText;

        [SerializeField]
        private Image playerImage;

        public void Initialize(PlayerTextInputData promptData, PlayerRatingData ratingData)
        {
            playerPromptText.text = promptData.text;
            Bird promptBird = ColourManager.Instance.birdMap[promptData.author];
            playerImage.sprite = promptBird.faceSprite;
            playerPromptText.color = promptBird.colour;

            SetRating(ratingData.likeCount);
        }
    }
}
