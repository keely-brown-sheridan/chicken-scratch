using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ChickenScratch.ColourManager;
using static ChickenScratch.DrawingController;

namespace ChickenScratch
{
    public class AccoladesStatManager : MonoBehaviour
    {
        public List<StatRole> allStatRoles = new List<StatRole>();
        public Dictionary<StatRole.StatRoleType, StatRole> statRoleMap = new Dictionary<StatRole.StatRoleType, StatRole>();
        public Dictionary<BirdName, PlayerStats> playerStatsMap = new Dictionary<BirdName, PlayerStats>();

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void AddPlayersideStats(BirdName player, float inTimeInCabinetArea, float inTimeGuessing, int inStickiesClicked, int inDrawingsTrashed, Dictionary<DrawingToolType, bool> inToolsUsageMap, int numberOfPlayersLiked, int numberOfLikesGiven, bool inUsedTutorial, float inTotalDistanceMoved)
        {
            if (!playerStatsMap.ContainsKey(player))
            {
                playerStatsMap.Add(player, new PlayerStats());
            }
            playerStatsMap[player].timeInCabinetArea = inTimeInCabinetArea;
            playerStatsMap[player].timeInGuessingRound = inTimeGuessing;
            playerStatsMap[player].clickedStickies = inStickiesClicked;
            playerStatsMap[player].trashedDrawings = inDrawingsTrashed;
            playerStatsMap[player].toolUsageMap = inToolsUsageMap;
            playerStatsMap[player].numberOfPlayersLiked = numberOfPlayersLiked;
            playerStatsMap[player].numberOfLikesGiven = numberOfLikesGiven;
            playerStatsMap[player].usedTutorial = inUsedTutorial;
            playerStatsMap[player].totalDistanceMoved = inTotalDistanceMoved;

            if (playerStatsMap.Count == SettingsManager.Instance.GetPlayerNameCount())
            {
                sendStatsToClients();
            }
        }

        private void sendStatsToClients()
        {
            Dictionary<BirdName, StatRole> statRoles = GetPlayerStatRoles();
            GameManager.Instance.gameDataHandler.RpcPlayerStatRolesWrapper(statRoles);

        }

        public void InitializeStatRoles()
        {
            statRoleMap = new Dictionary<StatRole.StatRoleType, StatRole>();
            foreach (StatRole statRole in allStatRoles)
            {
                statRoleMap.Add(statRole.statRoleType, statRole);
            }
        }

        public Dictionary<BirdName, StatRole> GetPlayerStatRoles()
        {
            InitializeStatRoles();
            Dictionary<BirdName, StatRole> playerStatRoleMap = new Dictionary<BirdName, StatRole>();
            List<StatRole> takenStatRoles = new List<StatRole>();
            //Determine list of all viable stat roles for all players
            int longestPrompt = 0;
            BirdName longestPrompter = BirdName.none;
            int easiestCaseDifficulty = 100;
            BirdName easiestCaseGuesser = BirdName.none;
            int hardestCaseDifficulty = 0;
            BirdName hardestCaseGuesser = BirdName.none;
            Dictionary<BirdName, float> timeTakenMap = new Dictionary<BirdName, float>();
            float closestTimeToDeadline = 1000f;
            BirdName closeCutter = BirdName.none;
            float timeRemaining = 0f;

            List<BirdName> allColourers = new List<BirdName>();
            List<BirdName> partialWordSnipers = new List<BirdName>();
            List<BirdName> fullWordSnipers = new List<BirdName>();

            Dictionary<BirdName, int> lineCountMap = new Dictionary<BirdName, int>();
            Dictionary<BirdName, int> pointsCountMap = new Dictionary<BirdName, int>();
            Dictionary<BirdName, List<Color>> colourUsageMap = new Dictionary<BirdName, List<Color>>();
            foreach (EndgameCaseData currentCase in GameManager.Instance.playerFlowManager.slidesRound.caseDataMap.Values)
            {
                foreach(EndgameTaskData currentTask in currentCase.taskDataMap.Values)
                {
                    if(!pointsCountMap.ContainsKey(currentTask.assignedPlayer))
                    {
                        pointsCountMap.Add(currentTask.assignedPlayer, 0);
                    }
                    pointsCountMap[currentTask.assignedPlayer] += currentCase.scoringData.GetTotalPoints() / currentCase.taskDataMap.Count;
                    switch (currentTask.taskType)
                    {
                        case TaskData.TaskType.prompt_drawing:
                        case TaskData.TaskType.base_drawing:
                        case TaskData.TaskType.copy_drawing:
                        case TaskData.TaskType.add_drawing:
                        case TaskData.TaskType.compile_drawing:
                            if (!timeTakenMap.ContainsKey(currentTask.drawingData.author))
                            {
                                timeTakenMap.Add(currentTask.drawingData.author, 0.0f);
                            }
                            timeTakenMap[currentTask.drawingData.author] += currentTask.drawingData.timeTaken;
                            timeRemaining = 0f;
                            if (timeRemaining < closestTimeToDeadline && timeRemaining > 0)
                            {
                                closestTimeToDeadline = timeRemaining;
                                closeCutter = currentTask.drawingData.author;
                            }
                            if (!lineCountMap.ContainsKey(currentTask.drawingData.author))
                            {
                                lineCountMap.Add(currentTask.drawingData.author, 0);
                            }
                            lineCountMap[currentTask.drawingData.author] += currentTask.drawingData.visuals.Count;

                            if (!allColourers.Contains(currentTask.drawingData.author))
                            {
                                foreach (DrawingLineData visual in currentTask.drawingData.visuals)
                                {
                                    if (!colourUsageMap.ContainsKey(currentTask.drawingData.author))
                                    {
                                        colourUsageMap.Add(currentTask.drawingData.author, new List<Color>());
                                    }
                                    if (!colourUsageMap[currentTask.drawingData.author].Contains(visual.lineColour))
                                    {
                                        colourUsageMap[currentTask.drawingData.author].Add(visual.lineColour);
                                    }
                                    if (colourUsageMap[currentTask.drawingData.author].Count == 4)
                                    {
                                        allColourers.Add(currentTask.drawingData.author);
                                    }
                                }
                            }
                            break;
                        case TaskData.TaskType.prompting:
                            if (currentTask.promptData.author == BirdName.none) continue;

                            CaseWordData correctPrefix = GameDataManager.Instance.GetWord(currentCase.correctWordIdentifierMap[1]);
                            CaseWordData correctNoun = GameDataManager.Instance.GetWord(currentCase.correctWordIdentifierMap[2]);
                            bool nounIsCorrect = currentTask.promptData.text.Contains(correctPrefix.value);
                            bool prefixIsCorrect = currentTask.promptData.text.Contains(correctNoun.value);
                            if (nounIsCorrect && prefixIsCorrect && !fullWordSnipers.Contains(currentTask.promptData.author))
                            {
                                fullWordSnipers.Add(currentTask.promptData.author);
                            }
                            else if ((nounIsCorrect || prefixIsCorrect) && !partialWordSnipers.Contains(currentTask.promptData.author))
                            {
                                partialWordSnipers.Add(currentTask.promptData.author);
                            }
                            if (currentTask.promptData.text.Length > longestPrompt)
                            {
                                longestPrompt = currentTask.promptData.text.Length;
                                longestPrompter = currentTask.promptData.author;
                            }
                            if (!timeTakenMap.ContainsKey(currentTask.promptData.author))
                            {
                                timeTakenMap.Add(currentTask.promptData.author, 0.0f);
                            }
                            timeTakenMap[currentTask.promptData.author] += currentTask.promptData.timeTaken;
                            timeRemaining = 0f;
                            if (timeRemaining < closestTimeToDeadline && timeRemaining > 0)
                            {
                                closestTimeToDeadline = timeRemaining;
                                closeCutter = currentTask.promptData.author;
                            }
                            if (!pointsCountMap.ContainsKey(currentTask.promptData.author))
                            {
                                pointsCountMap.Add(currentTask.promptData.author, 0);
                            }

                            break;
                        case TaskData.TaskType.base_guessing:
                            if (currentCase.WasCorrect())
                            {
                                int caseDifficulty = currentCase.GetDifficulty();

                                if (caseDifficulty < easiestCaseDifficulty)
                                {
                                    easiestCaseDifficulty = caseDifficulty;
                                    easiestCaseGuesser = currentTask.assignedPlayer;
                                }
                                if (caseDifficulty > hardestCaseDifficulty)
                                {
                                    hardestCaseDifficulty = caseDifficulty;
                                    hardestCaseGuesser = currentTask.assignedPlayer;
                                }
                            }
                            
                            break;
                    }
                }



            }

            float mostTimeUsed = 0;
            BirdName mostTimeUser = BirdName.none;
            float leastTimeUsed = 100000f;
            BirdName leastTimeUser = BirdName.none;
            foreach (KeyValuePair<BirdName, float> timeTaken in timeTakenMap)
            {
                if (mostTimeUsed < timeTaken.Value)
                {
                    mostTimeUsed = timeTaken.Value;
                    mostTimeUser = timeTaken.Key;
                }
                if (leastTimeUsed > timeTaken.Value)
                {
                    leastTimeUsed = timeTaken.Value;
                    leastTimeUser = timeTaken.Key;
                }
            }

            int mostLines = 0;
            BirdName mostLiner = BirdName.none;
            foreach (KeyValuePair<BirdName, int> playerLineCount in lineCountMap)
            {
                if (mostLines < playerLineCount.Value)
                {
                    mostLines = playerLineCount.Value;
                    mostLiner = playerLineCount.Key;
                }
            }
            int mostCasePoints = 0;
            BirdName mostSuccessfulPlayer = BirdName.none;
            foreach (KeyValuePair<BirdName, int> playerPointCount in pointsCountMap)
            {
                if (mostCasePoints < playerPointCount.Value)
                {
                    mostCasePoints = playerPointCount.Value;
                    mostSuccessfulPlayer = playerPointCount.Key;
                }
                else if (mostCasePoints == playerPointCount.Value)
                {
                    mostSuccessfulPlayer = BirdName.none;
                }
            }

            float slowestGuessTime = 0f;
            BirdName slowestGuesser = BirdName.none;
            float fastestGuessTime = 0f;
            BirdName fastestGuesser = BirdName.none;
            int mostStarsGiven = 0;
            BirdName mostStarsGiver = BirdName.none;

            List<BirdName> starSpreaders = new List<BirdName>();
            float mostBirdArmTime = 0f;
            BirdName mostBirdArmer = BirdName.none;
            float leastBirdArmTime = 0f;
            BirdName leastBirdArmer = BirdName.none;
            int mostTrashes = 0;
            BirdName trasher = BirdName.none;
            List<BirdName> allToolers = new List<BirdName>();
            List<BirdName> singleToolers = new List<BirdName>();
            List<BirdName> nonLikers = new List<BirdName>();
            int mostStickiesClicked = 0;
            BirdName highestClicker = BirdName.none;
            int leastStickiesClicked = 0;
            BirdName lowestClicker = BirdName.none;
            float mostDistanceArmMoved = 0.0f;
            BirdName mostArmMover = BirdName.none;
            foreach (KeyValuePair<BirdName, PlayerStats> playerStat in playerStatsMap)
            {
                if (playerStat.Value.timeInGuessingRound < fastestGuessTime)
                {
                    fastestGuessTime = playerStat.Value.timeInGuessingRound;
                    fastestGuesser = playerStat.Key;
                }
                else if (playerStat.Value.timeInGuessingRound > slowestGuessTime)
                {
                    slowestGuessTime = playerStat.Value.timeInGuessingRound;
                    slowestGuesser = playerStat.Key;
                }

                if (playerStat.Value.numberOfLikesGiven == 0)
                {
                    nonLikers.Add(playerStat.Key);
                }

                if (playerStat.Value.numberOfLikesGiven > mostStarsGiven)
                {
                    mostStarsGiven = playerStat.Value.numberOfLikesGiven;
                    mostStarsGiver = playerStat.Key;
                }
                int playerCount = SettingsManager.Instance.GetPlayerNameCount();
                if (playerStat.Value.numberOfPlayersLiked == playerCount - 1 &&
                    playerCount > 2)
                {
                    starSpreaders.Add(playerStat.Key);
                }

                if (playerStat.Value.timeInCabinetArea > mostBirdArmTime)
                {
                    mostBirdArmTime = playerStat.Value.timeInCabinetArea;
                    mostBirdArmer = playerStat.Key;
                }
                else if (playerStat.Value.timeInCabinetArea < leastBirdArmTime)
                {
                    leastBirdArmTime = playerStat.Value.timeInCabinetArea;
                    leastBirdArmer = playerStat.Key;
                }
                if (playerStat.Value.trashedDrawings > 0 && mostTrashes < playerStat.Value.trashedDrawings)
                {
                    mostTrashes = playerStat.Value.trashedDrawings;
                    trasher = playerStat.Key;
                }
                int toolsUsed = 0;
                foreach (bool toolUsed in playerStat.Value.toolUsageMap.Values)
                {
                    toolsUsed += toolUsed ? 1 : 0;
                }
                if (toolsUsed == 1)
                {
                    singleToolers.Add(playerStat.Key);
                }
                else if (toolsUsed == 4)
                {
                    allToolers.Add(playerStat.Key);
                }

                if (mostStickiesClicked < playerStat.Value.clickedStickies && playerStat.Value.clickedStickies > 0)
                {
                    mostStickiesClicked = playerStat.Value.clickedStickies;
                    highestClicker = playerStat.Key;
                }
                else if (leastStickiesClicked > playerStat.Value.clickedStickies && playerStat.Value.usedTutorial)
                {
                    leastStickiesClicked = playerStat.Value.clickedStickies;
                    lowestClicker = playerStat.Key;
                }

                if (mostDistanceArmMoved < playerStat.Value.totalDistanceMoved)
                {
                    mostDistanceArmMoved = playerStat.Value.totalDistanceMoved;
                    mostArmMover = playerStat.Key;
                }
            }

            int mostReactions = 0;
            BirdName mostReactor = BirdName.none;
            int mostReactionsTo = 0;
            BirdName mostReacted = BirdName.none;
            Dictionary<BirdName, int> reactingAtMap = new Dictionary<BirdName, int>();
            Dictionary<BirdName, int> reactedToMap = new Dictionary<BirdName, int>();
            foreach (KeyValuePair<BirdName, Dictionary<BirdName, List<int>>> reactedToPlayer in StatTracker.Instance.reactionMap)
            {
                if (!reactedToMap.ContainsKey(reactedToPlayer.Key))
                {
                    reactedToMap.Add(reactedToPlayer.Key, 0);
                }
                foreach (KeyValuePair<BirdName, List<int>> reactingPlayer in reactedToPlayer.Value)
                {
                    if (!reactingAtMap.ContainsKey(reactingPlayer.Key))
                    {
                        reactingAtMap.Add(reactingPlayer.Key, 0);
                    }
                    reactedToMap[reactedToPlayer.Key] += reactingPlayer.Value.Count;
                    reactingAtMap[reactingPlayer.Key] += reactingPlayer.Value.Count;
                }
            }
            foreach (KeyValuePair<BirdName, int> reactingPlayer in reactingAtMap)
            {
                if (reactingPlayer.Value > 0)
                {
                    if (reactingPlayer.Value > mostReactions)
                    {
                        mostReactions = reactingPlayer.Value;
                        mostReactor = reactingPlayer.Key;
                    }
                }
            }
            foreach (KeyValuePair<BirdName, int> reactedToPlayer in reactedToMap)
            {
                if (reactedToPlayer.Value > 0 && reactedToPlayer.Value > mostReactionsTo)
                {
                    mostReactionsTo = reactedToPlayer.Value;
                    mostReacted = reactedToPlayer.Key;
                }
            }
            //Add relevant stat roles to every player
            Dictionary<BirdName, List<StatRole>> allRolesMap = new Dictionary<BirdName, List<StatRole>>();
            List<BirdName> allActiveBirds = SettingsManager.Instance.GetAllActiveBirds();
            foreach (BirdName player in allActiveBirds)
            {
                allRolesMap.Add(player, new List<StatRole>());
                allRolesMap[player].Add(statRoleMap[StatRole.StatRoleType.safeguard_1]);
                allRolesMap[player].Add(statRoleMap[StatRole.StatRoleType.safeguard_2]);
                allRolesMap[player].Add(statRoleMap[StatRole.StatRoleType.safeguard_3]);
                allRolesMap[player].Add(statRoleMap[StatRole.StatRoleType.safeguard_4]);
                allRolesMap[player].Add(statRoleMap[StatRole.StatRoleType.safeguard_5]);
                allRolesMap[player].Add(statRoleMap[StatRole.StatRoleType.safeguard_6]);

                if (!reactingAtMap.ContainsKey(player) || reactingAtMap[player] == 0)
                {
                    allRolesMap[player].Add(statRoleMap[StatRole.StatRoleType.did_not_react]);
                }
                if (StatTracker.Instance.playersWhoSubmittedAnEmptyRound.Contains(player))
                {
                    allRolesMap[player].Add(statRoleMap[StatRole.StatRoleType.empty_round]);
                }
            }
            if (longestPrompter != BirdName.none)
            {
                allRolesMap[longestPrompter].Add(statRoleMap[StatRole.StatRoleType.longest_prompt]);
            }
            if (easiestCaseGuesser != BirdName.none)
            {
                allRolesMap[easiestCaseGuesser].Add(statRoleMap[StatRole.StatRoleType.easiest_guess]);
            }
            if (hardestCaseGuesser != BirdName.none)
            {
                allRolesMap[hardestCaseGuesser].Add(statRoleMap[StatRole.StatRoleType.hardest_guess]);
            }
            if (closeCutter != BirdName.none)
            {
                allRolesMap[closeCutter].Add(statRoleMap[StatRole.StatRoleType.cutting_it_close]);
            }
            if (mostTimeUser != BirdName.none)
            {
                allRolesMap[mostTimeUser].Add(statRoleMap[StatRole.StatRoleType.most_time_used]);
            }
            if (leastTimeUser != BirdName.none)
            {
                allRolesMap[leastTimeUser].Add(statRoleMap[StatRole.StatRoleType.least_time_used]);
            }
            if (mostLiner != BirdName.none)
            {
                allRolesMap[mostLiner].Add(statRoleMap[StatRole.StatRoleType.most_lines]);
            }
            if (mostSuccessfulPlayer != BirdName.none)
            {
                allRolesMap[mostSuccessfulPlayer].Add(statRoleMap[StatRole.StatRoleType.most_successful]);
            }
            if (slowestGuesser != BirdName.none)
            {
                allRolesMap[slowestGuesser].Add(statRoleMap[StatRole.StatRoleType.slowest_guess]);
            }
            if (fastestGuesser != BirdName.none)
            {
                allRolesMap[fastestGuesser].Add(statRoleMap[StatRole.StatRoleType.fastest_guess]);
            }
            if (mostStarsGiver != BirdName.none)
            {
                allRolesMap[mostStarsGiver].Add(statRoleMap[StatRole.StatRoleType.most_stars_given]);
            }
            if (mostBirdArmer != BirdName.none)
            {
                allRolesMap[mostBirdArmer].Add(statRoleMap[StatRole.StatRoleType.most_bird_arming]);
            }
            if (leastBirdArmer != BirdName.none)
            {
                allRolesMap[leastBirdArmer].Add(statRoleMap[StatRole.StatRoleType.least_bird_arming]);
            }
            if (trasher != BirdName.none)
            {
                allRolesMap[trasher].Add(statRoleMap[StatRole.StatRoleType.most_trashed_drawings]);
            }
            if (highestClicker != BirdName.none)
            {
                allRolesMap[highestClicker].Add(statRoleMap[StatRole.StatRoleType.most_stickies_clicked]);
            }
            if (lowestClicker != BirdName.none)
            {
                allRolesMap[lowestClicker].Add(statRoleMap[StatRole.StatRoleType.most_stickies_unclicked]);
            }
            if (mostReactor != BirdName.none)
            {
                allRolesMap[mostReactor].Add(statRoleMap[StatRole.StatRoleType.most_reactions]);
            }
            if (mostReacted != BirdName.none)
            {
                allRolesMap[mostReacted].Add(statRoleMap[StatRole.StatRoleType.reacted_to_most]);
            }
            if (mostArmMover != BirdName.none)
            {
                allRolesMap[mostArmMover].Add(statRoleMap[StatRole.StatRoleType.moved_arm_most_distance]);
            }

            foreach (BirdName starSpreader in starSpreaders)
            {
                allRolesMap[starSpreader].Add(statRoleMap[StatRole.StatRoleType.spread_the_stars]);
            }
            foreach (BirdName allColourer in allColourers)
            {
                allRolesMap[allColourer].Add(statRoleMap[StatRole.StatRoleType.used_all_colours]);
            }
            foreach (BirdName partialWordSniper in partialWordSnipers)
            {
                allRolesMap[partialWordSniper].Add(statRoleMap[StatRole.StatRoleType.prompted_word_correctly]);
            }
            foreach (BirdName fullWordSniper in fullWordSnipers)
            {
                allRolesMap[fullWordSniper].Add(statRoleMap[StatRole.StatRoleType.prompted_case_correctly]);
            }
            foreach (BirdName allTooler in allToolers)
            {
                allRolesMap[allTooler].Add(statRoleMap[StatRole.StatRoleType.used_all_tools]);
            }
            foreach (BirdName singleTooler in singleToolers)
            {
                allRolesMap[singleTooler].Add(statRoleMap[StatRole.StatRoleType.single_tooler]);
            }
            foreach (BirdName nonLiker in nonLikers)
            {
                allRolesMap[nonLiker].Add(statRoleMap[StatRole.StatRoleType.liked_nothing]);
            }

            foreach (BirdName player in allActiveBirds)
            {
                allRolesMap[player] = allRolesMap[player].OrderBy(a => Guid.NewGuid()).ToList();
            }
            List<StatRole.StatRoleType> chosenRoles = new List<StatRole.StatRoleType>();
            foreach (KeyValuePair<BirdName, List<StatRole>> playerStatRoles in allRolesMap)
            {
                //Choose the highest available stat role for each player
                int highestWeight = -1;
                StatRole chosenRole = null;
                foreach (StatRole playerStatRole in playerStatRoles.Value)
                {
                    if (playerStatRole.weight > highestWeight && !chosenRoles.Contains(playerStatRole.statRoleType))
                    {
                        chosenRole = playerStatRole;
                        highestWeight = chosenRole.weight;
                    }
                }
                chosenRoles.Add(chosenRole.statRoleType);
                playerStatRoleMap.Add(playerStatRoles.Key, chosenRole);
            }

            return playerStatRoleMap;
        }

        [System.Serializable]
        public class StatRole
        {
            public enum StatRoleType
            {
                longest_prompt, easiest_guess, slowest_guess,
                most_time_used, cutting_it_close, hardest_guess,
                least_time_used, spread_the_stars, most_stars_given,
                most_reactions, most_bird_arming, most_trashed_drawings,
                most_lines, used_all_tools, most_stickies_clicked,
                most_stickies_unclicked, used_all_colours, single_tooler,
                prompted_word_correctly, prompted_case_correctly, fastest_guess,
                most_successful, reacted_to_most, least_bird_arming,
                safeguard_1, safeguard_2, safeguard_3,
                safeguard_4, safeguard_5, safeguard_6,
                liked_nothing, did_not_react, moved_arm_most_distance,
                empty_round,
                invalid
            }
            public string name = "";
            public string description = "";
            public int weight = -1;
            public StatRoleType statRoleType;
        }
        public class PlayerStats
        {
            public float timeInCabinetArea = 0.0f;
            public float timeInGuessingRound = 0.0f;
            public int clickedStickies = 0;
            public int trashedDrawings = 0;
            public Dictionary<DrawingToolType, bool> toolUsageMap = new Dictionary<DrawingToolType, bool>();
            public int numberOfPlayersLiked = 0;
            public int numberOfLikesGiven = 0;
            public bool usedTutorial = false;
            public float totalDistanceMoved = 0.0f;
        }
    }
}