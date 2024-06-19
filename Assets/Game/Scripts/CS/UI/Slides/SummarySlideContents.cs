using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ChickenScratch
{
    public class SummarySlideContents : SlideContents
    {
        [SerializeField]
        private Transform summarySectionsHolder;

        [SerializeField]
        private Transform sectionReferencePositionTransform;

        [SerializeField]
        private List<GameObject> summarySectionPrefabs;

        [SerializeField]
        private TMPro.TMP_Text originalPromptText;

        [SerializeField]
        private TMPro.TMP_Text baseScoreText, finalScoreModifierText, finalScoreText;
        [SerializeField]
        private float horizontalSectionDistance, verticalSectionDistance;

        [SerializeField]
        private int maxColumnsPerRow;

        [SerializeField]
        private CaseTypeSlideVisualizer caseTypeSlideVisualizer;

        private Dictionary<SlideTypeData.SlideType, GameObject> summarySectionPrefabMap = new Dictionary<SlideTypeData.SlideType, GameObject>();

        private List<SummarySlideSection> summarySections = new List<SummarySlideSection>();
        private float duration;
        private float timeActive = 0f;
        private int currentRow = 0;
        private int currentColumn = 0;

        private void Start()
        {
            //DrawingData drawingData = new DrawingData();
            //drawingData.author = ColourManager.BirdName.red;
            //drawingData.visuals = new List<DrawingLineData>() { new DrawingLineData() { lineColour = DrawingLineData.LineColour.Colour, author = ColourManager.BirdName.red, positions = new List<Vector3>() { { new Vector3(1f,1f,0f) }, { new Vector3(3f, 1f, 0f) }, { new Vector3(6f, 1f, 0f) }, { new Vector3(3f, 3f, 0f) } } } };
            //Initialize("yup nope", 100f);
            //Dictionary<int, string> guessesMap = new Dictionary<int, string>() { { 1, "yup" }, { 2, "nope" } };
            //Dictionary<int, CaseWordData> correctWordsMap = new Dictionary<int, CaseWordData>() { { 1, new CaseWordData("yup", null, 1) }, { 2, new CaseWordData("yup", null, 1) } };
            //AddDrawing(drawingData, GameManager.Instance.playerFlowManager.slidesRound.transform);
            //AddGuess(ColourManager.BirdName.red, guessesMap, correctWordsMap);
            //AddDrawing(drawingData, GameManager.Instance.playerFlowManager.slidesRound.transform);
            //AddGuess(ColourManager.BirdName.red, guessesMap, correctWordsMap);
            //AddGuess(ColourManager.BirdName.red, guessesMap, correctWordsMap);
            //AddDrawing(drawingData, GameManager.Instance.playerFlowManager.slidesRound.transform);
            //AddGuess(ColourManager.BirdName.red, guessesMap, correctWordsMap);
            //AddGuess(ColourManager.BirdName.red, guessesMap, correctWordsMap);
        }


        private void Update()
        {
            if (active)
            {
                timeActive += Time.deltaTime * GameManager.Instance.playerFlowManager.slidesRound.slideSpeed;
                if (timeActive > duration)
                {
                    isComplete = true;
                }
            }
        }

        public void Initialize(string prefix, string noun, int caseID, float inDuration, float finalScore)
        {
            duration = inDuration;
            timeActive = 0f;
            summarySectionPrefabMap = new Dictionary<SlideTypeData.SlideType, GameObject>();
            foreach(GameObject summarySectionPrefab in summarySectionPrefabs)
            {
                SummarySlideSection summarySlideSection = summarySectionPrefab.GetComponent<SummarySlideSection>();
                summarySectionPrefabMap.Add(summarySlideSection.slideType, summarySectionPrefab);
            }
            
            originalPromptText.text = SettingsManager.Instance.CreatePromptText(prefix, noun);
            EndgameCaseData currentCase = GameManager.Instance.playerFlowManager.slidesRound.caseDataMap[caseID];
            caseTypeSlideVisualizer.Initialize(currentCase.caseTypeColour, currentCase.caseTypeName);
            finalScoreText.text = "Points Earned: " + finalScore.ToString();
        }

        public void AddDrawing(DrawingData drawingData, int round, int caseID, Transform summarySlidetransform, float timeModifierDecrement)
        {
            Vector3 spawnPosition = GetSpawnPosition();
            GameObject drawingSummarySlideSectionObject = Instantiate(summarySectionPrefabMap[SlideTypeData.SlideType.drawing], spawnPosition, Quaternion.identity, summarySectionsHolder);
            DrawingSummarySlideSection drawingSummarySlideSection = drawingSummarySlideSectionObject.GetComponent<DrawingSummarySlideSection>();
            drawingSummarySlideSection.Initialize(drawingData, summarySlidetransform, round, caseID, timeModifierDecrement);
            drawingSummarySlideSection.positionWhereItShouldBeIfUnityWasntShit = spawnPosition;
            summarySections.Add(drawingSummarySlideSection);
        }

        public void AddPrompt(PlayerTextInputData promptData, int round, int caseID, float timeModifierDecrement)
        {
            Vector3 spawnPosition = GetSpawnPosition();
            GameObject promptSummarySlideSectionObject = Instantiate(summarySectionPrefabMap[SlideTypeData.SlideType.prompt], spawnPosition, Quaternion.identity, summarySectionsHolder);
            PromptSummarySlideSection promptSummarySlideSection = promptSummarySlideSectionObject.GetComponent<PromptSummarySlideSection>();
            promptSummarySlideSection.Initialize(promptData, round, caseID, timeModifierDecrement);
            promptSummarySlideSection.positionWhereItShouldBeIfUnityWasntShit = spawnPosition;
            summarySections.Add(promptSummarySlideSection);
        }

        public void AddGuess(GuessData guessData, Dictionary<int,string> correctWordIdentifiersMap, int round, int caseID, float timeModifierDecrement)
        {
            Vector3 spawnPosition = GetSpawnPosition();
            GameObject guessSummarySlideSectionObject = Instantiate(summarySectionPrefabMap[SlideTypeData.SlideType.guess], spawnPosition, Quaternion.identity, summarySectionsHolder);
            GuessSummarySlideSection guessSummarySlideSection = guessSummarySlideSectionObject.GetComponent<GuessSummarySlideSection>();

            guessSummarySlideSection.Initialize(guessData, correctWordIdentifiersMap, round, caseID, timeModifierDecrement);
            guessSummarySlideSection.positionWhereItShouldBeIfUnityWasntShit = spawnPosition;
            summarySections.Add(guessSummarySlideSection);
        }

        private Vector3 GetSpawnPosition()
        {
            Vector3 spawnPosition = sectionReferencePositionTransform.position + Vector3.right * horizontalSectionDistance * currentColumn - Vector3.up * verticalSectionDistance * currentRow;
            currentColumn++;
            if(currentColumn >= maxColumnsPerRow)
            {
                currentColumn = 0;
                currentRow++;
            }
            return spawnPosition;
        }

        public override void Show()
        {
            base.Show();
            foreach(SummarySlideSection summarySection in summarySections)
            {
                summarySection.Show();
            }
        }
    }
}
