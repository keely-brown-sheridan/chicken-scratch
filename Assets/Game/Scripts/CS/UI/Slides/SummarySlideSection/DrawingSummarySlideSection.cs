using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class DrawingSummarySlideSection : SummarySlideSection
    {
        [SerializeField]
        private GoldStarDetectionArea goldStarDetectionArea;
        [SerializeField]
        private Transform drawingReferencePoint;

        [SerializeField]
        private Vector3 drawingOffset;

        [SerializeField]
        private float drawingSize;

        [SerializeField]
        private SlideTimeModifierDecrementVisual slideTimeModifierDecrementVisual;

        public Transform summarySlideTransform;

        private DrawingData drawingData;

        private void Start()
        {
            Show();
        }

        public void Initialize(DrawingData inDrawingData, Transform inSummarySlideTransform, int round, int caseID, float timeModifierDecrement)
        {
            drawingData = inDrawingData;
            summarySlideTransform = inSummarySlideTransform;

            BirdData authorBird = GameDataManager.Instance.GetBird(drawingData.author);
            if(authorBird == null)
            {
                Debug.LogError("Could not initialize drawing summary section because drawing bird["+drawingData.author.ToString()+"] was not mapped in Colour Manager.");
                return;
            }
            _author = drawingData.author;
            authorImage.sprite = authorBird.faceSprite;
            authorNameText.color = authorBird.colour;
            authorNameText.text = SettingsManager.Instance.GetPlayerName(inDrawingData.author);

            goldStarDetectionArea.Initialize(inDrawingData.author, round, caseID);
            slideTimeModifierDecrementVisual.Initialize(timeModifierDecrement);
        }

        public override void Show()
        {
            base.Show();
            Camera mainCamera = Camera.main;
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(drawingReferencePoint.position);
            worldPosition = new Vector3(worldPosition.x, worldPosition.y, 0f);
            GameObject drawingHolder = Instantiate(new GameObject(), summarySlideTransform);
            drawingHolder.transform.position = worldPosition;

            if(drawingData != null)
            {
                Vector3 drawingScale = new Vector3(drawingSize, drawingSize, 1f);
                GameManager.Instance.playerFlowManager.createDrawingVisuals(drawingData, drawingHolder.transform, drawingHolder.transform.position + drawingOffset, drawingScale, drawingSize);
            }
        }
    }
}
