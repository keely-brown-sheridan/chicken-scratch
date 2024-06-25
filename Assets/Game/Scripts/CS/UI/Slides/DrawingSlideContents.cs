using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class DrawingSlideContents : SlideContents
    {
        [SerializeField]
        private GoldStarDetectionArea goldStarDetectionArea;

        [SerializeField]
        private TMP_Text originalPromptReminderText;

        [SerializeField]
        private Transform drawingHolder;

        [SerializeField]
        private Image authorImage;

        [SerializeField]
        private TMP_Text authorNameText;

        [SerializeField]
        private Vector3 drawingOffset;

        [SerializeField]
        private float drawingSize;

        [SerializeField]
        private float timeShowingDrawing;

        [SerializeField]
        private CaseTypeSlideVisualizer caseTypeSlideVisualizer;

        [SerializeField]
        private SlideTimeModifierDecrementVisual slideTimeModifierDecrementVisual;

        private float duration;
        private float timeActive = 0f;
        private DrawingData drawingData;

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

        public void Initialize(DrawingData inDrawingData, string inPrefix, string inNoun, int inRound, int inCaseID, float inDuration, float inTimeModifierDecrement)
        {
            drawingData = inDrawingData;
            duration = inDuration;
            timeActive = 0f;
            
            Bird authorBird = ColourManager.Instance.birdMap[inDrawingData.author];
            authorImage.sprite = authorBird.faceSprite;
            authorNameText.color = authorBird.colour;
            authorNameText.text = SettingsManager.Instance.GetPlayerName(inDrawingData.author);
            goldStarDetectionArea.Initialize(inDrawingData.author, inRound, inCaseID);

            originalPromptReminderText.text = SettingsManager.Instance.CreatePromptText(inPrefix, inNoun);
            EndgameCaseData currentCase = GameManager.Instance.playerFlowManager.slidesRound.caseDataMap[inCaseID];
            caseTypeSlideVisualizer.Initialize(currentCase.caseTypeColour, currentCase.caseTypeName);
            slideTimeModifierDecrementVisual.Initialize(inTimeModifierDecrement);
        }

        public override void Show()
        {
            Vector3 drawingScale = new Vector3(drawingSize, drawingSize, 1f);
            GameManager.Instance.playerFlowManager.AnimateDrawingVisuals(drawingData, drawingHolder, drawingHolder.position + drawingOffset, drawingScale, drawingSize, timeShowingDrawing);

            base.Show();
        }
    }
}
