using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static ChickenScratch.TaskData;

namespace ChickenScratch
{
    public class DrawingCaseFolder : CaseFolder
    {
        [SerializeField]
        private DrawingController drawingBoard;

        [SerializeField]
        private TMPro.TMP_Text promptText;

        [SerializeField]
        private CaseWordCategoryVisual caseWordCategoryVisual;

        private TaskModifier drawingBoxModifier;

        public void Initialize(string inPromptText, TaskData.TaskModifier inDrawingBoxModifier, UnityAction inTimeCompleteAction)
        {
            drawingBoxModifier = inDrawingBoxModifier;
            promptText.text = inPromptText;
            promptText.gameObject.SetActive(true);
            timeCompleteAction = inTimeCompleteAction;
            RegisterToTimer(timeCompleteAction);
        }

        public override void Show(Color inFolderColour, float taskTime, float currentModifier, float maxModifierValue, float modifierDecrement)
        {
            base.Show(inFolderColour, taskTime, currentModifier, maxModifierValue, modifierDecrement);

            if (SettingsManager.Instance.GetSetting("stickies"))
            {
                TutorialSticky drawingToolsSticky1 = GameManager.Instance.playerFlowManager.instructionRound.drawingToolsSticky;
                TutorialSticky drawingToolsSticky2 = GameManager.Instance.playerFlowManager.instructionRound.drawingToolsSticky2;
                if (!drawingToolsSticky1.hasBeenPlaced)
                {
                    drawingToolsSticky1.Queue(true);
                }
                if (!drawingToolsSticky2.hasBeenPlaced)
                {
                    drawingToolsSticky2.Queue(true);
                }
            }
            drawingBoard.gameObject.SetActive(true);
            //drawingBoard.initialize();
            
            drawingBoard.SetDrawingBoxType(drawingBoxModifier);
        }

        public override void Hide()
        {
            base.Hide();
            drawingBoard.clearVisuals(true);
            caseWordCategoryVisual.Hide();
        }

        public override bool HasStarted()
        {
            return drawingBoard.hasVisuals();
        }

        public List<DrawingLineData> GetVisuals()
        {
            return drawingBoard.getDrawingVisuals();
        }

        public void ShowCategory(WordCategoryData wordCategory)
        {
            caseWordCategoryVisual.Initialize(wordCategory);
            caseWordCategoryVisual.Show();
        }
    }
}

