using Steamworks;
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

        [SerializeField]
        private CaseWordCategoryVisual caseWordCategoryVisual;

        [SerializeField]
        private int basePromptingLength = 24;

        public void Initialize(int round, DrawingData drawingData, UnityAction inTimeCompleteAction, List<TaskData.TaskModifier> taskModifiers, int promptingLength = -1)
        {
            if(promptingLength < 1)
            {
                promptInputField.characterLimit = basePromptingLength;
            }
            else
            {
                promptInputField.characterLimit = promptingLength;
            }
            casePlayerTabs.Initialize(round, drawingData.caseID);
            SetCaseTypeVisuals(drawingData.caseID);
            promptingContainer.Show(drawingData, drawingScalingFactor, taskModifiers);
            timeCompleteAction = inTimeCompleteAction;
            RegisterToTimer(inTimeCompleteAction);
        }
        public override void Show(Color inFolderColour, float taskTime, float currentModifier, float maxModifierValue, float modifierDecrement)
        {
            GameManager.Instance.playerFlowManager.drawingRound.caseFolderOnStartAction = QueueSelectInputField;
            base.Show(inFolderColour, taskTime, currentModifier, maxModifierValue, modifierDecrement);
        }
        public override void Hide()
        {
            GameManager.Instance.playerFlowManager.drawingRound.caseFolderOnStartAction = null;
            base.Hide();
            promptInputField.text = "";
            HidePreviousDrawings();
            caseWordCategoryVisual.Hide();
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

        public void ShowCategory(WordCategoryData wordCategoryData)
        {
            caseWordCategoryVisual.Initialize(wordCategoryData);
            caseWordCategoryVisual.Show();
        }
    }

}
