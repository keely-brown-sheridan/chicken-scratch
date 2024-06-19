using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class GuessSlideContents : SlideContents
    {
        [SerializeField]
        private GoldStarDetectionArea goldStarDetectionArea;
        [SerializeField]
        private Image authorImage;
        [SerializeField]
        private TMP_Text authorNameText;
        [SerializeField]
        private TMP_Text prefixText, nounText;
        [SerializeField]
        private TMP_Text originalPromptText;

        [SerializeField]
        private float prefixWaitDuration;
        [SerializeField]
        private float nounWaitDuration;
        [SerializeField]
        private float modifierWaitDuration;

        [SerializeField]
        private ParticleSystem successDescriptorEffect, successNounEffect;

        [SerializeField]
        private ParticleSystem failureDescriptorEffect, failureNounEffect;

        [SerializeField]
        private Color correctColour, incorrectColour;

        [SerializeField]
        private CaseTypeSlideVisualizer caseTypeSlideVisualizer;
        [SerializeField]
        private SlideTimeModifierDecrementVisual slideTimeModifierDecrementVisual;

        [SerializeField]
        private SlideCaseScoreVisualization slideCaseScoreVisualization;

        [SerializeField]
        private float timeToUpdatePrefixScore, timeToUpdateNounScore, timeToUpdateModifierScore;

        private bool hasShownModifier = false;
        private bool hasShownNoun = false;
        private bool hasShownPrefix = false;
        private float duration;
        private float timeWaiting = 0.0f;

        private bool isPrefixCorrect, isNounCorrect;
        private float prefixScoreTarget, nounScoreTarget, modifierScoreTarget;

        private void Start()
        {
            //Test();
        }

        private void Test()
        {
            GameDataManager.Instance.RefreshWords(new List<CaseWordData>());
            Dictionary<int,string> testCorrectIdentifiers = new Dictionary<int, string>() { { 1, "prefixes-NEUTRAL-ATTACHED" }, {2, "nouns-ANIMAL-AARDVARK" } };
            GuessData testGuess = new GuessData() { author = ColourManager.BirdName.red, prefix = "ATTACHED", noun = "AARDVARK", round = 1, timeTaken = 0f };
            EndgameCaseData testCase = new EndgameCaseData() { caseTypeColour = Color.cyan, caseTypeName = "Corners", guessData = testGuess, correctWordIdentifierMap = testCorrectIdentifiers, scoreModifier = 1.5f };
            GameManager.Instance.playerFlowManager.slidesRound.caseDataMap.Add(1, testCase);

            EndgameTaskData testTask = new EndgameTaskData() { timeModifierDecrement = -0.25f };
            testCase.taskDataMap.Add(1, testTask);
            Initialize(testCase, testTask, 1, 12f);
            active = true;
        }

        public void Initialize(EndgameCaseData caseData, EndgameTaskData taskData, int round, float inDuration)
        {
            duration = inDuration;
            GuessData guessData = caseData.guessData;
            string prefix = GameDataManager.Instance.GetWord(caseData.correctWordIdentifierMap[1]).value;
            string noun = GameDataManager.Instance.GetWord(caseData.correctWordIdentifierMap[2]).value;
            timeWaiting = 0f;
            Bird authorBird = ColourManager.Instance.birdMap[guessData.author];
            authorImage.sprite = authorBird.faceSprite;
            authorNameText.color = authorBird.colour;
            authorNameText.text = SettingsManager.Instance.GetPlayerName(guessData.author);
            prefixText.text = guessData.prefix;
            isPrefixCorrect = guessData.prefix == prefix;
            prefixText.color = isPrefixCorrect ? correctColour : incorrectColour;
            nounText.text = guessData.noun;
            isNounCorrect = guessData.noun == noun;
            nounText.color = isNounCorrect ? correctColour : incorrectColour;
            goldStarDetectionArea.Initialize(guessData.author, round, caseData.identifier);
            caseTypeSlideVisualizer.Initialize(caseData.caseTypeColour, caseData.caseTypeName);
            originalPromptText.text = SettingsManager.Instance.CreatePromptText(prefix, noun);
            slideTimeModifierDecrementVisual.Initialize(taskData.timeModifierDecrement);

            CaseChoiceData originalCaseChoice = GameDataManager.Instance.GetCaseChoice(caseData.caseTypeName);
            float bestModifier = originalCaseChoice.startingScoreModifier;
            float bestPossibleScore = bestModifier * (originalCaseChoice.bonusPoints + originalCaseChoice.pointsPerCorrectWord * 2);
            slideCaseScoreVisualization.Initialize(bestPossibleScore, caseData.scoreModifier);

            if(isPrefixCorrect)
            {
                prefixScoreTarget = originalCaseChoice.pointsPerCorrectWord;
            }
            nounScoreTarget = prefixScoreTarget;
            if(isNounCorrect)
            {
                nounScoreTarget += originalCaseChoice.pointsPerCorrectWord;
                if(isPrefixCorrect)
                {
                    nounScoreTarget += originalCaseChoice.bonusPoints;
                }
            }
            modifierScoreTarget = nounScoreTarget * caseData.scoreModifier;
        }

        private void Update()
        {
            if(active)
            {
                timeWaiting += Time.deltaTime * GameManager.Instance.playerFlowManager.slidesRound.slideSpeed;
                if (timeWaiting > duration)
                {
                    isComplete = true;
                }
                else if (timeWaiting * GameManager.Instance.playerFlowManager.slidesRound.slideSpeed > prefixWaitDuration &&
                    !hasShownPrefix)
                {
                    
                    slideCaseScoreVisualization.SetTarget(prefixScoreTarget, timeToUpdatePrefixScore);
                    hasShownPrefix = true;
                    prefixText.gameObject.SetActive(true);
                    if (isPrefixCorrect)
                    {
                        successDescriptorEffect.gameObject.SetActive(true);
                        successDescriptorEffect.Play();
                        AudioManager.Instance.PlaySound("TimeBonus");
                    }
                    else
                    {
                        failureDescriptorEffect.gameObject.SetActive(true);
                        failureDescriptorEffect.Play();
                        AudioManager.Instance.PlaySound("TimePenalty");
                    }
                }
                else if (timeWaiting * GameManager.Instance.playerFlowManager.slidesRound.slideSpeed > nounWaitDuration &&
                        !hasShownNoun)
                {
                    slideCaseScoreVisualization.SetTarget(nounScoreTarget, timeToUpdateNounScore);
                    hasShownNoun = true;
                    nounText.gameObject.SetActive(true);
                    if (isNounCorrect)
                    {
                        successNounEffect.gameObject.SetActive(true);
                        successNounEffect.Play();
                        AudioManager.Instance.PlaySound("TimeBonus");
                    }
                    else
                    {
                        failureNounEffect.gameObject.SetActive(true);
                        failureNounEffect.Play();
                        AudioManager.Instance.PlaySound("TimePenalty");
                    }
                }
                else if(timeWaiting * GameManager.Instance.playerFlowManager.slidesRound.slideSpeed > modifierWaitDuration &&
                    !hasShownModifier)
                {
                    slideCaseScoreVisualization.ShowScoreModifier();
                    slideCaseScoreVisualization.SetTarget(modifierScoreTarget, timeToUpdateModifierScore);
                    hasShownModifier = true;
                }
            }
            
        }
    }
}
