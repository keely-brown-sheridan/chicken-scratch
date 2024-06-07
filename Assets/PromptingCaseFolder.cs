using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        public void Initialize(DrawingData drawingData)
        {
            promptingContainer.Show(drawingData, drawingScalingFactor, drawingOffset);
        }
        public override void Show(Color inFolderColour, float taskTime, float currentModifier, float modifierDecrement)
        {
            base.Show(inFolderColour, taskTime, currentModifier, modifierDecrement);
        }
        public override void Hide()
        {
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
    }

}
