using UnityEngine;

namespace ChickenScratch
{
    public class PromptSummarySlideSection : SummarySlideSection
    {
        [SerializeField]
        private GoldStarDetectionArea goldStarDetectionArea;
        [SerializeField]
        private TMPro.TMP_Text promptText;
        [SerializeField]
        private SlideTimeModifierDecrementVisual slideTimeModifierDecrementVisual;

        public void Initialize(PlayerTextInputData promptData, int round, int caseID, float timeModifierDecrement)
        {
            Bird authorBird = ColourManager.Instance.birdMap[promptData.author];
            authorImage.sprite = authorBird.faceSprite;
            authorNameText.color = authorBird.colour;
            authorNameText.text = SettingsManager.Instance.GetPlayerName(promptData.author);

            promptText.text = promptData.text;
            promptText.color = authorBird.colour;

            goldStarDetectionArea.Initialize(promptData.author, round, caseID);
            slideTimeModifierDecrementVisual.Initialize(timeModifierDecrement);
        }
    }
}
