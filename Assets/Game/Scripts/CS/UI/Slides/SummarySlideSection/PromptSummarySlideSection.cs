using UnityEngine;

namespace ChickenScratch
{
    public class PromptSummarySlideSection : SummarySlideSection
    {
        [SerializeField]
        private TMPro.TMP_Text promptText;

        public void Initialize(PlayerTextInputData promptData)
        {
            Bird authorBird = ColourManager.Instance.birdMap[promptData.author];
            authorImage.sprite = authorBird.faceSprite;
            authorNameText.color = authorBird.colour;

            promptText.text = promptData.text;
            promptText.color = authorBird.colour;
        }
    }
}
