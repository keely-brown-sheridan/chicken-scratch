using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ChickenScratch.ColourManager;

namespace ChickenScratch
{
    public class GameManager : Singleton<GameManager>
    {
        public enum GameScene
        {
            lobby, game, theater
        }
        public string versionNumber = "0.01";
        public GameScene currentGameScene;
        public Color workerColour, traitorColour;
        public GameObject linePrefab;
        public HideCursorOnHover cursorHider;
        public GameObject submitBtn;
        public Sprite folderFailSprite, folderSuccessSprite;

        public GameFlowManager gameFlowManager;
        public PlayerFlowManager playerFlowManager;

        public DrawingTestManager drawingTestManager;
        public string gameID;
        public DCManager dcManager;

        public GameDataHandler gameDataHandler;

        public PauseModTools pauseModTools;

        // Start is called before the first frame update
        void Start()
        {
            if (!gameFlowManager)
            {
                gameFlowManager = FindObjectOfType<GameFlowManager>();
            }

            if (NetworkServer.connections.Count > 0)
            {
                if (gameFlowManager)
                {
                    gameFlowManager.gameObject.SetActive(true);
                }

            }
        }

        public void GenerateTestingData()
        {
            playerFlowManager.playerNameMap.Add(BirdName.red, "beebodeebo");
            playerFlowManager.playerNameMap.Add(BirdName.blue, "beebodeebo");
            playerFlowManager.playerNameMap.Add(BirdName.teal, "beebodeebo");
            playerFlowManager.playerNameMap.Add(BirdName.green, "beebodeebo");

            SettingsManager.Instance.birdName = BirdName.red;
            SettingsManager.Instance.AssignBirdToPlayer(ColourManager.BirdName.red, "beebodeebo");
            SettingsManager.Instance.AssignBirdToPlayer(ColourManager.BirdName.blue, "beebodeebo");
            SettingsManager.Instance.AssignBirdToPlayer(ColourManager.BirdName.teal, "beebodeebo");
            SettingsManager.Instance.AssignBirdToPlayer(ColourManager.BirdName.green, "beebodeebo");
            GameDataManager.Instance.RefreshWords(new List<CaseWordData>());

            //create sample case and task and add player
            PlayerRatingData ratingData = new PlayerRatingData() { likeCount = 3, dislikeCount = 1 };

            EndgameTaskData testTask = new EndgameTaskData() { ratingData = ratingData, assignedPlayer = BirdName.red };
            testTask.taskType = TaskData.TaskType.base_guessing;
            testTask.ratingData = new PlayerRatingData() { likeCount = 1, target = BirdName.red };
            testTask.assignedPlayer = BirdName.red;

            CaseScoringData testScoringData = new CaseScoringData() { bonusBirdbucks = 2, nounBirdbucks = 4, prefixBirdbucks = 4, scoreModifier = 1.0f };
            PlayerTextInputData promptData = new PlayerTextInputData() { author = BirdName.blue, text = "huh what?" };
            PlayerTextInputData promptData2 = new PlayerTextInputData() { author = BirdName.teal, text = "huh what?" };
            PlayerTextInputData promptData3 = new PlayerTextInputData() { author = BirdName.green, text = "huh what?" };
            EndgameTaskData testTask2 = new EndgameTaskData() { assignedPlayer = BirdName.blue, promptData = promptData, taskType = TaskData.TaskType.prompting, ratingData = ratingData };
            EndgameTaskData testTask3 = new EndgameTaskData() { assignedPlayer = BirdName.teal, promptData = promptData2, taskType = TaskData.TaskType.prompting, ratingData = ratingData };
            EndgameTaskData testTask4 = new EndgameTaskData() { assignedPlayer = BirdName.green, promptData = promptData3, taskType = TaskData.TaskType.prompting, ratingData = ratingData };
            GuessData testGuess = new GuessData() { author = ColourManager.BirdName.red, prefix = "ATTACHED", noun = "AARDVARK", round = 1, timeTaken = 0f };

            EndgameCaseData testCase = new EndgameCaseData() { identifier = 1, caseTypeColour = Color.cyan, caseTypeName = "Corners", guessData = testGuess, scoreModifier = 1.5f, scoringData = testScoringData };

            testCase.taskDataMap.Add(1, testTask2);
            testCase.taskDataMap.Add(2, testTask3);
            testCase.taskDataMap.Add(3, testTask4);
            testCase.taskDataMap.Add(4, testTask);
            testCase.correctWordIdentifierMap = new Dictionary<int, string>() { { 1, "prefixes-NEUTRAL-ATTACHED" }, { 2, "nouns-ANIMAL-AARDVARK" } };
            playerFlowManager.slidesRound.caseDataMap.Add(1, testCase);
        }
    }
}