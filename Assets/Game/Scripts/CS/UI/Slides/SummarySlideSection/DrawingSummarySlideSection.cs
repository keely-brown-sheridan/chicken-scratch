using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class DrawingSummarySlideSection : SummarySlideSection
    {
        [SerializeField]
        private Transform drawingReferencePoint;

        [SerializeField]
        private Vector3 drawingOffset;

        [SerializeField]
        private float drawingSize;

        public Transform summarySlideTransform;

        private DrawingData drawingData;

        private void Start()
        {
            Show();
        }

        public void Initialize(DrawingData inDrawingData, Transform inSummarySlideTransform)
        {
            drawingData = inDrawingData;
            summarySlideTransform = inSummarySlideTransform;

            Bird authorBird = ColourManager.Instance.birdMap[drawingData.author];
            authorImage.sprite = authorBird.faceSprite;
            authorNameText.color = authorBird.colour;
            //Add the name?
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
