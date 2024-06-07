using ChickenScratch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddingCaseFolder : CaseFolder
{
    [SerializeField]
    private float drawingScalingFactor;

    [SerializeField]
    private Vector3 drawingOffset;

    [SerializeField]
    private DrawingController drawingBoard;

    [SerializeField]
    private TMPro.TMP_Text promptText;

    public void Initialize(DrawingData drawingData, string prompt)
    {
        //Add the drawing lines from the drawing data to the drawingBoard
        drawingBoard.AddDrawingLines(drawingData);

        promptText.text = prompt;
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
