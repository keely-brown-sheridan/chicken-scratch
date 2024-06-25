using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static ChickenScratch.TaskData;

namespace ChickenScratch
{
    public class CopyingCaseFolder : CaseFolder
    {
        [SerializeField]
        private DrawingsContainer copyingContainer;

        [SerializeField]
        private float drawingScalingFactor;

        [SerializeField]
        private Vector3 drawingOffset;

        [SerializeField]
        private DrawingController drawingBoard;

        [SerializeField]
        private CaseWordCategoryVisual caseWordCategoryVisual;

        private TaskModifier drawingBoxModifier;

        public void Initialize(DrawingData drawingData, TaskModifier inDrawingBoxModifier, UnityAction inTimeCompleteAction)
        {
            drawingBoxModifier = inDrawingBoxModifier;
            copyingContainer.Show(drawingData, drawingScalingFactor, drawingOffset);
            timeCompleteAction = inTimeCompleteAction;
            RegisterToTimer(inTimeCompleteAction);
        }

        public override void Show(Color inFolderColour, float taskTime, float currentModifier, float maxModifierValue, float modifierDecrement)
        {
            base.Show(inFolderColour, taskTime, currentModifier, maxModifierValue, modifierDecrement);
            drawingBoard.gameObject.SetActive(true);
            //drawingBoard.initialize();
            
            drawingBoard.SetDrawingBoxType(drawingBoxModifier);
        }

        public override void Hide()
        {
            base.Hide();
            drawingBoard.clearVisuals();
            HidePreviousDrawings();
            caseWordCategoryVisual.Hide();
        }

        public void HidePreviousDrawings()
        {
            copyingContainer.HidePreviousDrawings();
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
