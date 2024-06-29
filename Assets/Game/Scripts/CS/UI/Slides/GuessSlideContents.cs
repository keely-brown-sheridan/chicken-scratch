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
        private TMP_Text prefixText, nounText;

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
        private SlideTimeModifierDecrementVisual slideTimeModifierDecrementVisual;

        [SerializeField]
        private SlideCaseScoreVisualization slideCaseScoreVisualization;

        [SerializeField]
        private float timeToUpdatePrefixScore, timeToUpdateNounScore, timeToUpdateModifierScore;

        [SerializeField]
        private GameObject botcherCoinPrefab;

        [SerializeField]
        private Transform botcherCoinHolder;

        private bool hasShownModifier = false;
        private bool hasShownNoun = false;
        private bool hasShownPrefix = false;
        private float duration;
        private float timeWaiting = 0.0f;

        private bool isPrefixCorrect, isNounCorrect;
        private float prefixScoreTarget, nounScoreTarget, modifierScoreTarget;
        private ColourManager.BirdName author;
        

        private void Start()
        {
            //Test();
        }

        private void Test()
        {
            SettingsManager.Instance.AssignBirdToPlayer(ColourManager.BirdName.red, "test");
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
            CaseWordData prefix = GameDataManager.Instance.GetWord(caseData.correctWordIdentifierMap[1]);
            CaseWordData noun = GameDataManager.Instance.GetWord(caseData.correctWordIdentifierMap[2]);
            timeWaiting = 0f;
            author = guessData.author;
            
            prefixText.text = guessData.prefix;
            isPrefixCorrect = guessData.prefix == prefix.value;
            prefixText.color = isPrefixCorrect ? correctColour : incorrectColour;
            nounText.text = guessData.noun;
            isNounCorrect = guessData.noun == noun.value;
            nounText.color = isNounCorrect ? correctColour : incorrectColour;
            goldStarDetectionArea.Initialize(guessData.author, round, caseData.identifier);
            
            slideTimeModifierDecrementVisual.Initialize(taskData.timeModifierDecrement);
            slideCaseScoreVisualization.Initialize(caseData.scoreModifier, caseData.maxScoreModifier);

            prefixScoreTarget = caseData.scoringData.prefixBirdbucks + GameManager.Instance.playerFlowManager.slidesRound.currentBirdBuckTotal;
            nounScoreTarget = caseData.scoringData.bonusBirdbucks + caseData.scoringData.nounBirdbucks + prefixScoreTarget;
            modifierScoreTarget = caseData.scoringData.GetTotalPoints() + GameManager.Instance.playerFlowManager.slidesRound.currentBirdBuckTotal;
        }

        public override void Show()
        {
            GameManager.Instance.playerFlowManager.slidesRound.ShowCaseDetails();
            GameManager.Instance.playerFlowManager.slidesRound.UpdatePreviewBird(author);
            base.Show();
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
                        if(SettingsManager.Instance.gameMode.hasAccusationRound)
                        {
                            AddBotcherCoin();
                        }
                        
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
                        if (SettingsManager.Instance.gameMode.hasAccusationRound)
                        {
                            AddBotcherCoin();
                            if(!isPrefixCorrect)
                            {
                                AddBotcherCoin();
                            }
                        }
                        
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

        private void AddBotcherCoin()
        {
            Instantiate(botcherCoinPrefab, botcherCoinHolder);
            if(SettingsManager.Instance.playerRole.team == RoleData.Team.botcher)
            {
                SettingsManager.Instance.botcherCoins++;
            }
        }
    }
}
