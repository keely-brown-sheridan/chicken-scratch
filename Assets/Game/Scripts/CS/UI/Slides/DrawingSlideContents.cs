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

        public void Initialize(DrawingData drawingData, string prefix, string noun, int round, int caseID, float inDuration, float timeModifierDecrement)
        {
            duration = inDuration;
            timeActive = 0f;
            Vector3 drawingScale = new Vector3(drawingSize, drawingSize, 1f);
            GameManager.Instance.playerFlowManager.createDrawingVisuals(drawingData, drawingHolder, drawingHolder.position + drawingOffset, drawingScale, drawingSize);

            Bird authorBird = ColourManager.Instance.birdMap[drawingData.author];
            authorImage.sprite = authorBird.faceSprite;
            authorNameText.color = authorBird.colour;
            authorNameText.text = SettingsManager.Instance.GetPlayerName(drawingData.author);
            goldStarDetectionArea.Initialize(drawingData.author, round, caseID);

            originalPromptReminderText.text = SettingsManager.Instance.CreatePromptText(prefix, noun);
            EndgameCaseData currentCase = GameManager.Instance.playerFlowManager.slidesRound.caseDataMap[caseID];
            caseTypeSlideVisualizer.Initialize(currentCase.caseTypeColour, currentCase.caseTypeName);
            slideTimeModifierDecrementVisual.Initialize(timeModifierDecrement);
        }
    }
}
