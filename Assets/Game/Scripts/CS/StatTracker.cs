using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ChickenScratch.ColourManager;
using static ChickenScratch.DrawingController;

namespace ChickenScratch
{
    public class StatTracker : Singleton<StatTracker>
    {
        public float timeInBirdArea = 0f;
        public float timeInGuessingRound = 0f;
        public Dictionary<DrawingToolType, bool> drawingToolUsageMap = new Dictionary<DrawingToolType, bool>();
        public int stickyClickCount = 0;
        public int drawingsTrashed = 0;
        public int numberOfLikesGiven = 0;

        public bool guessRoundStarted = false;
        public bool wordIsGuessed = false;
        public float totalDistanceMoved = 0.0f;
        public int totalSpent = 0;
        public int totalItemsPurchased = 0;
        public int totalCoffeeItemsPurchased = 0;
        public float timeChoosing = 0f;
        public int casesStarted = 0;
        public bool alwaysChoseHighestDifficulty = true;
        public bool alwaysChoseLowestDifficulty = true;
        public int storeRestocks = 0;
        public bool restockedEmptyShop = false;
        public bool hasLostModifier = false;
        public List<string> uniqueCaseChoiceIdentifiers = new List<string>();

        public List<BirdName> playersWhoSubmittedAnEmptyRound = new List<BirdName>();

        public Dictionary<BirdName, Dictionary<BirdName, List<int>>> reactionMap = new Dictionary<BirdName, Dictionary<BirdName, List<int>>>();
        private List<BirdName> playersLiked = new List<BirdName>();
        private bool isInitialized = false;
        void Start()
        {
            if (!isInitialized)
            {
                Initialize();
            }
        }

        void Update()
        {

        }

        private void Initialize()
        {
            drawingToolUsageMap = new Dictionary<DrawingToolType, bool>();
            drawingToolUsageMap.Add(DrawingToolType.pencil, false);
            drawingToolUsageMap.Add(DrawingToolType.colour_marker, false);
            drawingToolUsageMap.Add(DrawingToolType.light_marker, false);
            drawingToolUsageMap.Add(DrawingToolType.eraser, false);
            drawingToolUsageMap.Add(DrawingToolType.square_stamp, false);
        }

        public void SendStatsToServer()
        {
            if (!isInitialized)
            {
                Initialize();
            }
            PlayerStatData statData = new PlayerStatData()
            {
                pencilUsed = drawingToolUsageMap[DrawingToolType.pencil], 
                colourMarkerUsed = drawingToolUsageMap[DrawingToolType.colour_marker], 
                lightMarkerUsed = drawingToolUsageMap[DrawingToolType.light_marker],                                               
                eraserUsed = drawingToolUsageMap[DrawingToolType.eraser], 
                birdName = SettingsManager.Instance.birdName, 
                timeInCabinetArea = timeInBirdArea, 
                guessingTime = timeInGuessingRound, 
                stickyCount = stickyClickCount,
                drawingsTrashed = drawingsTrashed, 
                numberOfPlayersLiked = playersLiked.Count, 
                numberOfLikesGiven = numberOfLikesGiven, 
                usedStickies = SettingsManager.Instance.GetSetting("stickies", false), 
                totalDistanceMoved = totalDistanceMoved,
                totalSpent = totalSpent,
                totalItemsPurchased = totalItemsPurchased,
                totalUnspent = GameManager.Instance.playerFlowManager.storeRound.currentMoney,
                totalCoffeeItemsPurchased = totalCoffeeItemsPurchased,
                timeChoosing = timeChoosing,
                casesStarted = casesStarted,
                alwaysChoseHighestDifficulty = alwaysChoseHighestDifficulty,
                alwaysChoseLowestDifficulty = alwaysChoseLowestDifficulty,
                storeRestocks = storeRestocks,
                restockedEmptyShop = restockedEmptyShop,
                hasLostModifier = hasLostModifier,
                numberOfUniqueCases = uniqueCaseChoiceIdentifiers.Count
            };

            GameManager.Instance.gameDataHandler.CmdPlayerStats(statData);
        }

        public void AddToReactionCounter(BirdName player, string reactionName, int round)
        {
            DrawingRound drawingRound = GameManager.Instance.playerFlowManager.drawingRound;
            SlidesRound slidesRound = GameManager.Instance.playerFlowManager.slidesRound;

            if (drawingRound.caseMap.ContainsKey(slidesRound.currentSlideCaseIndex))
            {
                ChainData currentCase = drawingRound.caseMap[slidesRound.currentSlideCaseIndex];
                if (currentCase.playerOrder.ContainsKey(round))
                {
                    BirdName reactedToPlayer = currentCase.playerOrder[round];
                    if (!reactionMap.ContainsKey(reactedToPlayer))
                    {
                        reactionMap.Add(reactedToPlayer, new Dictionary<BirdName, List<int>>());

                    }
                    if (!reactionMap[reactedToPlayer].ContainsKey(player))
                    {
                        reactionMap[reactedToPlayer].Add(player, new List<int>());
                    }
                    if (!reactionMap[reactedToPlayer][player].Contains(round))
                    {
                        reactionMap[reactedToPlayer][player].Add(round);
                    }
                }
            }
        }

        public void LikePlayer(BirdName birdName)
        {
            if (!playersLiked.Contains(birdName))
            {
                playersLiked.Add(birdName);
            }
            numberOfLikesGiven++;
        }

        public void AddEmptySubmitter(BirdName birdName)
        {
            if (!playersWhoSubmittedAnEmptyRound.Contains(birdName))
            {
                playersWhoSubmittedAnEmptyRound.Add(birdName);
            }
        }
    }
}