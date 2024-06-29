using ChickenScratch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

    [SerializeField]
    private CaseWordCategoryVisual caseWordCategoryVisual;

    private TaskData.TaskModifier drawingBoxModifier;

    public void Initialize(DrawingData drawingData, string prompt, TaskData.TaskModifier inDrawingBoxModifier, UnityAction inTimeCompleteAction)
    {
        drawingBoxModifier = inDrawingBoxModifier;

        foreach(DrawingLineData line in drawingData.visuals)
        {
            line.locked = true;
        }

        //Add the drawing lines from the drawing data to the drawingBoard
        drawingBoard.AddDrawingLines(drawingData);

        promptText.text = prompt;
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

    public void ShowCategory(WordCategoryData wordCategoryData)
    {
        caseWordCategoryVisual.Initialize(wordCategoryData);
        caseWordCategoryVisual.Show();
    }
}
