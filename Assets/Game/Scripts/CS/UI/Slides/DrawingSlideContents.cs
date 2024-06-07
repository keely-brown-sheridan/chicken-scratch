using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class DrawingSlideContents : SlideContents
    {
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

        private float duration;
        private float timeActive = 0f;

        private void Update()
        {
            if (active)
            {
                timeActive += Time.deltaTime;
                if (timeActive > duration)
                {
                    isComplete = true;
                }
            }
        }

        public void Initialize(DrawingData drawingData, string correctPrompt, float inDuration)
        {
            duration = inDuration;
            timeActive = 0f;
            Vector3 drawingScale = new Vector3(drawingSize, drawingSize, 1f);
            GameManager.Instance.playerFlowManager.createDrawingVisuals(drawingData, drawingHolder, drawingHolder.position + drawingOffset, drawingScale, drawingSize);

            Bird authorBird = ColourManager.Instance.birdMap[drawingData.author];
            authorImage.sprite = authorBird.faceSprite;
            authorNameText.color = authorBird.colour;

            originalPromptReminderText.text = correctPrompt;
        }
    }
}
