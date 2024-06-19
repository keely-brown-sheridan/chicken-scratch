using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace ChickenScratch
{
    public class PromptingCaseFolder : CaseFolder
    {
        [SerializeField]
        private TMPro.TMP_InputField promptInputField;
        [SerializeField]
        private DrawingsContainer promptingContainer;

        [SerializeField]
        private float drawingScalingFactor;

        [SerializeField]
        private Vector3 drawingOffset;

        public void Initialize(DrawingData drawingData, UnityAction inTimeCompleteAction)
        {
            promptingContainer.Show(drawingData, drawingScalingFactor, drawingOffset);
            timeCompleteAction = inTimeCompleteAction;
            RegisterToTimer(inTimeCompleteAction);
        }
        public override void Show(Color inFolderColour, float taskTime, float currentModifier, float modifierDecrement)
        {
            GameManager.Instance.playerFlowManager.drawingRound.caseFolderOnStartAction = QueueSelectInputField;
            base.Show(inFolderColour, taskTime, currentModifier, modifierDecrement);
        }
        public override void Hide()
        {
            GameManager.Instance.playerFlowManager.drawingRound.caseFolderOnStartAction = null;
            base.Hide();
            promptInputField.text = "";
            HidePreviousDrawings();
        }

        public void HidePreviousDrawings()
        {
            promptingContainer.HidePreviousDrawings();
        }

        public override bool HasStarted()
        {
            return promptInputField.text != "";
        }

        public string GetPromptText()
        {
            return promptInputField.text;
        }

        public void ToUpperPromptText()
        {
            promptInputField.text = promptInputField.text.ToUpper();
        }

        public void QueueSelectInputField()
        {
            StartCoroutine(SelectInputField());
        }

        IEnumerator SelectInputField()
        {
            yield return new WaitForEndOfFrame();
            promptInputField.ActivateInputField();
        }
    }

}
