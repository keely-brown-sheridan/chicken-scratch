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
        private TMP_Text playerPromptText;

        [SerializeField]
        private SlideTimeModifierDecrementVisual slideTimeModifierDecrementVisual;
        private float duration;
        private float timeActive = 0f;
        private ColourManager.BirdName author;

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

        public void Initialize(PlayerTextInputData promptData, int round, int caseID, float inDuration, float inTimeModifierDecrement)
        {
            duration = inDuration;
            timeActive = 0f;
            BirdData authorBird = GameDataManager.Instance.GetBird(promptData.author);
            if(authorBird == null)
            {
                Debug.LogError("Could not initialize prompt slide contents because prompt bird["+promptData.author.ToString()+"] was not mapped in the Colour Manager.");
                return;
            }
            author = promptData.author;
            playerPromptText.text = promptData.text;
            playerPromptText.color = authorBird.colour;
            goldStarDetectionArea.Initialize(promptData.author, round, caseID);
            EndgameCaseData currentCase = GameManager.Instance.playerFlowManager.slidesRound.caseDataMap[caseID];
            slideTimeModifierDecrementVisual.Initialize(inTimeModifierDecrement);
        }

        public override void Show()
        {
            GameManager.Instance.playerFlowManager.slidesRound.ShowCaseDetails();
            GameManager.Instance.playerFlowManager.slidesRound.UpdatePreviewBird(author);
            base.Show();
        }
    }
}
