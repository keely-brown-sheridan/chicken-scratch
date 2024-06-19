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
        [SerializeField]
        private TMPro.TMP_Text playerNameText;

        public void Initialize(DrawingData drawingData, PlayerRatingData ratingData, float drawingRatio)
        {
            Bird drawingBird = ColourManager.Instance.birdMap[drawingData.author];
            playerImage.sprite = drawingBird.faceSprite;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(drawingParent.position);
            worldPosition = new Vector3(worldPosition.x, worldPosition.y, 0f);
            drawingParent.position = worldPosition;

            playerNameText.color = drawingBird.colour;
            playerNameText.text = SettingsManager.Instance.GetPlayerName(drawingData.author);

            Vector3 drawingScale = new Vector3(drawingSize* drawingRatio, drawingSize*drawingRatio, 0f);
            GameManager.Instance.playerFlowManager.createDrawingVisuals(drawingData, drawingParent, drawingParent.position, drawingScale, drawingSize* drawingRatio);

            SetRating(ratingData.likeCount);
        }
    }
}
