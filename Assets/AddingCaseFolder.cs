using ChickenScratch;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static ChickenScratch.TaskData;

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
    private TaskData.TaskModifier drawingTypeModifier;

    public void Initialize(int round, DrawingData drawingData, string prompt, List<TaskModifier> taskModifiers, UnityAction inTimeCompleteAction)
    {
        casePlayerTabs.Initialize(round, drawingData.caseID);
        SetCaseTypeVisuals(drawingData.caseID);
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

    public void ShowCategory(WordCategoryData wordCategoryData)
    {
        caseWordCategoryVisual.Initialize(wordCategoryData);
        caseWordCategoryVisual.Show();
    }
}
