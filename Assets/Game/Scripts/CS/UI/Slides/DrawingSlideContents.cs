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
        private Transform drawingHolder;

        [SerializeField]
        private Vector3 drawingOffset;

        [SerializeField]
        private float drawingSize;

        [SerializeField]
        private float timeShowingDrawing;

        [SerializeField]
        private SlideTimeModifierDecrementVisual slideTimeModifierDecrementVisual;

        private float duration;
        private float timeActive = 0f;
        private DrawingData drawingData;
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

        public void Initialize(DrawingData inDrawingData, int inRound, int inCaseID, float inDuration, float inTimeModifierDecrement)
        {
            drawingData = inDrawingData;
            duration = inDuration;
            timeActive = 0f;

            BirdData authorBird = GameDataManager.Instance.GetBird(drawingData.author);
            if (authorBird == null)
            {
                Debug.LogError("Could not initialize drawing slide contents because drawing bird["+drawingData.author.ToString()+"] was not mapped in the Colour Manager.");
                return;
            }
            author = drawingData.author;

            goldStarDetectionArea.Initialize(inDrawingData.author, inRound, inCaseID);
            EndgameCaseData currentCase = GameManager.Instance.playerFlowManager.slidesRound.caseDataMap[inCaseID];
            slideTimeModifierDecrementVisual.Initialize(inTimeModifierDecrement);
        }
        public override void Show()
        {
            GameManager.Instance.playerFlowManager.slidesRound.ShowCaseDetails();
            GameManager.Instance.playerFlowManager.slidesRound.UpdatePreviewBird(author);
            Vector3 drawingScale = new Vector3(drawingSize, drawingSize, 1f);
            GameManager.Instance.playerFlowManager.AnimateDrawingVisuals(drawingData, drawingHolder, drawingHolder.position + drawingOffset, drawingScale, drawingSize, timeShowingDrawing);

            base.Show();
        }
    }
}
