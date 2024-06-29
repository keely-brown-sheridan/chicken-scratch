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
            BirdData authorBird = GameDataManager.Instance.GetBird(promptData.author);
            if(authorBird == null)
            {
                Debug.LogError("Could not initialize prompt summary slide section because prompt bird["+promptData.author.ToString()+"] was not mapped in the Colour Manager.");
                return;
            }
            _author = promptData.author;

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
