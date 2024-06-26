using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class PromptSlideContents : SlideContents
    {
        [SerializeField]
        private GoldStarDetectionArea goldStarDetectionArea;
        [SerializeField]
        private TMP_Text originalPromptReminderText;
        [SerializeField]
        private TMP_Text playerPromptText;
        [SerializeField]
        private Image authorImage;
        [SerializeField]
        private TMP_Text authorNameText;
        [SerializeField]
        private CaseTypeSlideVisualizer caseTypeSlideVisualizer;
        [SerializeField]
        private SlideTimeModifierDecrementVisual slideTimeModifierDecrementVisual;
        private float duration;
        private float timeActive = 0f;

        private void Update()
        {
            if (active)
            {
                timeActive += Time.deltaTime * GameManager.Instance.playerFlowManager.slidesRound.slideSpeed;
                if (timeActive > duration)
                {
                    isComplete = true;
                }
            }
        }

        public void Initialize(PlayerTextInputData promptData, string prefix, string noun, int round, int caseID, float inDuration, float inTimeModifierDecrement)
        {
            duration = inDuration;
            timeActive = 0f;
            Bird authorBird = ColourManager.Instance.GetBird(promptData.author);
            if(authorBird == null)
            {
                Debug.LogError("Could not initialize prompt slide contents because prompt bird["+promptData.author.ToString()+"] was not mapped in the Colour Manager.");
                return;
            }
            authorImage.sprite = authorBird.faceSprite;
            authorNameText.text = SettingsManager.Instance.GetPlayerName(promptData.author);
            authorNameText.color = authorBird.colour;

            originalPromptReminderText.text = SettingsManager.Instance.CreatePromptText(prefix, noun);
            playerPromptText.text = promptData.text;
            playerPromptText.color = authorBird.colour;
            goldStarDetectionArea.Initialize(promptData.author, round, caseID);
            EndgameCaseData currentCase = GameManager.Instance.playerFlowManager.slidesRound.caseDataMap[caseID];
            caseTypeSlideVisualizer.Initialize(currentCase.caseTypeColour, currentCase.caseTypeName);
            slideTimeModifierDecrementVisual.Initialize(inTimeModifierDecrement);
        }
    }
}
