using UnityEngine;

namespace ChickenScratch
{
    public class IntroSlideContents : SlideContents
    {
        [SerializeField]
        private TMPro.TMP_Text originalPromptText;
        [SerializeField]
        private CaseTypeSlideVisualizer caseTypeSlideVisualizer;

        private float duration;
        private float timeActive = 0f;
        private void Update()
        {
            if(active)
            {
                timeActive += Time.deltaTime * GameManager.Instance.playerFlowManager.slidesRound.slideSpeed;
                if(timeActive > duration)
                {
                    isComplete = true;
                }
            }
        }

        public void Initialize(string prefix, string noun, int caseID,  float inDuration)
        {
            duration = inDuration;
            timeActive = 0f;
            originalPromptText.text = SettingsManager.Instance.CreatePromptText(prefix, noun);
            EndgameCaseData currentCase = GameManager.Instance.playerFlowManager.slidesRound.caseDataMap[caseID];
            caseTypeSlideVisualizer.Initialize(currentCase.caseTypeColour, currentCase.caseTypeName);
        }
    }
}
