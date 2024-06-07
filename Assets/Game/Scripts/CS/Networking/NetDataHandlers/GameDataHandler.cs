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
using static Org.BouncyCastle.Asn1.Cmp.Challenge;

public class GameDataHandler : NetworkBehaviour
{
    [Command(requiresAuthority = false)]
    public void CmdSetPlayerBird(string playerID, ColourManager.BirdName birdName, NetworkConnectionToClient sender = null)
    {
        SettingsManager.Instance.playerNameMap.Add(birdName, playerID);
        SettingsManager.Instance.birdConnectionMap.Add(birdName, sender);
    }

    [Command(requiresAuthority = false)]
    public void CmdPlayerLoadedGame(ColourManager.BirdName birdName)
    {
        if (GameManager.Instance.gameFlowManager.connectedPlayers.Contains(birdName))
        {
            return;
        }
        GameManager.Instance.gameFlowManager.totalPlayersConnected++;
        GameManager.Instance.gameFlowManager.connectedPlayers.Add(birdName);
        GameManager.Instance.gameFlowManager.SetCabinetOwnership();
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

    public void RpcPlayerInitializationWrapper(int numberOfRounds, Dictionary<ColourManager.BirdName, string> playerNameMap)
    {
        List<BirdName> playerNameKeys = new List<BirdName>();
        List<string> playerNameValues = new List<string>();
        foreach(KeyValuePair<BirdName, string> keyValuePair in playerNameMap)
        {
            playerNameKeys.Add(keyValuePair.Key);
            playerNameValues.Add(keyValuePair.Value);
        }
        RpcPlayerInitialization(numberOfRounds, playerNameKeys, playerNameValues);
    }

    //Broadcast - game_initialization
    [ClientRpc]
    public void RpcPlayerInitialization(int numberOfRounds, List<ColourManager.BirdName> playerNameKeys,List<string> playerNameValues)
    {
        Dictionary<BirdName, string> playerNameMap = new Dictionary<BirdName, string>();
        for(int i = 0; i < playerNameKeys.Count; i++)
        {
            playerNameMap.Add(playerNameKeys[i], playerNameValues[i]);
        }
        GameManager.Instance.playerFlowManager.numberOfCabinetRounds = numberOfRounds;
        GameManager.Instance.playerFlowManager.playerNameMap = playerNameMap;
    }

    //Broadcast - cabinet_drawer_is_ready
    [ClientRpc]
    public void RpcUpdateFolderAsReady(FolderUpdateData folderUpdateData)
    {
        DrawingRound drawingRound = GameManager.Instance.playerFlowManager.drawingRound;

        if (folderUpdateData.cabinetIndex == drawingRound.playerCabinetIndex)
        {
            Debug.LogError("Setting currentRound["+folderUpdateData.roundNumber.ToString()+"] for case["+folderUpdateData.caseID.ToString()+"]");
            drawingRound.currentState = folderUpdateData.currentState;
            if(!drawingRound.currentRoundMap.ContainsKey(folderUpdateData.caseID))
            {
                drawingRound.currentRoundMap.Add(folderUpdateData.caseID, folderUpdateData.roundNumber);
            }
            else
            {
                drawingRound.currentRoundMap[folderUpdateData.caseID] = folderUpdateData.roundNumber;
            }
        }

        if (!drawingRound.caseMap.ContainsKey(folderUpdateData.caseID))
        {
            ChainData newCase = new ChainData();
            newCase.identifier = folderUpdateData.caseID;
            drawingRound.caseMap.Add(folderUpdateData.caseID, newCase);
        }
        drawingRound.caseMap[folderUpdateData.caseID].currentScoreModifier = folderUpdateData.currentScoreModifier;
        drawingRound.caseMap[folderUpdateData.caseID].currentTaskDuration = folderUpdateData.taskTime;

        drawingRound.cabinetDrawerMap[folderUpdateData.cabinetIndex].currentChainData = drawingRound.caseMap[folderUpdateData.caseID];
        drawingRound.cabinetDrawerMap[folderUpdateData.cabinetIndex].currentChainData.identifier = folderUpdateData.caseID;
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
        if (currentCase.correctWordsMap.ContainsKey(1))
        {
            currentCase.correctWordsMap[1].difficulty = prefixDifficulty;
        }
        if (currentCase.correctWordsMap.ContainsKey(2))
        {
            currentCase.correctWordsMap[2].difficulty = nounDifficulty;
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
        GameManager.Instance.playerFlowManager.drawingRound.SetInitialPrompt(caseID, prompt, requiresConfirmation);
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
    public void TargetSendCaseChoices(NetworkConnectionToClient target, List<CaseChoiceData> caseChoices)
    {
        GameManager.Instance.playerFlowManager.drawingRound.ShowCaseChoices(caseChoices[0], caseChoices[1]);
    }

    [TargetRpc]
    public void TargetStartChoiceDrawing(NetworkConnectionToClient target, int caseID, string prompt, TaskData taskData, float currentModifier)
    {
        GameManager.Instance.playerFlowManager.drawingRound.UpdateCaseRound(caseID, 1);
        float modifierDecrement = SettingsManager.Instance.gameMode.scoreModifierDecrement;
        GameManager.Instance.playerFlowManager.drawingRound.StartChoiceCaseDrawing(caseID, prompt, taskData.duration, currentModifier, modifierDecrement);
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

    public void CmdPromptGuessWrapper(BirdName birdName, Dictionary<int, string> guessWords, int caseID)
    {
        List<int> guessWordKeys = new List<int>();
        List<string> guessWordValues = new List<string>();
        foreach(KeyValuePair<int,string> pair in guessWords)
        {
            guessWordKeys.Add(pair.Key);
            guessWordValues.Add(pair.Value);
        }
        CmdPromptGuess(birdName, guessWordKeys, guessWordValues, caseID);
    }
    //Command - prompt_guess
    [Command(requiresAuthority = false)]
    public void CmdPromptGuess(BirdName birdName, List<int> guessWordKeys, List<string> guessWordValues, int caseID)
    {
        if(SettingsManager.Instance.gameMode.caseDeliveryMode == CaseDeliveryMode.free_for_all && SettingsManager.Instance.gameMode.numberOfCases > 0)
        {
            //GameManager.Instance.gameDataHandler.TargetActivatePileOFiles(SettingsManager.Instance.birdConnectionMap[birdName]);
        }

        Dictionary<int, string> guessWords = new Dictionary<int, string>();
        for(int i = 0; i < guessWordKeys.Count; i++)
        {
            guessWords.Add(guessWordKeys[i], guessWordValues[i]);
        }
        GameManager.Instance.playerFlowManager.addGuessPrompt(birdName, guessWords, caseID);
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
        List<CaseChoiceData> caseChoices = SettingsManager.Instance.gameMode.pileCaseChoices.Where(x => x.numberOfTasks <= GameManager.Instance.gameFlowManager.gamePlayers.Count).OrderBy(x => Guid.NewGuid()).Take(2).ToList();
        foreach(CaseChoiceData caseChoice in caseChoices)
        {
            //Set the word options for the choices
            switch (SettingsManager.Instance.gameMode.wordDistributionMode)
            {
                case GameModeData.WordDistributionMode.random:
                    GameManager.Instance.gameFlowManager.wordManager.PopulateChoiceWords(caseChoice);
                    break;
            }
        }
        TargetSendCaseChoices(SettingsManager.Instance.birdConnectionMap[birdName], caseChoices);
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
    public void CmdChooseCase(BirdName birdName, CaseChoiceData choiceData)
    {
        int caseID = GameManager.Instance.gameFlowManager.CreateCaseFromChoice(birdName, choiceData);
        SettingsManager.Instance.gameMode.numberOfCases--;
        //Send the base task
        ChainData newCase = GameManager.Instance.playerFlowManager.drawingRound.caseMap[caseID];
        TaskData baseTaskData = newCase.taskQueue[0];

        //Start the case immediately instead of queueing it?
        RpcUpdateNumberOfCases(SettingsManager.Instance.gameMode.numberOfCases);
        TargetStartChoiceDrawing(SettingsManager.Instance.birdConnectionMap[birdName], caseID, choiceData.correctPrompt, baseTaskData, newCase.currentScoreModifier);
        
    }

    [Command(requiresAuthority =false)]
    public void CmdRequestNextCase(BirdName birdName)
    {
        GameManager.Instance.playerFlowManager.drawingRound.SendNextInQueue(birdName);
    }
    [Command(requiresAuthority =false)]
    public void CmdTransitionCase(int caseID, float inScoreModifier)
    {
        GameManager.Instance.playerFlowManager.drawingRound.caseMap[caseID].currentScoreModifier = inScoreModifier;
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
