using Steamworks;
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
        private TaskModifier drawingTypeModifier;

        public void Initialize(int caseID, int round, string inPromptText, List<TaskData.TaskModifier> taskModifiers, UnityAction inTimeCompleteAction)
        {
            casePlayerTabs.Initialize(round, caseID);
            SetCaseTypeVisuals(caseID);
            SetCertificationSlots(caseID);
            drawingTypeModifier = TaskModifier.invalid;
            drawingBoxModifier = TaskModifier.standard;
            foreach (TaskModifier modifier in taskModifiers)
            {
                switch (modifier)
                {
                    case TaskModifier.shrunk:
                    case TaskModifier.thirds_first:
                    case TaskModifier.thirds_second:
                    case TaskModifier.thirds_third:
                    case TaskModifier.top:
                    case TaskModifier.bottom:
                    case TaskModifier.top_left:
                    case TaskModifier.top_right:
                    case TaskModifier.bottom_left:
                    case TaskModifier.bottom_right:
                    case TaskModifier.expanding:
                    case TaskModifier.collapsing:
                        drawingBoxModifier = modifier;
                        break;
                    case TaskModifier.blind:
                        drawingTypeModifier = modifier;
                        break;
                }
            }

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
            drawingBoard.SetTimeInTask(taskTime);
            drawingBoard.SetDrawingBoxType(drawingBoxModifier);
            drawingBoard.SetDrawingType(drawingTypeModifier);
            drawingBoard.gameObject.SetActive(true);
            //drawingBoard.initialize();
            
            
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

