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
        SettingsManager.Instance.AssignBirdToPlayer(birdName, playerID);
        SettingsManager.Instance.BroadcastBirdAssignmentInGame();
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
    public void RpcPlayerInitialization(List<ColourManager.BirdName> playerNameKeys, List<string> playerNameValues)
    {
        if(playerNameKeys.Count != playerNameValues.Count)
        {
            Debug.LogError("ERROR: Invalid number of arguments provided to RpcPlayerInitialization from wrapper.");
            return;
        }
        Dictionary<BirdName, string> playerNameMap = new Dictionary<BirdName, string>();
        for(int i = 0; i < playerNameKeys.Count; i++)
        {
            playerNameMap.Add(playerNameKeys[i], playerNameValues[i]);
        }
        GameManager.Instance.playerFlowManager.playerNameMap = playerNameMap;
    }

    [ClientRpc]
    public void RpcShowDayResult()
    {
        GameManager.Instance.playerFlowManager.slidesRound.ShowDayResult();
    }

    [ClientRpc]
    public void RpcUpdateGameDay(int newDayValue)
    {
        GameManager.Instance.playerFlowManager.currentDay = newDayValue;
        GameManager.Instance.gameDataHandler.CmdTransitionCondition("day_loaded:" + SettingsManager.Instance.birdName);
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

        bool updateFolderIsInMyCabinet = folderUpdateData.cabinetIndex == drawingRound.playerCabinetIndex;
        if (updateFolderIsInMyCabinet)
        {
            drawingRound.UpdateQueuedFolder(folderUpdateData.caseID, folderUpdateData.roundNumber, folderUpdateData.currentState, folderUpdateData.wordCategory);

            currentCase = drawingRound.caseMap[folderUpdateData.caseID];
            currentCase.PopulateFromFolderUpdateData(folderUpdateData);
        }

        CabinetDrawer currentDrawer = drawingRound.GetCabinet(folderUpdateData.cabinetIndex);
        if(currentDrawer == null)
        {
            Debug.LogError("Could not set drawer as ready because the provided cabinet index["+folderUpdateData.cabinetIndex.ToString()+"] did not exist for the drawing round.");
            return;
        }
        currentDrawer.setAsReady(folderUpdateData.player);
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
        bool senderIsMe = sender == SettingsManager.Instance.birdName;
        if (senderIsMe) return;
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
        if(playerNameKeys.Count != playerNameValues.Count)
        {
            Debug.LogError("Could not update player name map because the keys and values count didn't match.");
            return;
        }
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
        GameManager.Instance.playerFlowManager.slidesRound.UpdateCaseRatings(caseDataValues);

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
        if(birdArmPositionKeys.Count != birdArmPositionValues.Count)
        {
            Debug.LogError("ERROR[RpcDrawingPhasePositions]: Keys and Values do not have the same count.");
            return;
        }
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

    public void RpcStorePhasePositionsWrapper(Dictionary<BirdName, Vector3> longArmPositionMap)
    {
        List<BirdName> longArmPositionKeys = new List<BirdName>();
        List<Vector3> longArmPositionValues = new List<Vector3>();
        foreach (KeyValuePair<BirdName, Vector3> keyValuePair in longArmPositionMap)
        {
            longArmPositionKeys.Add(keyValuePair.Key);
            longArmPositionValues.Add(keyValuePair.Value);
        }
        RpcStorePhasePositions(longArmPositionKeys, longArmPositionValues);
    }

    //Broadcast - drawing_phase_positions
    [ClientRpc]
    public void RpcStorePhasePositions(List<BirdName> longArmPositionKeys, List<Vector3> longArmPositionValues)
    {
        if (longArmPositionKeys.Count != longArmPositionValues.Count)
        {
            Debug.LogError("ERROR[RpcStorePhasePositions]: Keys and Values do not have the same count.");
            return;
        }
        Dictionary<BirdName, Vector3> longArmPositionMap = new Dictionary<BirdName, Vector3>();
        for (int i = 0; i < longArmPositionKeys.Count; i++)
        {
            longArmPositionMap.Add(longArmPositionKeys[i], longArmPositionValues[i]);
        }
        foreach (KeyValuePair<BirdName, Vector3> currentBirdArmPosition in longArmPositionMap)
        {
            GameManager.Instance.playerFlowManager.storeRound.SetLongArmTargetPosition(currentBirdArmPosition.Key, currentBirdArmPosition.Value);
        }
    }

    //Broadcast - accusation_seat
    [ClientRpc]
    public void RpcRandomizedSetBirdIndex(int randomizedIndex, BirdName birdName)
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
        if(!GameManager.Instance.playerFlowManager.drawingRound.cabinetDrawerMap.ContainsKey(cabinetID))
        {
            Debug.LogError("ERROR[RpcUpdateQueuedFolderVisuals]: Could not find matching cabinet["+cabinetID.ToString()+"]");
            return;
        }
        GameManager.Instance.playerFlowManager.drawingRound.cabinetDrawerMap[cabinetID].setQueuedFolders(queuedFolderColours);
    }

    
    public void RpcSendStoreItemWrapper(StoreItemData storeItemData)
    {
        StoreItemNetData netData = new StoreItemNetData(storeItemData);
        RpcSendStoreItem(netData);
    }

    [ClientRpc]
    public void RpcSendStoreItem(StoreItemNetData netData)
    {
        StoreItemData storeData = new StoreItemData(netData);
        GameManager.Instance.playerFlowManager.storeRound.CreateStoreItem(storeData);
    }

    public void RpcSendValueStoreItemWrapper(ValueStoreItemData valueItemData)
    {
        ValueStoreItemNetData valueNetData = new ValueStoreItemNetData(valueItemData);
        RpcSendValueStoreItem(valueNetData);
    }

    [ClientRpc]
    public void RpcSendValueStoreItem(ValueStoreItemNetData netData)
    {
        ValueStoreItemData valueData = new ValueStoreItemData(netData);
        GameManager.Instance.playerFlowManager.storeRound.CreateStoreItem(valueData);
    }

    public void RpcSendChargeStoreItemWrapper(ChargedStoreItemData chargeItemData)
    {
        ChargeStoreItemNetData chargeNetData = new ChargeStoreItemNetData(chargeItemData);
        RpcSendChargeStoreItem(chargeNetData);
    }

    [ClientRpc]
    public void RpcSendChargeStoreItem(ChargeStoreItemNetData netData)
    {
        ChargedStoreItemData chargeData = new ChargedStoreItemData(netData);
        GameManager.Instance.playerFlowManager.storeRound.CreateStoreItem(chargeData);
    }

    public void RpcSendMarkerStoreItemWrapper(MarkerStoreItemData markerItemData)
    {
        MarkerStoreItemNetData markerNetData = new MarkerStoreItemNetData(markerItemData);
        RpcSendMarkerStoreItem(markerNetData);
    }

    [ClientRpc]
    public void RpcSendMarkerStoreItem(MarkerStoreItemNetData netData)
    {
        MarkerStoreItemData markerData = new MarkerStoreItemData(netData);
        GameManager.Instance.playerFlowManager.storeRound.CreateStoreItem(markerData);
    }

    public void RpcSendUpgradeStoreItemWrapper(CaseUpgradeStoreItemData upgradeItemData)
    {
        CaseUpgradeStoreItemNetData upgradeNetData = new CaseUpgradeStoreItemNetData(upgradeItemData);
        RpcSendUpgradeStoreItem(upgradeNetData);
    }

    [ClientRpc]
    public void RpcSendUpgradeStoreItem(CaseUpgradeStoreItemNetData netData)
    {
        CaseUpgradeStoreItemData upgradeData = new CaseUpgradeStoreItemData(netData);
        GameManager.Instance.playerFlowManager.storeRound.CreateStoreItem(upgradeData);
    }

    public void RpcSendUnlockStoreItemWrapper(CaseUnlockStoreItemData unlockItemData)
    {
        CaseUnlockStoreItemNetData unlockNetData = new CaseUnlockStoreItemNetData(unlockItemData);
        RpcSendUnlockStoreItem(unlockNetData);
    }

    [ClientRpc]
    public void RpcSendUnlockStoreItem(CaseUnlockStoreItemNetData netData)
    {
        CaseUnlockStoreItemData unlockData = new CaseUnlockStoreItemData(netData);
        GameManager.Instance.playerFlowManager.storeRound.CreateStoreItem(unlockData);
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
            if(correctPrefix != null)
            {
                correctPrefix.difficulty = prefixDifficulty;
            }
        }
        if (currentCase.correctWordIdentifierMap.ContainsKey(2))
        {
            CaseWordData correctNoun = GameDataManager.Instance.GetWord(currentCase.correctWordIdentifierMap[2]);
            if(correctNoun != null)
            {
                correctNoun.difficulty = nounDifficulty;
            }
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
        if(statRoleKeys.Count != statRoleValues.Count)
        {
            Debug.LogError("ERROR[RpcPlayerStatRoles]: Keys and Values have different count.");
            return;
        }
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
        if(!GameManager.Instance.playerFlowManager.drawingRound.cabinetDrawerMap.ContainsKey(cabinetIndex))
        {
            Debug.LogError("ERROR[RpcSetCabinetOwner]: Could not find matching cabinet["+cabinetIndex.ToString()+"]");
            return;
        }
        GameManager.Instance.playerFlowManager.drawingRound.cabinetDrawerMap[cabinetIndex].setCabinetOwner(player);
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
            EndgameCaseData endgameCase = new EndgameCaseData(netDataCase);
            if(GameManager.Instance.playerFlowManager.slidesRound.caseDataMap.ContainsKey(netDataCase.identifier))
            {
                continue;
            }
            GameManager.Instance.playerFlowManager.slidesRound.caseDataMap.Add(netDataCase.identifier, endgameCase);
            
            bool caseContainsPlayer = endgameCase.ContainsBird(SettingsManager.Instance.birdName);
            if (caseContainsPlayer)
            {
                int birdBucksEarned = endgameCase.scoringData.GetTotalPoints() / endgameCase.taskDataMap.Count;
                GameManager.Instance.playerFlowManager.storeRound.IncreaseCurrentMoney(birdBucksEarned);
            }
        }
        CmdTransitionCondition("endgame_data_loaded:" + SettingsManager.Instance.birdName.ToString());
    }

    [ClientRpc]
    public void RpcPurchaseStoreItem(BirdName purchaser, int itemIndex)
    {
        GameManager.Instance.playerFlowManager.storeRound.PurchaseStoreItem(purchaser, itemIndex);
    }

    [ClientRpc]
    public void RpcRemoveStoreWaitingForPlayerVisual(BirdName player)
    {
        GameManager.Instance.playerFlowManager.storeRound.RemoveWaitingForPlayerVisual(player);
    }

    [ClientRpc]
    public void RpcRemoveReviewWaitingForPlayerVisual(BirdName player)
    {
        GameManager.Instance.playerFlowManager.reviewRound.RemoveWaitingForPlayerVisual(player);
    }

    [ClientRpc]
    public void RpcUpgradeCaseChoice(string itemName)
    {
        CaseUpgradeStoreItemData upgradeData = GameDataManager.Instance.GetMatchingCaseUpgradeStoreItem(itemName);
        if(upgradeData != null)
        {
            GameDataManager.Instance.UpgradeCaseChoice(upgradeData);
        }
        
    }

    [ClientRpc]
    public void RpcSetAccusation(BirdName accusingPlayer, BirdName accusedPlayer)
    {
        GameManager.Instance.playerFlowManager.reviewRound.SetAccusation(accusingPlayer, accusedPlayer);
    }

    [ClientRpc]
    public void RpcShowAccuseChoice(int caseID, int taskID)
    {
        GameManager.Instance.playerFlowManager.accusationRound.ShowEvidence(caseID, taskID);
    }

    [ClientRpc]
    public void RpcUpdateAccusationState(AccuseStateTimingData timingData)
    {
        GameManager.Instance.playerFlowManager.accusationRound.UpdateState(timingData);
    }

    [ClientRpc]
    public void RpcAccuseVote(BirdName voter, BirdName target)
    {
        GameManager.Instance.playerFlowManager.accusationRound.SetVoteArmTarget(voter, target);
    }

    public void RpcInitializeAccuseRevealWrapper(BirdName accuser, BirdName accused, Dictionary<BirdName, List<BirdName>> voteMap)
    {
        List<BirdName> voteKeys = new List<BirdName>();
        List<List<BirdName>> voteValues = new List<List<BirdName>>();

        foreach(KeyValuePair<BirdName,List<BirdName>> voteGroup in voteMap)
        {
            voteKeys.Add(voteGroup.Key);
            voteValues.Add(voteGroup.Value);
        }

        RpcInitializeAccuseRevealWrapper(accuser, accused, voteKeys, voteValues);
    }

    [ClientRpc]
    public void RpcInitializeAccuseRevealWrapper(BirdName accuser, BirdName accused, List<BirdName> voteKeys, List<List<BirdName>> voteValues)
    {
        Dictionary<BirdName, List<BirdName>> voteMap = new Dictionary<BirdName, List<BirdName>>();
        for(int i = 0; i < voteKeys.Count; i++)
        {
            voteMap.Add(voteKeys[i], voteValues[i]);
        }

        GameManager.Instance.playerFlowManager.accusationRound.StartReveal(accuser, accused, voteMap);
    }

    public void RpcInitializeAccuseRevealWrapper(BirdName accuser, BirdName accused, Dictionary<BirdName, List<BirdName>> voteMap, PlayerRevealNetData revealData)
    {
        List<BirdName> voteKeys = new List<BirdName>();
        List<List<BirdName>> voteValues = new List<List<BirdName>>();

        foreach (KeyValuePair<BirdName, List<BirdName>> voteGroup in voteMap)
        {
            voteKeys.Add(voteGroup.Key);
            voteValues.Add(voteGroup.Value);
        }

        RpcInitializeAccuseRevealWrapper(accuser, accused, voteKeys, voteValues, revealData);
    }

    [ClientRpc]
    public void RpcInitializeAccuseRevealWrapper(BirdName accuser, BirdName accused, List<BirdName> voteKeys, List<List<BirdName>> voteValues, PlayerRevealNetData revealData)
    {
        Dictionary<BirdName, List<BirdName>> voteMap = new Dictionary<BirdName, List<BirdName>>();
        for (int i = 0; i < voteKeys.Count; i++)
        {
            voteMap.Add(voteKeys[i], voteValues[i]);
        }

        GameManager.Instance.playerFlowManager.accusationRound.StartReveal(accuser, accused, voteMap, revealData);
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
        if(possibleWordKeys.Count != possibleWordValues.Count)
        {
            Debug.LogError("ERROR[TargetPossibleWords]: Different count for keys and values.");
            return;
        }
        Dictionary<int, List<string>> possibleWords = new Dictionary<int, List<string>>();
        for (int i = 0; i < possibleWordKeys.Count; i++)
        {
            if (possibleWords.ContainsKey(possibleWordKeys[i]))
            {
                continue;
            }
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
    public void TargetStartChoiceDrawing(NetworkConnectionToClient target, int cabinetIndex, int caseID, string prompt, TaskData taskData, float currentModifier, float maxScoreModifier, float modifierDecrement, TaskData.TaskModifier drawingBoxModifier)
    {
        GameManager.Instance.playerFlowManager.drawingRound.StartChoiceCaseDrawing(cabinetIndex, caseID, prompt, taskData.duration, currentModifier, maxScoreModifier, modifierDecrement, drawingBoxModifier);
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
            if (!drawingRound.caseMap[drawingData.caseID].drawings.ContainsKey(drawingData.round))
            {
                Debug.LogError("Missing drawing["+drawingData.round.ToString()+"] for the case["+drawingData.caseID.ToString()+"].");
                return;
            }
            drawingRound.caseMap[drawingData.caseID].drawings[drawingData.round].visuals.AddRange(drawingData.visuals);
        }
        
    }

    [TargetRpc]
    public void TargetSendPointsToScoreTrackerPlayer(NetworkConnectionToClient target, int points)
    {
        GameManager.Instance.playerFlowManager.drawingRound.UpdateScoreTrackerPoints(points);
    }

    [TargetRpc]
    public void TargetInitializePlayer(NetworkConnectionToClient target, string playerName, BirdName bird, RoleData.RoleType roleType)
    {
        GameManager.Instance.playerFlowManager.instructionRound.InitializePlayer(bird, playerName, roleType);
    }

    //Command - transition_condition
    [Command(requiresAuthority = false)]
    public void CmdTransitionCondition(string transitionCondition)
    {
        GameManager.Instance.gameFlowManager.resolveTransitionCondition(transitionCondition);
    }

    [Command(requiresAuthority =false)]
    public void CmdFinishWithStore(BirdName player)
    {
        GameManager.Instance.playerFlowManager.storeRound.FinishWithStoreForPlayer(player);
    }

    [Command(requiresAuthority = false)]
    public void CmdFinishWithReview(BirdName player)
    {
        GameManager.Instance.playerFlowManager.reviewRound.FinishWithReviewForPlayer(player);
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

    [Command(requiresAuthority = false)]
    public void CmdLongArmPosition(BirdName birdName, Vector3 currentPosition, PlayerStretchArm.Variant variant)
    {
        GameManager.Instance.gameFlowManager.SetLongArmPosition(birdName, currentPosition, variant);
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
        if(validCaseChoices.Count == 0)
        {
            Debug.LogError("ERROR[CmdRequestCaseChoice]: Could not find any valid case choices.");
            return;
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
                    CaseChoiceNetData netData = GameManager.Instance.gameFlowManager.wordManager.PopulateChoiceWords(caseChoice);
                    if(netData == null)
                    {
                        Debug.LogError("ERROR[CmdRequestCaseChoice]: Could not generate net data for case choice.");
                        return;
                    }
                    caseChoiceDatas.Add(netData);
                    break;
            }
        }
        TargetSendCaseChoices(SettingsManager.Instance.GetConnection(birdName), caseChoiceDatas);
        SettingsManager.Instance.gameMode.casesRemaining--;
        RpcUpdateNumberOfCases(SettingsManager.Instance.gameMode.casesRemaining);
        

    }

    [Command(requiresAuthority = false)]
    public void CmdRerollCaseChoice(BirdName birdName)
    {
        List<CaseChoiceData> allCaseChoices = GameDataManager.Instance.GetCaseChoices(SettingsManager.Instance.gameMode.caseChoiceIdentifiers);
        int numberOfPlayers = GameManager.Instance.gameFlowManager.GetNumberOfConnectedPlayers();
        List<CaseChoiceData> validCaseChoices = new List<CaseChoiceData>();
        //Iterate over all case choices and add to the valid case choice list based on what is possible to complete and the frequency set in the choice
        foreach (CaseChoiceData caseChoice in allCaseChoices)
        {
            if (caseChoice.numberOfTasks <= numberOfPlayers)
            {
                for (int i = 0; i < caseChoice.selectionFrequency; i++)
                {
                    validCaseChoices.Add(caseChoice);
                }
            }
        }
        if (validCaseChoices.Count == 0)
        {
            Debug.LogError("ERROR[CmdRequestCaseChoice]: Could not find any valid case choices.");
            return;
        }
        List<CaseChoiceData> caseChoices = new List<CaseChoiceData>();
        caseChoices.Add(validCaseChoices.OrderBy(x => Guid.NewGuid()).ToList()[0]);
        caseChoices.Add(validCaseChoices.OrderBy(x => Guid.NewGuid()).ToList()[0]);
        caseChoices.Add(validCaseChoices.OrderBy(x => Guid.NewGuid()).ToList()[0]);

        List<CaseChoiceNetData> caseChoiceDatas = new List<CaseChoiceNetData>();
        foreach (CaseChoiceData caseChoice in caseChoices)
        {
            //Set the word options for the choices
            switch (SettingsManager.Instance.gameMode.wordDistributionMode)
            {
                case GameModeData.WordDistributionMode.random:
                    CaseChoiceNetData netData = GameManager.Instance.gameFlowManager.wordManager.PopulateChoiceWords(caseChoice);
                    if(netData == null)
                    {
                        Debug.LogError("ERROR[CmdRerollCaseChoice]: Could not generate net data for case choice.");
                        return;
                    }
                    caseChoiceDatas.Add(netData);
                    break;
            }
        }

        TargetSendCaseChoices(SettingsManager.Instance.GetConnection(birdName), caseChoiceDatas);
    }

    [Command(requiresAuthority =false)]
    public void CmdRegisterPlayerForScoreTracker(BirdName player)
    {
        if (!GameManager.Instance.gameFlowManager.scoreTrackerPlayers.Contains(player))
        {
            GameManager.Instance.gameFlowManager.scoreTrackerPlayers.Add(player);
        }
            
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
        
        if(!GameManager.Instance.playerFlowManager.drawingRound.caseMap.ContainsKey(caseID))
        {
            Debug.LogError("ERROR[CmdChooseCase]: Case map did not contain case["+caseID.ToString()+"]");
            return;
        }

        //Send the base task
        ChainData newCase = GameManager.Instance.playerFlowManager.drawingRound.caseMap[caseID];

        if(newCase.taskQueue.Count == 0)
        {
            Debug.LogError("ERROR[CmdChooseCase]: Task queue is empty for case["+caseID.ToString()+"]");
            return;
        }
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
        if(!GameManager.Instance.gameFlowManager.playerCabinetMap.ContainsKey(birdName))
        {
            Debug.LogError("ERROR[CmdChooseCase]: Could not find matching cabinet for player["+birdName.ToString()+"]");
            return;
        }
        int birdCabinetIndex = GameManager.Instance.gameFlowManager.playerCabinetMap[birdName];
        TargetStartChoiceDrawing(SettingsManager.Instance.GetConnection(birdName), birdCabinetIndex, caseID, choiceData.correctPrompt, baseTaskData, newCase.currentScoreModifier, choiceData.maxScoreModifier, choiceData.scoreModifierDecrement, drawingBoxModifier);
        
    }

    [Command(requiresAuthority =false)]
    public void CmdRequestNextCase(BirdName birdName)
    {
        GameManager.Instance.playerFlowManager.drawingRound.SendNextInQueue(birdName);
    }

    [Command(requiresAuthority =false)]
    public void CmdSendPointsToScoreTrackerPlayers(int caseID)
    {
        if(!GameManager.Instance.playerFlowManager.drawingRound.caseMap.ContainsKey(caseID))
        {
            Debug.LogError("ERROR[CmdSendPointsToScoreTrackerPlayers]: caseMap did not contain case["+caseID.ToString()+"]");
            return;
        }
        ChainData currentCase = GameManager.Instance.playerFlowManager.drawingRound.caseMap[caseID];
        int casePoints = currentCase.GetTotalPoints();
        foreach(BirdName player in GameManager.Instance.gameFlowManager.scoreTrackerPlayers)
        {
            TargetSendPointsToScoreTrackerPlayer(SettingsManager.Instance.GetConnection(player), casePoints);
        }
        
    }

    [Command(requiresAuthority =false)]
    public void CmdTransitionCase(int caseID)
    {
        GameManager.Instance.gameFlowManager.SendTaskToNextPlayer(caseID);
    }

    [Command(requiresAuthority =false)]
    public void CmdUpdateCaseScoreModifier(int caseID, float inScoreModifier)
    {
        if(!GameManager.Instance.playerFlowManager.drawingRound.caseMap.ContainsKey(caseID))
        {
            Debug.LogError("ERROR[CmdUpdateCaseScoreModifier]: CaseMap does not contain case[" + caseID.ToString() + "]");
            return;
        }
        ChainData transitioningCase = GameManager.Instance.playerFlowManager.drawingRound.caseMap[caseID];
        if(transitioningCase.taskQueue.Count <= (transitioningCase.currentRound -1))
                {
            Debug.LogError("ERROR[CmdUpdateCaseScoreModifier]: Task queue["+transitioningCase.taskQueue.Count.ToString()+"] is not big enough for previous round["+(transitioningCase.currentRound-1).ToString()+"]");
            return;
        }
        TaskData transitioningTask = transitioningCase.taskQueue[transitioningCase.currentRound - 1];
        float changeInScoreModifier = inScoreModifier - transitioningCase.currentScoreModifier;
        transitioningTask.timeModifierDecrement += changeInScoreModifier;
        transitioningCase.currentScoreModifier = inScoreModifier;
    }

    [Command(requiresAuthority =false)]
    public void CmdTryToPurchaseStoreItem(BirdName player, int storeItemIndex)
    {
        GameManager.Instance.playerFlowManager.storeRound.HandleClientRequestItem(player, storeItemIndex);
    }

    [Command(requiresAuthority=false)]
    public void CmdAccusePlayer(BirdName accusingPlayer, BirdName accusedPlayer)
    {
        //Add transition conditions
        foreach(BirdName player in SettingsManager.Instance.GetAllActiveBirds())
        {
            GameManager.Instance.gameFlowManager.addTransitionCondition("accuse_set:" + player.ToString());
        }
        RpcSetAccusation(accusingPlayer, accusedPlayer);
        
        GameManager.Instance.gameFlowManager.timeRemainingInPhase = 0f;
    }

    [Command(requiresAuthority =false)]
    public void CmdShowAccuseChoice(int caseID, int taskID)
    {
        RpcShowAccuseChoice(caseID, taskID);
    }

    [Command(requiresAuthority =false)]
    public void CmdAccuseVote(BirdName voter, BirdName target)
    {
        RpcAccuseVote(voter, target);

        GameManager.Instance.playerFlowManager.accusationRound.AddVote(voter, target);
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
