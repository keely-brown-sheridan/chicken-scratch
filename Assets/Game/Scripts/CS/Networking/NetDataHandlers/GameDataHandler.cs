using ChickenScratch;
using Mirror;
using Org.BouncyCastle.Asn1.X500;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ChickenScratch.ColourManager;
using static ChickenScratch.DrawingData;
using static ChickenScratch.GameFlowManager;
using static ChickenScratch.GameModeData;
using static ChickenScratch.ReactionIndex;
using static ChickenScratch.TaskData;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;

public class GameDataHandler : NetworkBehaviour
{
    [Command(requiresAuthority = false)]
    public void CmdSetPlayerBird(string playerID, ColourManager.BirdName birdName, NetworkConnectionToClient sender = null)
    {
        
        SettingsManager.Instance.AssignBirdToConnection(birdName, sender);
        RpcSetPlayerBird(playerID, birdName);
    }

    [ClientRpc]
    public void RpcSetPlayerBird(string playerID, ColourManager.BirdName birdName)
    {
        SettingsManager.Instance.AssignBirdToPlayer(birdName, playerID);
    }

    [Command(requiresAuthority = false)]
    public void CmdPlayerLoadedGame(ColourManager.BirdName birdName, NetworkConnectionToClient sender = null)
    {
        if (GameManager.Instance.gameFlowManager.connectedBirds.Contains(birdName))
        {
            return;
        }
        SettingsManager.Instance.AssignBirdToConnection(birdName, sender);
        GameManager.Instance.gameFlowManager.connectedBirds.Add(birdName);

    }

    [ClientRpc]
    public void RpcStartTestGame()
    {
        GameManager.Instance.drawingTestManager.confirmReadyToStart();
    }

    [ClientRpc]
    public void RpcServerIsReady()
    {
        GameManager.Instance.playerFlowManager.serverIsReady = true;
    }
     
    //Broadcast - host_joining_lobby
    [ClientRpc]
    public void RpcHostJoiningLobby()
    {
        SettingsManager.Instance.isHostInLobby = true;
        GameManager.Instance.dcManager.handleHostDisconnection();
    }

    public void RpcPlayerInitializationWrapper(Dictionary<ColourManager.BirdName, string> playerNameMap)
    {
        List<BirdName> playerNameKeys = new List<BirdName>();
        List<string> playerNameValues = new List<string>();
        foreach(KeyValuePair<BirdName, string> keyValuePair in playerNameMap)
        {
            playerNameKeys.Add(keyValuePair.Key);
            playerNameValues.Add(keyValuePair.Value);
        }
        RpcPlayerInitialization(playerNameKeys, playerNameValues);
    }

    //Broadcast - game_initialization
    [ClientRpc]
    public void RpcPlayerInitialization(List<ColourManager.BirdName> playerNameKeys,List<string> playerNameValues)
    {
        Dictionary<BirdName, string> playerNameMap = new Dictionary<BirdName, string>();
        for(int i = 0; i < playerNameKeys.Count; i++)
        {
            playerNameMap.Add(playerNameKeys[i], playerNameValues[i]);
        }
        GameManager.Instance.playerFlowManager.playerNameMap = playerNameMap;
    }

    //Broadcast - cabinet_drawer_is_ready
    [ClientRpc]
    public void RpcUpdateFolderAsReady(FolderUpdateData folderUpdateData)
    {
       
        DrawingRound drawingRound = GameManager.Instance.playerFlowManager.drawingRound;
        ChainData currentCase;
        if (!drawingRound.caseMap.ContainsKey(folderUpdateData.caseID))
        {
            currentCase = new ChainData();
            currentCase.identifier = folderUpdateData.caseID;
            drawingRound.caseMap.Add(folderUpdateData.caseID, currentCase);
        }
        if (folderUpdateData.cabinetIndex == drawingRound.playerCabinetIndex)
        {
            drawingRound.UpdateQueuedFolder(folderUpdateData.caseID, folderUpdateData.roundNumber, folderUpdateData.currentState);

            currentCase = drawingRound.caseMap[folderUpdateData.caseID];
            currentCase.identifier = folderUpdateData.caseID;
            currentCase.currentScoreModifier = folderUpdateData.currentScoreModifier;
            currentCase.currentTaskDuration = folderUpdateData.taskTime;
            currentCase.currentTaskModifiers = folderUpdateData.taskModifiers;
            if (!currentCase.playerOrder.ContainsKey(folderUpdateData.roundNumber))
            {
                currentCase.playerOrder.Add(folderUpdateData.roundNumber, folderUpdateData.lastPlayer);
            }
            else
            {
                currentCase.playerOrder[folderUpdateData.roundNumber] = folderUpdateData.lastPlayer;
            }
        }

        CabinetDrawer currentDrawer = drawingRound.GetCabinet(folderUpdateData.cabinetIndex);
        currentDrawer.setAsReady(folderUpdateData.player);
        GameManager.Instance.playerFlowManager.waitingForPlayersNotification.SetActive(false);
    }

    //Broadcast - access_drawer
    [ClientRpc]
    public void RpcCloseCabinetDrawer(int cabinetIndex)
    {
        GameManager.Instance.playerFlowManager.drawingRound.SetDrawerAsClosed(cabinetIndex);
    }

    [ClientRpc]
    public void RpcRequestConnectionAcknowledgment()
    {
        CmdPlayerLoadedGame(SettingsManager.Instance.birdName);
    }

    //Broadcast - update_phase
    [ClientRpc]
    public void RpcUpdateGamePhase(GamePhase gamePhase)
    {
        GameManager.Instance.playerFlowManager.UpdatePhase(gamePhase);
    }

    //Broadcast - show_slide_rating_visual
    [ClientRpc]
    public void RpcShowSlideRatingVisual(ColourManager.BirdName sender, ColourManager.BirdName receiver)
    {
        if (sender == SettingsManager.Instance.birdName) return;
        GameManager.Instance.playerFlowManager.slidesRound.showPlayerRatingVisual(sender, receiver);
    }

    //Broadcast - slide_speed
    [ClientRpc]
    public void RpcSetSlideSpeed(float slideSpeed)
    {
        GameManager.Instance.playerFlowManager.slidesRound.setSlideSpeed(slideSpeed);
    }

    public void RPCUpdatePlayerNameMapWrapper(Dictionary<BirdName,string> playerNameMap)
    {
        List<BirdName> playerNameKeys = new List<BirdName>();
        List<string> playerNameValues = new List<string>();
        foreach(KeyValuePair<BirdName,string> playerName in playerNameMap)
        {
            playerNameKeys.Add(playerName.Key);
            playerNameValues.Add(playerName.Value);
        }
        RPCUpdatePlayerNameMap(playerNameKeys, playerNameValues);
    }

    [ClientRpc]
    public void RPCUpdatePlayerNameMap(List<BirdName> playerNameKeys, List<string> playerNameValues)
    {
        Dictionary<BirdName, string> playerNameMap = new Dictionary<BirdName, string>();
        for (int i = 0; i < playerNameKeys.Count; i++)
        {
            playerNameMap.Add(playerNameKeys[i], playerNameValues[i]);
        }
        GameManager.Instance.playerFlowManager.playerNameMap = playerNameMap;
    }

    //Broadcast - show_next_slide
    [ClientRpc]
    public void RpcShowNextSlide()
    {
        GameManager.Instance.playerFlowManager.slidesRound.ShowNextSlide();
    }

    public void RpcSlideRoundEndInfoWrapper(Dictionary<int,EndgameCaseData> caseDataMap)
    {
        List<EndgameCaseNetData> caseDataValues = new List<EndgameCaseNetData>();
        foreach(KeyValuePair<int,EndgameCaseData> caseData in caseDataMap)
        {
            caseDataValues.Add(new EndgameCaseNetData(caseData.Value));
        }
        RpcSlideRoundEndInfo(caseDataValues);
    }

    //Broadcast - slide_round_end_info
    [ClientRpc]
    public void RpcSlideRoundEndInfo(List<EndgameCaseNetData> caseNetDataValues)
    {
        List<EndgameCaseData> caseDataValues = new List<EndgameCaseData>();
        foreach(EndgameCaseNetData netData in caseNetDataValues)
        {
            caseDataValues.Add(new EndgameCaseData(netData));
        }
        GameManager.Instance.playerFlowManager.slidesRound.updateChainRatings(caseDataValues);

        GameManager.Instance.playerFlowManager.slidesRound.hasReceivedRatings = true;
        GameManager.Instance.gameDataHandler.CmdTransitionCondition("ratings_loaded:" + SettingsManager.Instance.birdName.ToString());
    }

    public void RpcCreateSlidesFromCaseWrapper(EndgameCaseData caseData)
    {
        RpcCreateSlidesFromCase(new EndgameCaseNetData(caseData));
    }
    [ClientRpc]
    public void RpcCreateSlidesFromCase(EndgameCaseNetData netCaseData)
    {
        GameManager.Instance.playerFlowManager.slidesRound.CreateSlidesFromCase(new EndgameCaseData(netCaseData));
    }

    public void RpcDrawingPhasePositionsWrapper(Dictionary<BirdName, Vector3> birdArmPositionMap)
    {
        List<BirdName> birdArmPositionKeys = new List<BirdName>();
        List<Vector3> birdArmPositionValues = new List<Vector3>();
        foreach(KeyValuePair<BirdName,Vector3> keyValuePair in birdArmPositionMap)
        {
            birdArmPositionKeys.Add(keyValuePair.Key);
            birdArmPositionValues.Add(keyValuePair.Value);
        }
        RpcDrawingPhasePositions(birdArmPositionKeys, birdArmPositionValues);
    }

    //Broadcast - drawing_phase_positions
    [ClientRpc]
    public void RpcDrawingPhasePositions(List<BirdName> birdArmPositionKeys, List<Vector3> birdArmPositionValues)
    {
        Dictionary<BirdName, Vector3> birdArmPositionMap = new Dictionary<BirdName, Vector3>();
        for(int i = 0; i <  birdArmPositionKeys.Count; i++)
        {
            birdArmPositionMap.Add(birdArmPositionKeys[i], birdArmPositionValues[i]);
        }
        foreach (KeyValuePair<BirdName, Vector3> currentBirdArmPosition in birdArmPositionMap)
        {
            GameManager.Instance.playerFlowManager.drawingRound.SetBirdArmTargetPosition(currentBirdArmPosition.Key, currentBirdArmPosition.Value);
        }
    }

    //Broadcast - accusation_seat
    [ClientRpc]
    public void RpcRandomizedSetBirdPosition(int randomizedIndex, BirdName birdName)
    {
        GameManager.Instance.playerFlowManager.slidesRound.initializeGalleryBird(randomizedIndex, birdName);
        GameManager.Instance.playerFlowManager.accoladesRound.initializeAccoladeBirdRow(randomizedIndex, birdName);
    }

    //Broadcast - update_timer
    [ClientRpc]
    public void RpcUpdateTimer(float newTime)
    {
        GameManager.Instance.playerFlowManager.timeRemainingText.gameObject.SetActive(true);
        GameManager.Instance.playerFlowManager.loadingCircleObject.SetActive(false);
        GameManager.Instance.playerFlowManager.currentTimeInRound = newTime;
    }

    //Broadcast - update_queued_folder_visuals
    [ClientRpc]
    public void RpcUpdateQueuedFolderVisuals(int cabinetID, List<BirdName> queuedFolderColours)
    {
        GameManager.Instance.playerFlowManager.drawingRound.cabinetDrawerMap[cabinetID].setQueuedFolders(queuedFolderColours);
    }

    //Broadcast - reaction
    [ClientRpc]
    public void RpcShowReaction(BirdName birdName, Reaction reaction)
    {
        GameManager.Instance.playerFlowManager.slidesRound.showReaction(birdName, reaction);
    }

    //Broadcast - case_difficulty_values
    [ClientRpc]
    public void RpcCaseDifficultyValues(int caseID, int prefixDifficulty, int nounDifficulty)
    {
        DrawingRound drawingRound = GameManager.Instance.playerFlowManager.drawingRound;
        if (!drawingRound.caseMap.ContainsKey(caseID))
        {
            Debug.LogError("Case map is missing case[" + caseID.ToString() + "] cannot append word difficulty values.");
            return;
        }
        ChainData currentCase = drawingRound.caseMap[caseID];
        if (currentCase.correctWordIdentifierMap.ContainsKey(1))
        {
            CaseWordData correctPrefix = GameDataManager.Instance.GetWord(currentCase.correctWordIdentifierMap[1]);
            correctPrefix.difficulty = prefixDifficulty;
        }
        if (currentCase.correctWordIdentifierMap.ContainsKey(2))
        {
            CaseWordData correctNoun = GameDataManager.Instance.GetWord(currentCase.correctWordIdentifierMap[2]);
            correctNoun.difficulty = nounDifficulty;
        }
    }

    //Broadcast - request_stats
    [ClientRpc]
    public void RpcSendStats()
    {
        StatTracker.Instance.SendStatsToServer();
    }

    public void RpcPlayerStatRolesWrapper(Dictionary<BirdName, AccoladesStatManager.StatRole> statRoleMap)
    {
        List<BirdName> statRoleKeys = new List<BirdName>();
        List<AccoladesStatManager.StatRole> statRoleValues = new List<AccoladesStatManager.StatRole>();
        foreach(KeyValuePair<BirdName,AccoladesStatManager.StatRole> pair in statRoleMap)
        {
            statRoleKeys.Add(pair.Key);
            statRoleValues.Add(pair.Value);
        }
        RpcPlayerStatRoles(statRoleKeys, statRoleValues);
    }
    //Broadcast - player_stat_roles
    [ClientRpc]
    public void RpcPlayerStatRoles(List<BirdName> statRoleKeys, List<AccoladesStatManager.StatRole> statRoleValues)
    {
        Dictionary<BirdName, AccoladesStatManager.StatRole> statRoleMap = new Dictionary<BirdName, AccoladesStatManager.StatRole>();
        for(int i = 0; i < statRoleKeys.Count; i++)
        {
            statRoleMap.Add(statRoleKeys[i], statRoleValues[i]);
        }
        GameManager.Instance.playerFlowManager.accoladesRound.SetPlayerAccoladeCards(statRoleMap);
        GameManager.Instance.playerFlowManager.resultsRound.SetPlayerStatRoles(statRoleMap);
        GameManager.Instance.gameDataHandler.CmdTransitionCondition("stats_loaded:" + SettingsManager.Instance.birdName.ToString());
    }

    //Broadcast - set_cabinet_owner
    [ClientRpc]
    public void RpcSetCabinetOwner(int cabinetIndex, BirdName player)
    {
        GameManager.Instance.playerFlowManager.drawingRound.cabinetDrawerMap[cabinetIndex].setCabinetOwner(player);
    }

    //Broadcast - restart_game
    [ClientRpc]
    public void RpcRestartGame()
    {
        SceneManager.LoadScene("Game");
    }

    //Broadcast - show_quick_results
    [ClientRpc]
    public void RpcShowQuickResults()
    {
        GameManager.Instance.playerFlowManager.slidesRound.ShowFastResults();
    }

    [ClientRpc]
    public void RpcSendDisconnectionNotification(BirdName player)
    {
        GameManager.Instance.dcManager.OnPlayerLeftRoom(player);
    }

    [ClientRpc]
    public void RpcSendWords(List<CaseWordData> customWords)
    {
        GameDataManager.Instance.RefreshWords(customWords);
    }

    [ClientRpc]
    public void RpcActivateCasePile()
    {
        GameManager.Instance.playerFlowManager.drawingRound.newCaseCabinet.gameObject.SetActive(true);
        GameManager.Instance.playerFlowManager.drawingRound.newCaseCabinet.Activate();
    }
    [ClientRpc]
    public void RpcUpdateNumberOfCases(int numberOfCases)
    {
        GameManager.Instance.playerFlowManager.drawingRound.UpdateNumberOfCases(numberOfCases);
    }

    public void RpcSendEndgameCaseDataWrapper(List<EndgameCaseData> endgameCases)
    {
        List<EndgameCaseNetData> netCases = new List<EndgameCaseNetData>();
        foreach(EndgameCaseData endgameCase in endgameCases)
        {
            netCases.Add(new EndgameCaseNetData(endgameCase));
        }
        RpcSendEndgameCaseData(netCases);
    }
    [ClientRpc]
    public void RpcSendEndgameCaseData(List<EndgameCaseNetData> netDataCases)
    {
        GameManager.Instance.playerFlowManager.slidesRound.caseDataMap.Clear();
        foreach (EndgameCaseNetData netDataCase in netDataCases)
        {
            GameManager.Instance.playerFlowManager.slidesRound.caseDataMap.Add(netDataCase.identifier, new EndgameCaseData(netDataCase));
        }
        CmdTransitionCondition("endgame_data_loaded:" + SettingsManager.Instance.birdName.ToString());
    }

    //Target - initial_cabinet_prompt_contents
    [TargetRpc]
    public void TargetInitialCabinetPromptContents(NetworkConnectionToClient target, int caseID, string prompt, bool requiresConfirmation)
    {
        GameManager.Instance.playerFlowManager.drawingRound.SetInitialPrompt(caseID, prompt);
    }

    public void TargetPossibleWordsWrapper(NetworkConnectionToClient target, int caseID, Dictionary<int, List<string>> possibleWords)
    {
        List<int> possibleWordKeys = new List<int>();
        List<List<string>> possibleWordValues = new List<List<string>>();
        foreach(KeyValuePair<int,List<string>> keyValuePair in possibleWords)
        {
            possibleWordKeys.Add(keyValuePair.Key);
            possibleWordValues.Add(keyValuePair.Value);
        }
        TargetPossibleWords(target, caseID, possibleWordKeys, possibleWordValues);
    }

    //Target - possible_words
    [TargetRpc]
    public void TargetPossibleWords(NetworkConnectionToClient target, int caseID, List<int> possibleWordKeys,List<List<string>> possibleWordValues)
    {
        Dictionary<int, List<string>> possibleWords = new Dictionary<int, List<string>>();
        for(int i = 0; i < possibleWordKeys.Count; i++)
        {
            possibleWords.Add(possibleWordKeys[i], possibleWordValues[i]);
        }
        DrawingRound drawingRound = GameManager.Instance.playerFlowManager.drawingRound;
        if (!drawingRound.caseMap.ContainsKey(caseID))
        {
            ChainData newCase = new ChainData();
            newCase.identifier = caseID;
            drawingRound.caseMap.Add(caseID, newCase);
        }
        drawingRound.caseMap[caseID].possibleWordsMap = possibleWords;
    }

    //Target - cabinet_prompt_contents
    [TargetRpc]
    public void TargetCabinetPromptContents(NetworkConnectionToClient target, int caseID, int tab, BirdName author, string prompt)
    {
        GameManager.Instance.playerFlowManager.drawingRound.SetPrompt(caseID, tab, author, prompt, 0f);
    }

    //Target - force_submit
    [TargetRpc]
    public void TargetForceSubmit(NetworkConnectionToClient target)
    {
        GameManager.Instance.playerFlowManager.drawingRound.Submit(true);
    }

    //Target - empty_drawing
    [TargetRpc]
    public void TargetEmptyDrawing(NetworkConnectionToClient target, int cabinetID, int round, BirdName author)
    {
        GameManager.Instance.playerFlowManager.drawingRound.HandleEmptyDrawingToPlayer(cabinetID, round, author);
    }

    //Target - assign_cabinet_to_player
    [TargetRpc]
    public void TargetAssignCabinetToPlayer(NetworkConnectionToClient target, int cabinetIndex)
    {
        GameManager.Instance.playerFlowManager.drawingRound.playerCabinetIndex = cabinetIndex;
    }

    //Target - chain_prompt
    [TargetRpc]
    public void TargetChainPrompt(NetworkConnectionToClient target, int caseID, int round, BirdName birdName, string prompt, float timeTaken)
    {
        GameManager.Instance.playerFlowManager.setChainPrompt(caseID, round, birdName, prompt, timeTaken);
    }

    //Target - player_initialize
    [TargetRpc]
    public void TargetInitializePlayer(NetworkConnectionToClient target, BirdName playerBird, PlayerData player)
    {
        GameManager.Instance.playerFlowManager.instructionRound.InitializePlayer(playerBird, player);
    }

    [TargetRpc]
    public void TargetActivatePileOFiles(NetworkConnectionToClient target)
    {
        GameManager.Instance.playerFlowManager.drawingRound.newCaseCabinet.Activate();
    }
    [TargetRpc]
    public void TargetSendCaseChoices(NetworkConnectionToClient target, List<CaseChoiceNetData> caseChoices)
    {
        GameManager.Instance.playerFlowManager.drawingRound.ShowCaseChoices(caseChoices);
    }

    [TargetRpc]
    public void TargetStartChoiceDrawing(NetworkConnectionToClient target, int cabinetIndex, int caseID, string prompt, TaskData taskData, float currentModifier, TaskData.TaskModifier drawingBoxModifier)
    {
        float modifierDecrement = SettingsManager.Instance.gameMode.scoreModifierDecrement;
        GameManager.Instance.playerFlowManager.drawingRound.StartChoiceCaseDrawing(cabinetIndex, caseID, prompt, taskData.duration, currentModifier, modifierDecrement, drawingBoxModifier);
    }

    [TargetRpc]
    public void TargetSendCompiledDrawing(NetworkConnectionToClient target, DrawingData drawingData)
    {
        DrawingRound drawingRound = GameManager.Instance.playerFlowManager.drawingRound;
        if (!drawingRound.caseMap.ContainsKey(drawingData.caseID))
        {
            drawingRound.caseMap.Add(drawingData.caseID, new ChainData());
            drawingRound.caseMap[drawingData.caseID].addDrawing(drawingData.round, drawingData);
        }
        else
        {
            drawingRound.caseMap[drawingData.caseID].drawings[drawingData.round].visuals.AddRange(drawingData.visuals);
        }
        
    }

    //Command - transition_condition
    [Command(requiresAuthority = false)]
    public void CmdTransitionCondition(string transitionCondition)
    {
        GameManager.Instance.gameFlowManager.resolveTransitionCondition(transitionCondition);
    }

    //Command - prompt
    [Command(requiresAuthority = false)]
    public void CmdPrompt(int caseID, int tab, BirdName author, string prompt, float timeTaken)
    {
        GameManager.Instance.playerFlowManager.drawingRound.SetPrompt(caseID, tab, author, prompt, timeTaken);
        GameManager.Instance.gameFlowManager.resolveTransitionCondition("prompt_submitted:" + author);
    }

    //Command - access_drawer
    [Command(requiresAuthority = false)]
    public void CmdCloseDrawer(int cabinetID)
    {
        GameManager.Instance.playerFlowManager.drawingRound.SetDrawerAsClosed(cabinetID);
        GameManager.Instance.gameDataHandler.RpcCloseCabinetDrawer(cabinetID);
    }

    //Command - prompt_guess
    [Command(requiresAuthority = false)]
    public void CmdPromptGuess(GuessData guessData, int caseID, float timeTaken)
    {
        GameManager.Instance.playerFlowManager.addGuessPrompt(guessData, caseID, timeTaken);
    }

    //Command - drawing_arm_position
    [Command(requiresAuthority = false)]
    public void CmdDrawingArmPosition(BirdName birdName, Vector3 currentPosition)
    {
        GameManager.Instance.gameFlowManager.SetBirdArmPosition(birdName, currentPosition);
    }

    //Command - rate_slide
    [Command(requiresAuthority = false)]
    public void CmdRateSlide(int caseID, int tab, BirdName sender, BirdName receiver)
    {
        GameManager.Instance.playerFlowManager.addToRating(caseID, tab, sender, receiver);
    }

    //Command - speed_up_slides
    [Command(requiresAuthority = false)]
    public void CmdSpeedUpSlides()
    {
        GameManager.Instance.playerFlowManager.slidesRound.increaseSlideSpeed();
    }

    //Command - empty_drawing
    [Command(requiresAuthority = false)]
    public void CmdEmptyDrawing(BirdName birdName, int cabinetID, int round)
    {
        GameManager.Instance.playerFlowManager.drawingRound.HandleEmptyDrawingToServer(birdName, cabinetID, round);
    }

    //Command - dequeue_front_case
    [Command(requiresAuthority = false)]
    public void CmdDequeueFrontCase(BirdName birdName)
    {
        GameManager.Instance.gameFlowManager.dequeueFrontCase(birdName);
    }

    //Command - ready_for_case_details
    [Command(requiresAuthority = false)]
    public void CmdReadyForCaseDetails(int caseID, BirdName birdName)
    {
        GameManager.Instance.playerFlowManager.drawingRound.caseMap[caseID].sendCaseDetailsToClient(birdName);
    }

    //Command - reaction
    [Command(requiresAuthority = false)]
    public void CmdReaction(BirdName birdName, Reaction reaction)
    {
        StatTracker.Instance.AddToReactionCounter(birdName, reaction.ToString(), GameManager.Instance.playerFlowManager.slidesRound.currentSlideContentIndex);
        GameManager.Instance.gameDataHandler.RpcShowReaction(birdName, reaction);

    }

    //Command - player_stats
    [Command(requiresAuthority = false)]
    public void CmdPlayerStats(bool pencilUsed, bool colourMarkerUsed, bool lightMarkerUsed, bool eraserUsed, BirdName birdName, float timeInCabinetArea, float guessingTime, int stickyCount, int drawingsTrashed, int numberOfPlayersLiked, int numberOfLikesGiven, bool usedStickies, float totalDistanceMoved)
    {
        Dictionary<DrawingController.DrawingToolType, bool> toolUsageMap = new Dictionary<DrawingController.DrawingToolType, bool>();
        toolUsageMap.Add(DrawingController.DrawingToolType.pencil, pencilUsed);
        toolUsageMap.Add(DrawingController.DrawingToolType.colour_marker, colourMarkerUsed);
        toolUsageMap.Add(DrawingController.DrawingToolType.light_marker, lightMarkerUsed);
        toolUsageMap.Add(DrawingController.DrawingToolType.eraser, eraserUsed);

        GameManager.Instance.playerFlowManager.accoladesRound.playerStatsManager.AddPlayersideStats(birdName, timeInCabinetArea, guessingTime, stickyCount, drawingsTrashed, toolUsageMap, numberOfPlayersLiked, numberOfLikesGiven, usedStickies, totalDistanceMoved);

    }

    [Command(requiresAuthority = false)]
    public void CmdRequestCaseChoice(BirdName birdName)
    {
        List<CaseChoiceData> allCaseChoices = GameDataManager.Instance.GetCaseChoices(SettingsManager.Instance.gameMode.caseChoiceIdentifiers);
        int numberOfPlayers = GameManager.Instance.gameFlowManager.GetNumberOfConnectedPlayers();
        List<CaseChoiceData> validCaseChoices = new List<CaseChoiceData>();
        //Iterate over all case choices and add to the valid case choice list based on what is possible to complete and the frequency set in the choice
        foreach (CaseChoiceData caseChoice in allCaseChoices)
        {
            if(caseChoice.numberOfTasks <= numberOfPlayers)
            {
                for(int i = 0; i < caseChoice.selectionFrequency; i++)
                {
                    validCaseChoices.Add(caseChoice);
                }
            }
        }
        List<CaseChoiceData> caseChoices = new List<CaseChoiceData>();
        caseChoices.Add(validCaseChoices.OrderBy(x => Guid.NewGuid()).ToList()[0]);
        caseChoices.Add(validCaseChoices.OrderBy(x => Guid.NewGuid()).ToList()[0]);
        caseChoices.Add(validCaseChoices.OrderBy(x => Guid.NewGuid()).ToList()[0]);

        List<CaseChoiceNetData> caseChoiceDatas = new List<CaseChoiceNetData>();
        foreach(CaseChoiceData caseChoice in caseChoices)
        {
            //Set the word options for the choices
            switch (SettingsManager.Instance.gameMode.wordDistributionMode)
            {
                case GameModeData.WordDistributionMode.random:
                    caseChoiceDatas.Add(GameManager.Instance.gameFlowManager.wordManager.PopulateChoiceWords(caseChoice));
                    break;
            }
        }
        SettingsManager.Instance.gameMode.casesRemaining--;
        RpcUpdateNumberOfCases(SettingsManager.Instance.gameMode.casesRemaining);
        TargetSendCaseChoices(SettingsManager.Instance.GetConnection(birdName), caseChoiceDatas);
    }

    [Command(requiresAuthority = false)]
    public void CmdSendDrawing(DrawingData drawingData)
    {
        if (!GameManager.Instance.playerFlowManager.drawingRound.caseMap.ContainsKey(drawingData.caseID))
        {
            Debug.LogError("Could not add drawing data because the corresponding case["+drawingData.caseID.ToString()+"] was missing.");
            return;
        }
        ChainData currentCase = GameManager.Instance.playerFlowManager.drawingRound.caseMap[drawingData.caseID];
        if (!currentCase.drawings.ContainsKey(drawingData.round))
        {
            currentCase.drawings.Add(drawingData.round, drawingData);
        }
        else
        {
            currentCase.drawings[drawingData.round] = drawingData;
        }

        RpcSendDrawing(drawingData);
    }

    [Command(requiresAuthority =false)]
    public void CmdChooseCase(BirdName birdName, CaseChoiceNetData choiceData)
    {
        int caseID = GameManager.Instance.gameFlowManager.CreateCaseFromChoice(birdName, choiceData);
        
        //Send the base task
        ChainData newCase = GameManager.Instance.playerFlowManager.drawingRound.caseMap[caseID];
        TaskData baseTaskData = newCase.taskQueue[0];

        //Start the case immediately instead of queueing it?
        TaskModifier drawingBoxModifier = TaskModifier.standard;
        foreach (TaskModifier modifier in baseTaskData.modifiers)
        {
            switch (modifier)
            {
                case TaskModifier.shrunk:
                case TaskModifier.thirds_first:
                case TaskModifier.thirds_second:
                case TaskModifier.thirds_third:
                case TaskModifier.top:
                case TaskModifier.bottom:
                case TaskModifier.top_left:
                case TaskModifier.top_right:
                case TaskModifier.bottom_left:
                case TaskModifier.bottom_right:
                    drawingBoxModifier = modifier;
                    break;
            }
        }
        int birdCabinetIndex = GameManager.Instance.gameFlowManager.playerCabinetMap[birdName];
        TargetStartChoiceDrawing(SettingsManager.Instance.GetConnection(birdName), birdCabinetIndex, caseID, choiceData.correctPrompt, baseTaskData, newCase.currentScoreModifier, drawingBoxModifier);
        
    }

    [Command(requiresAuthority =false)]
    public void CmdRequestNextCase(BirdName birdName)
    {
        GameManager.Instance.playerFlowManager.drawingRound.SendNextInQueue(birdName);
    }
    [Command(requiresAuthority =false)]
    public void CmdTransitionCase(int caseID, float inScoreModifier)
    {
        ChainData transitioningCase = GameManager.Instance.playerFlowManager.drawingRound.caseMap[caseID];
        TaskData transitioningTask = transitioningCase.taskQueue[transitioningCase.currentRound-1];
        float changeInScoreModifier = inScoreModifier - transitioningCase.currentScoreModifier;
        transitioningTask.timeModifierDecrement += changeInScoreModifier;
        transitioningCase.currentScoreModifier = inScoreModifier;
        GameManager.Instance.gameFlowManager.SendTaskToNextPlayer(caseID);
    }

    [ClientRpc]
    public void RpcSendDrawing(DrawingData drawingData)
    {
        DrawingRound drawingRound = GameManager.Instance.playerFlowManager.drawingRound;
        if (!drawingRound.caseMap.ContainsKey(drawingData.caseID))
        {
            drawingRound.caseMap.Add(drawingData.caseID, new ChainData());
        }
        drawingRound.caseMap[drawingData.caseID].addDrawing(drawingData.round, drawingData);
        GameManager.Instance.gameDataHandler.CmdTransitionCondition("drawing_receipt:" + SettingsManager.Instance.birdName.ToString() + ":" + drawingData.caseID.ToString());

    }
}
