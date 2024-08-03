using ChickenScratch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ChickenScratch.CaseEmail;

public class ArgumentPanel : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> taskEmailSectionPrefabs;

    [SerializeField]
    private Transform leftTaskVisualHolder, middleTaskVisualHolder, rightTaskVisualHolder;

    [SerializeField]
    private GameObject choiceButtonObject;

    [SerializeField]
    private Image accuserFaceImage;

    [SerializeField]
    private Image accusedFaceImage;

    [SerializeField]
    private Image backgroundImage;



    private ColourManager.BirdName accuserBird, accusedBird, currentBird;
    private Dictionary<CaseEmailTaskType, GameObject> taskEmailSectionPrefabMap = new Dictionary<CaseEmailTaskType, GameObject>();
    private Dictionary<ColourManager.BirdName, PlayerReviewData> playerReviewMap = new Dictionary<ColourManager.BirdName, PlayerReviewData>();

    public void Initialize(ColourManager.BirdName inAccusedBird, ColourManager.BirdName inAccuserBird)
    {
        accuserBird = inAccuserBird;
        accusedBird = inAccusedBird;
        taskEmailSectionPrefabMap.Clear();
        foreach (GameObject taskEmailSectionPrefab in taskEmailSectionPrefabs)
        {
            CaseEmailSection caseEmailSection = taskEmailSectionPrefab.GetComponent<CaseEmailSection>();
            taskEmailSectionPrefabMap.Add(caseEmailSection.taskType, taskEmailSectionPrefab);
        }

        PlayerReviewData currentPlayerData;
        playerReviewMap.Clear();
        foreach (EndgameCaseData caseData in GameManager.Instance.playerFlowManager.slidesRound.caseDataMap.Values)
        {
            foreach (KeyValuePair<int, EndgameTaskData> task in caseData.taskDataMap)
            {
                if(task.Value.assignedPlayer != accusedBird && task.Value.assignedPlayer != accuserBird)
                {
                    continue;
                }

                if (!playerReviewMap.ContainsKey(task.Value.assignedPlayer))
                {
                    playerReviewMap.Add(task.Value.assignedPlayer, new PlayerReviewData());
                }
                currentPlayerData = playerReviewMap[task.Value.assignedPlayer];
                currentPlayerData.caseIndices.Add(caseData.identifier);
                currentPlayerData.caseTaskMap.Add(caseData.identifier, task.Key);
            }
        }

        currentPlayerData = playerReviewMap[accusedBird];

        if (currentPlayerData.caseIndices.Count != 0)
        {
            currentPlayerData.currentCaseIndex = 0;
        }

        currentPlayerData.playerName = GameManager.Instance.playerFlowManager.playerNameMap[accusedBird];
        BirdData playerBird = GameDataManager.Instance.GetBird(accusedBird);
        if (playerBird != null)
        {
            currentPlayerData.playerColour = playerBird.colour;
            currentPlayerData.faceSprite = playerBird.faceSprite;
            currentPlayerData.bgColour = playerBird.bgColour;
            accusedFaceImage.sprite = playerBird.faceSprite;
            backgroundImage.color = currentPlayerData.bgColour;
        }

        currentPlayerData = playerReviewMap[accuserBird];

        if (currentPlayerData.caseIndices.Count != 0)
        {
            currentPlayerData.currentCaseIndex = 0;
        }

        currentPlayerData.playerName = GameManager.Instance.playerFlowManager.playerNameMap[accuserBird];
        playerBird = GameDataManager.Instance.GetBird(accuserBird);
        if (playerBird != null)
        {
            currentPlayerData.playerColour = playerBird.colour;
            currentPlayerData.faceSprite = playerBird.faceSprite;
            currentPlayerData.bgColour = playerBird.bgColour;
            accuserFaceImage.sprite = playerBird.faceSprite;
        }

        currentBird = accusedBird;

        UpdateCurrentCase();
    }

    private void UpdateCurrentCase()
    {
        //Clear previous task visualizations
        ClearTaskVisualizations();

        PlayerReviewData reviewData = playerReviewMap[currentBird];
        backgroundImage.color = reviewData.bgColour;

        //Get the current task
        int caseIndex = reviewData.caseIndices[reviewData.currentCaseIndex];
        int taskIndex = reviewData.caseTaskMap[caseIndex];
        EndgameCaseData caseData = GameManager.Instance.playerFlowManager.slidesRound.caseDataMap[caseIndex];
        EndgameTaskData taskData = caseData.taskDataMap[taskIndex];
        CreateTaskVisualization(caseData, taskData, middleTaskVisualHolder);

        //Get the previous task if there was one
        if (caseData.taskDataMap.ContainsKey(taskIndex - 1))
        {
            CreateTaskVisualization(caseData, caseData.taskDataMap[taskIndex - 1], leftTaskVisualHolder);
        }
        else
        {
            CreateIntroVisualization(caseData, leftTaskVisualHolder);
        }

        //Get the next task if there was one
        if (caseData.taskDataMap.ContainsKey(taskIndex + 1))
        {
            CreateTaskVisualization(caseData, caseData.taskDataMap[taskIndex + 1], rightTaskVisualHolder);
        }
    }

    private void CreateTaskVisualization(EndgameCaseData caseData, EndgameTaskData taskData, Transform parent)
    {
        switch (taskData.taskType)
        {
            case TaskData.TaskType.base_drawing:
            case TaskData.TaskType.copy_drawing:
            case TaskData.TaskType.add_drawing:
            case TaskData.TaskType.prompt_drawing:
            case TaskData.TaskType.compile_drawing:
            case TaskData.TaskType.blender_drawing:
                GameObject drawingCaseEmailSectionObject = Instantiate(taskEmailSectionPrefabMap[CaseEmailTaskType.drawing], parent);
                DrawingCaseEmailSection drawingCaseEmailSection = drawingCaseEmailSectionObject.GetComponent<DrawingCaseEmailSection>();
                drawingCaseEmailSection.Initialize(taskData.drawingData, taskData.ratingData, 1f);
                break;
            case TaskData.TaskType.prompting:
                GameObject promptCaseEmailSectionObject = Instantiate(taskEmailSectionPrefabMap[CaseEmailTaskType.prompt], parent);
                PromptCaseEmailSection promptCaseEmailSection = promptCaseEmailSectionObject.GetComponent<PromptCaseEmailSection>();
                promptCaseEmailSection.Initialize(taskData.promptData, taskData.ratingData);
                break;
            case TaskData.TaskType.morph_guessing:
            case TaskData.TaskType.base_guessing:
            case TaskData.TaskType.competition_guessing:
            case TaskData.TaskType.binary_guessing:
                GameObject guessCaseEmailSectionObject = Instantiate(taskEmailSectionPrefabMap[CaseEmailTaskType.guess], parent);
                GuessCaseEmailSection guessCaseEmailSection = guessCaseEmailSectionObject.GetComponent<GuessCaseEmailSection>();
                guessCaseEmailSection.Initialize(caseData.correctWordIdentifierMap, caseData.guessData, taskData.ratingData);
                break;

        }
    }

    private void CreateIntroVisualization(EndgameCaseData caseData, Transform parent)
    {
        GameObject originalCaseEmailSectionObject = Instantiate(taskEmailSectionPrefabMap[CaseEmailTaskType.original], parent);
        OriginalCaseEmailSection originalCaseEmailSection = originalCaseEmailSectionObject.GetComponent<OriginalCaseEmailSection>();
        CaseWordData correctPrefix = GameDataManager.Instance.GetWord(caseData.correctWordIdentifierMap[1]);
        CaseWordData correctNoun = GameDataManager.Instance.GetWord(caseData.correctWordIdentifierMap[2]);
        originalCaseEmailSection.Initialize(correctPrefix.value, correctNoun.value);
    }

    private void ClearTaskVisualizations()
    {
        List<Transform> transformsToDestroy = new List<Transform>();
        foreach (Transform child in leftTaskVisualHolder)
        {
            transformsToDestroy.Add(child);
        }
        foreach (Transform child in middleTaskVisualHolder)
        {
            transformsToDestroy.Add(child);
        }
        foreach (Transform child in rightTaskVisualHolder)
        {
            transformsToDestroy.Add(child);
        }
        for (int i = transformsToDestroy.Count - 1; i >= 0; i--)
        {
            Destroy(transformsToDestroy[i].gameObject);
        }
    }

    public void ChooseAccuser()
    {
        currentBird = accuserBird;

        //Update the background colour
        backgroundImage.color = playerReviewMap[currentBird].bgColour;
        UpdateCurrentCase();
    }

    public void ChooseAccused()
    {
        currentBird = accusedBird;
        backgroundImage.color = playerReviewMap[currentBird].bgColour;
        UpdateCurrentCase();
    }

    public void ShiftNextCase()
    {
        PlayerReviewData currentReviewData = playerReviewMap[currentBird];
        currentReviewData.currentCaseIndex++;
        if(currentReviewData.currentCaseIndex >= currentReviewData.caseIndices.Count)
        {
            currentReviewData.currentCaseIndex = 0;
        }
        UpdateCurrentCase();
    }

    public void ShiftPreviousCase()
    {
        PlayerReviewData currentReviewData = playerReviewMap[currentBird];
        currentReviewData.currentCaseIndex--;
        if(currentReviewData.currentCaseIndex < 0)
        {
            currentReviewData.currentCaseIndex = currentReviewData.caseIndices.Count - 1;
        }
        UpdateCurrentCase();
    }

    public void ShowChoice()
    {
        PlayerReviewData currentReviewData = playerReviewMap[currentBird];
        int caseID = currentReviewData.caseIndices[currentReviewData.currentCaseIndex];
        int taskID = currentReviewData.caseTaskMap[caseID];
        GameManager.Instance.gameDataHandler.CmdShowAccuseChoice(caseID, taskID);
    }

    public void HideChoiceButton()
    {
        choiceButtonObject.SetActive(false);
    }

    public void ShowChoiceButton()
    {
        choiceButtonObject.SetActive(true);
    }
}
