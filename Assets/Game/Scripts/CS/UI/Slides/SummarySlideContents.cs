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
        private float horizontalSectionDistance, verticalSectionDistance;

        [SerializeField]
        private int maxColumnsPerRow;

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
                timeActive += Time.deltaTime;
                if (timeActive > duration)
                {
                    isComplete = true;
                }
            }
        }

        public void Initialize(string originalPrompt, float inDuration)
        {
            duration = inDuration;
            timeActive = 0f;
            summarySectionPrefabMap = new Dictionary<SlideTypeData.SlideType, GameObject>();
            foreach(GameObject summarySectionPrefab in summarySectionPrefabs)
            {
                SummarySlideSection summarySlideSection = summarySectionPrefab.GetComponent<SummarySlideSection>();
                summarySectionPrefabMap.Add(summarySlideSection.slideType, summarySectionPrefab);
            }
            originalPromptText.text = originalPrompt;
        }

        public void AddDrawing(DrawingData drawingData, Transform summarySlidetransform)
        {
            Vector3 spawnPosition = GetSpawnPosition();
            GameObject drawingSummarySlideSectionObject = Instantiate(summarySectionPrefabMap[SlideTypeData.SlideType.drawing], spawnPosition, Quaternion.identity, summarySectionsHolder);
            DrawingSummarySlideSection drawingSummarySlideSection = drawingSummarySlideSectionObject.GetComponent<DrawingSummarySlideSection>();
            drawingSummarySlideSection.Initialize(drawingData, summarySlidetransform);
            drawingSummarySlideSection.positionWhereItShouldBeIfUnityWasntShit = spawnPosition;
            summarySections.Add(drawingSummarySlideSection);
        }

        public void AddPrompt(PlayerTextInputData promptData)
        {
            Vector3 spawnPosition = GetSpawnPosition();
            GameObject promptSummarySlideSectionObject = Instantiate(summarySectionPrefabMap[SlideTypeData.SlideType.prompt], spawnPosition, Quaternion.identity, summarySectionsHolder);
            PromptSummarySlideSection promptSummarySlideSection = promptSummarySlideSectionObject.GetComponent<PromptSummarySlideSection>();
            promptSummarySlideSection.Initialize(promptData);
            promptSummarySlideSection.positionWhereItShouldBeIfUnityWasntShit = spawnPosition;
            summarySections.Add(promptSummarySlideSection);
        }

        public void AddGuess(ColourManager.BirdName guesser, Dictionary<int,string> guessesMap, Dictionary<int,CaseWordData> correctWordsMap)
        {
            Vector3 spawnPosition = GetSpawnPosition();
            GameObject guessSummarySlideSectionObject = Instantiate(summarySectionPrefabMap[SlideTypeData.SlideType.guess], spawnPosition, Quaternion.identity, summarySectionsHolder);
            Debug.LogError("Spawned guess summary slide object position: " + guessSummarySlideSectionObject.transform.position.ToString());
            GuessSummarySlideSection guessSummarySlideSection = guessSummarySlideSectionObject.GetComponent<GuessSummarySlideSection>();
            guessSummarySlideSection.Initialize(guesser, guessesMap, correctWordsMap);
            guessSummarySlideSection.positionWhereItShouldBeIfUnityWasntShit = spawnPosition;
            summarySections.Add(guessSummarySlideSection);
        }

        private Vector3 GetSpawnPosition()
        {
            Vector3 spawnPosition = sectionReferencePositionTransform.position + Vector3.right * horizontalSectionDistance * currentColumn - Vector3.up * verticalSectionDistance * currentRow;
            Debug.LogError("Spawn position: " + spawnPosition.ToString());
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
