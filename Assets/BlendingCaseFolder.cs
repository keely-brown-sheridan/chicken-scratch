using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChickenScratch.TaskData;
using UnityEngine.Events;
using UnityEngine;

namespace ChickenScratch
{
    public class BlendingCaseFolder : CaseFolder
    {
        [SerializeField]
        private DrawingsContainer drawingContainer1;

        [SerializeField]
        private DrawingsContainer drawingContainer2;

        [SerializeField]
        private float drawingScalingFactor;

        [SerializeField]
        private Vector3 drawingOffset;

        [SerializeField]
        private DrawingController drawingBoard;

        [SerializeField]
        private CaseWordCategoryVisual caseWordCategoryVisual;

        private TaskModifier drawingBoxModifier;
        private TaskModifier drawingTypeModifier;

        public void Initialize(int round, DrawingData drawingData1, DrawingData drawingData2, List<TaskModifier> taskModifiers, UnityAction inTimeCompleteAction)
        {
            casePlayerTabs.Initialize(round, drawingData1.caseID);
            SetCaseTypeVisuals(drawingData1.caseID);
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
                        drawingBoxModifier = modifier;
                        break;
                    case TaskModifier.blind:
                        drawingTypeModifier = modifier;
                        break;
                }
            }
            drawingContainer1.Show(drawingData1, drawingScalingFactor, taskModifiers);
            drawingContainer2.Show(drawingData2, drawingScalingFactor, taskModifiers);
            timeCompleteAction = inTimeCompleteAction;
            RegisterToTimer(inTimeCompleteAction);
        }

        public override void Show(Color inFolderColour, float taskTime, float currentModifier, float maxModifierValue, float modifierDecrement)
        {
            base.Show(inFolderColour, taskTime, currentModifier, maxModifierValue, modifierDecrement);
            drawingBoard.SetDrawingBoxType(drawingBoxModifier);
            drawingBoard.SetDrawingType(drawingTypeModifier);
            drawingBoard.gameObject.SetActive(true);
            //drawingBoard.initialize();
        }

        public override void Hide()
        {
            base.Hide();
            drawingBoard.clearVisuals(true);
            HidePreviousDrawings();
            caseWordCategoryVisual.Hide();
        }

        public void HidePreviousDrawings()
        {
            drawingContainer1.HidePreviousDrawings();
            drawingContainer2.HidePreviousDrawings();
        }

        public override bool HasStarted()
        {
            return drawingBoard.hasVisuals();
        }

        public List<DrawingLineData> GetVisuals()
        {
            return drawingBoard.getDrawingVisuals();
        }

        public void ShowCategory(WordCategoryData wordCategoryData)
        {
            caseWordCategoryVisual.Initialize(wordCategoryData);
            caseWordCategoryVisual.Show();
        }
    }
}
