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
            if (guessRoundStarted && !wordIsGuessed)
            {
                timeInGuessingRound += Time.deltaTime;
            }
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

            GameManager.Instance.gameDataHandler.CmdPlayerStats(drawingToolUsageMap[DrawingToolType.pencil], drawingToolUsageMap[DrawingToolType.colour_marker], drawingToolUsageMap[DrawingToolType.light_marker],
                                                                drawingToolUsageMap[DrawingToolType.eraser], SettingsManager.Instance.birdName, timeInBirdArea, timeInGuessingRound, stickyClickCount,
                                                                drawingsTrashed, playersLiked.Count, numberOfLikesGiven, SettingsManager.Instance.GetSetting("stickies", false), totalDistanceMoved);
        }

        public void SetServerPlayerStats()
        {
            bool usedStickies = SettingsManager.Instance.GetSetting("stickies", false);
            AccoladesStatManager statManager = GameManager.Instance.playerFlowManager.accoladesRound.playerStatsManager;
            statManager.AddPlayersideStats(SettingsManager.Instance.birdName, timeInBirdArea, timeInGuessingRound, stickyClickCount, drawingsTrashed, drawingToolUsageMap, playersLiked.Count, numberOfLikesGiven, usedStickies, totalDistanceMoved);
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