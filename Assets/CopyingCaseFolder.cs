using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

        public void Initialize(DrawingData drawingData)
        {
            copyingContainer.Show(drawingData, drawingScalingFactor, drawingOffset);
        }

        public override void Show(Color inFolderColour, float taskTime, float currentModifier, float modifierDecrement)
        {
            base.Show(inFolderColour, taskTime, currentModifier, modifierDecrement);
            drawingBoard.initialize();
            drawingBoard.gameObject.SetActive(true);
            drawingBoard.open();
        }

        public override void Hide()
        {
            base.Hide();
            drawingBoard.clearVisuals();
            HidePreviousDrawings();
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
    }

}
