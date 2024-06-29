using UnityEngine;

namespace ChickenScratch
{
    public class IntroSlideContents : SlideContents
    {
        [SerializeField]
        private TMPro.TMP_Text originalPromptText;
        [SerializeField]
        private CaseTypeSlideVisualizer caseTypeSlideVisualizer;

        [SerializeField]
        private TMPro.TMP_Text caseProgressReminderText;

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

        public void Initialize(string prefix, string noun, int caseID,  float inDuration, string caseReminder)
        {
            duration = inDuration;
            timeActive = 0f;
            originalPromptText.text = SettingsManager.Instance.CreatePromptText(prefix, noun);
            caseProgressReminderText.text = caseReminder;
            if (!GameManager.Instance.playerFlowManager.slidesRound.caseDataMap.ContainsKey(caseID))
            {
                Debug.LogError("ERROR[Initialize]: Could not initialize case type slide visualizer because caseDataMap did not contain caseID["+caseID.ToString()+"]");
            }
            EndgameCaseData currentCase = GameManager.Instance.playerFlowManager.slidesRound.caseDataMap[caseID];
            caseTypeSlideVisualizer.Initialize(currentCase.caseTypeColour, currentCase.caseTypeName);
        }

        public override void Show()
        {
            GameManager.Instance.playerFlowManager.slidesRound.HideCaseDetails();
            base.Show();
        }
    }
}
