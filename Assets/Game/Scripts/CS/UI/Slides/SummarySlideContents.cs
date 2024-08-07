﻿using Steamworks;
using System;
using System.Collections;
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
        private TMPro.TMP_Text baseScoreText, finalScoreModifierText, finalScoreText;
        [SerializeField]
        private float horizontalSectionDistance, verticalSectionDistance;

        [SerializeField]
        private int maxColumnsPerRow;

        [SerializeField]
        private SlideBirdbuckDistributor slideBirdbuckDistributor;

        private Dictionary<SlideTypeData.SlideType, GameObject> summarySectionPrefabMap = new Dictionary<SlideTypeData.SlideType, GameObject>();

        private List<SummarySlideSection> summarySections = new List<SummarySlideSection>();
        private float duration;
        private float timeActive = 0f;
        private int currentRow = 0;
        private int currentColumn = 0;
        private int currentCaseID = -1;

        private void Start()
        {
            //StartCoroutine(Test());
        }

        IEnumerator Test()
        {
            SettingsManager.Instance.AssignBirdToPlayer(ColourManager.BirdName.red, "test");
            SettingsManager.Instance.AssignBirdToPlayer(ColourManager.BirdName.blue, "test");
            SettingsManager.Instance.AssignBirdToPlayer(ColourManager.BirdName.green, "test");
            SettingsManager.Instance.AssignBirdToPlayer(ColourManager.BirdName.orange, "test");
            GameDataManager.Instance.RefreshWords(new List<CaseWordData>());
            EndgameCaseData caseData = new EndgameCaseData() { caseTypeColour = Color.red, caseTypeName = "beans" };
            caseData.scoringData.prefixBirdbucks = 4;
            caseData.scoringData.nounBirdbucks = 6;
            caseData.scoringData.scoreModifier = 5.0f;
            GameManager.Instance.playerFlowManager.slidesRound.caseDataMap.Add(1, caseData);
            currentCaseID = 1;

            DrawingData drawingData = new DrawingData();
            drawingData.author = ColourManager.BirdName.red;
            drawingData.visuals = new List<DrawingLineData>() { new DrawingLineData() { lineColour = Color.red, author = ColourManager.BirdName.red, positions = new List<Vector3>() { { new Vector3(1f, 1f, 0f) }, { new Vector3(3f, 1f, 0f) }, { new Vector3(6f, 1f, 0f) }, { new Vector3(3f, 3f, 0f) } } } };
            Initialize(1, 8f, 10f);
            GuessData guess = new GuessData() { author = ColourManager.BirdName.red, prefix = "yup", noun = "nope", round = 2, timeTaken = 5f };
            Dictionary<int, string> correctWordsMap = new Dictionary<int, string>() { { 1, WordManager.testingPrefixIdentifier }, { 2, WordManager.testingNounIdentifier } };
            AddDrawing(drawingData, 1, 1, GameManager.Instance.playerFlowManager.slidesRound.transform, 0f);
            AddDrawing(drawingData, 1, 1, GameManager.Instance.playerFlowManager.slidesRound.transform, 0f);
            AddDrawing(drawingData, 1, 1, GameManager.Instance.playerFlowManager.slidesRound.transform, 0f);
            AddGuess(guess, correctWordsMap, 2, 1, 0f);
            LoadSections();
            yield return new WaitForSeconds(.1f);
            
            Show();
            active = true;

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

        public void Initialize(int caseID, float inDuration, float finalScore)
        {
            duration = inDuration;
            timeActive = 0f;
            currentCaseID = caseID;
            summarySectionPrefabMap = new Dictionary<SlideTypeData.SlideType, GameObject>();
            foreach(GameObject summarySectionPrefab in summarySectionPrefabs)
            {
                SummarySlideSection summarySlideSection = summarySectionPrefab.GetComponent<SummarySlideSection>();
                if(summarySectionPrefabMap.ContainsKey(summarySlideSection.slideType))
                {
                    Debug.LogError("Could not add summary section prefab["+summarySlideSection.slideType.ToString()+"] because it already exists in the prefab map.");
                }
                summarySectionPrefabMap.Add(summarySlideSection.slideType, summarySectionPrefab);
            }

            finalScoreText.text = "Birdbucks: " + finalScore.ToString();
            
        }

        public void AddDrawing(DrawingData drawingData, int round, int caseID, Transform summarySlidetransform, float timeModifierDecrement)
        {
            Vector3 spawnPosition = GetSpawnPosition();
            GameObject drawingSummarySlideSectionObject = Instantiate(summarySectionPrefabMap[SlideTypeData.SlideType.drawing], summarySectionsHolder);
            drawingSummarySlideSectionObject.transform.localPosition = spawnPosition;
            DrawingSummarySlideSection drawingSummarySlideSection = drawingSummarySlideSectionObject.GetComponent<DrawingSummarySlideSection>();
            drawingSummarySlideSection.Initialize(drawingData, summarySlidetransform, round, caseID, timeModifierDecrement);
            drawingSummarySlideSection.positionWhereItShouldBeIfUnityWasntShit = spawnPosition;
            summarySections.Add(drawingSummarySlideSection);
        }

        public void AddPrompt(PlayerTextInputData promptData, int round, int caseID, float timeModifierDecrement)
        {
            Vector3 spawnPosition = GetSpawnPosition();
            GameObject promptSummarySlideSectionObject = Instantiate(summarySectionPrefabMap[SlideTypeData.SlideType.prompt], summarySectionsHolder);
            promptSummarySlideSectionObject.transform.localPosition = spawnPosition;
            PromptSummarySlideSection promptSummarySlideSection = promptSummarySlideSectionObject.GetComponent<PromptSummarySlideSection>();
            promptSummarySlideSection.Initialize(promptData, round, caseID, timeModifierDecrement);
            promptSummarySlideSection.positionWhereItShouldBeIfUnityWasntShit = spawnPosition;

            summarySections.Add(promptSummarySlideSection);
        }

        public void AddGuess(GuessData guessData, Dictionary<int,string> correctWordIdentifiersMap, int round, int caseID, float timeModifierDecrement)
        {
            Vector3 spawnPosition = GetSpawnPosition();
            GameObject guessSummarySlideSectionObject = Instantiate(summarySectionPrefabMap[SlideTypeData.SlideType.guess], summarySectionsHolder);
            GuessSummarySlideSection guessSummarySlideSection = guessSummarySlideSectionObject.GetComponent<GuessSummarySlideSection>();
            guessSummarySlideSectionObject.transform.localPosition = spawnPosition;
            guessSummarySlideSection.Initialize(guessData, correctWordIdentifiersMap, round, caseID, timeModifierDecrement);
            guessSummarySlideSection.positionWhereItShouldBeIfUnityWasntShit = spawnPosition;
            
            summarySections.Add(guessSummarySlideSection);
        }

        public void LoadSections()
        {
            EndgameCaseData currentCase = GameManager.Instance.playerFlowManager.slidesRound.caseDataMap[currentCaseID];

            List<SummarySlideSection> sectionsToReceiveMoney = new List<SummarySlideSection>();
            foreach(SummarySlideSection slideSection in summarySections)
            {
                if(currentCase.GetPointsForPlayerOnTask(slideSection.author) > 0)
                {
                    sectionsToReceiveMoney.Add(slideSection);
                    CaseChoiceData caseChoiceData = GameDataManager.Instance.GetCaseChoice(currentCase.caseTypeName);
                    if(caseChoiceData != null && caseChoiceData.caseFormat == CaseTemplateData.CaseFormat.competition && slideSection.author != currentCase.guessData.author)
                    {
                        //This player was selected for the competition, a visual should be shown to indicate this
                        slideSection.ShowCompetitionSelectionVisual();
                    }
                }
            }

            slideBirdbuckDistributor.Initialize(currentCase.scoringData, sectionsToReceiveMoney);
        }

        private Vector3 GetSpawnPosition()
        {
            Vector3 spawnPosition = Vector3.right * horizontalSectionDistance * currentColumn - Vector3.up * verticalSectionDistance * currentRow;
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
            GameManager.Instance.playerFlowManager.slidesRound.UpdatePreviewBird(ColourManager.BirdName.none);
            GameManager.Instance.playerFlowManager.slidesRound.ShowCaseDetails();
            base.Show();
            foreach(SummarySlideSection summarySection in summarySections)
            {
                summarySection.Show();
            }
        }
    }
}
