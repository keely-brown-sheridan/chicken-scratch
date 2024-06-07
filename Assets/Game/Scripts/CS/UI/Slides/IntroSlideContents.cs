using UnityEngine;

namespace ChickenScratch
{
    public class IntroSlideContents : SlideContents
    {
        [SerializeField]
        private TMPro.TMP_Text originalPromptText;

        private float duration;
        private float timeActive = 0f;
        private void Update()
        {
            if(active)
            {
                timeActive += Time.deltaTime;
                if(timeActive > duration)
                {
                    isComplete = true;
                }
            }
        }

        public void Initialize(string originalPrompt, float inDuration)
        {
            duration = inDuration;
            timeActive = 0f;
            originalPromptText.text = originalPrompt;
        }
    }
}
