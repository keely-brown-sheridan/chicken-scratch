using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class PromptSlideContents : SlideContents
    {
        [SerializeField]
        private TMP_Text originalPromptReminderText;
        [SerializeField]
        private TMP_Text playerPromptText;
        [SerializeField]
        private Image authorImage;
        [SerializeField]
        private TMP_Text authorNameText;

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

        public void Initialize(PlayerTextInputData promptData, string correctPrompt, float inDuration)
        {
            duration = inDuration;
            timeActive = 0f;
            Bird authorBird = ColourManager.Instance.birdMap[promptData.author];
            authorImage.sprite = authorBird.faceSprite;
            authorNameText.color = authorBird.colour;

            originalPromptReminderText.text = correctPrompt;
            playerPromptText.text = promptData.text;
            playerPromptText.color = authorBird.colour;
        }
    }
}
