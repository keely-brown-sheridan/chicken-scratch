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
            
            Bird promptBird = ColourManager.Instance.GetBird(promptData.author);
            if(promptBird == null)
            {
                Debug.LogError("Could not initialize the prompt case email section because the prompt bird["+promptData.author.ToString()+"] was not mapped in the Colour Manager.");
                return;
            }
            playerNameText.color = promptBird.colour;
            playerImage.sprite = promptBird.faceSprite;
            playerPromptText.color = promptBird.colour;

            SetRating(ratingData.likeCount);
        }
    }
}
