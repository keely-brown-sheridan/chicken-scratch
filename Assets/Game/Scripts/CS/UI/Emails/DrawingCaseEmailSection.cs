using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class DrawingCaseEmailSection : CaseEmailSection
    {
        [SerializeField]
        private Transform drawingParent;

        [SerializeField]
        private Image playerImage;

        [SerializeField]
        private float drawingSize;

        public void Initialize(DrawingData drawingData, PlayerRatingData ratingData)
        {
            Bird drawingBird = ColourManager.Instance.birdMap[drawingData.author];
            playerImage.sprite = drawingBird.faceSprite;

            Vector3 drawingScale = new Vector3(drawingSize, drawingSize, 0f);
            GameManager.Instance.playerFlowManager.createDrawingVisuals(drawingData, drawingParent, drawingParent.position, drawingScale, drawingSize);

            SetRating(ratingData.likeCount);
        }
    }
}
