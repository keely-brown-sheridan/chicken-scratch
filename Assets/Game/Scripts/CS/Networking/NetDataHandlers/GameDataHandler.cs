using ChickenScratch;
using Mirror;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ChickenScratch.ColourManager;
using static ChickenScratch.DrawingData;
using static ChickenScratch.GameFlowManager;
using static ChickenScratch.GameModeData;
using static ChickenScratch.ReactionIndex;
using static ChickenScratch.TaskData;

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
    public void RpcUpdateFolderAsReady(int caseID, int cabinetIndex, BirdName player)
    {
        DrawingRound drawingRound = GameManager.Instance.playerFlowManager.drawingRound;
        ChainData currentCase;
        if (!drawingRound.caseMap.ContainsKey(caseID))
        {
            currentCase = new ChainData();
            currentCase.identifier = caseID;
            drawingRound.caseMap.Add(caseID, currentCase);
        }

        CabinetDrawer currentDrawer = drawingRound.GetCabinet(cabinetIndex);
        if(currentDrawer == null)
        {
            Debug.LogError("Could not set drawer as ready because the provided cabinet index["+ cabinetIndex.ToString()+"] did not exist for the drawing round.");
            return;
        }
        currentDrawer.setAsReady(player);
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
        List<PlayerRatingNetData> ratingValues = new List<PlayerRatingNetData>();
        foreach(KeyValuePair<int,EndgameCaseData> caseData in caseDataMap)
        {
            foreach(KeyValuePair<int,EndgameTaskData> taskData in caseData.Value.taskDataMap)
            {
                ratingValues.Add(new PlayerRatingNetData(taskData.Value.ratingData, caseData.Key, taskData.Key));
            }
           
        }
        RpcSlideRoundEndInfo(ratingValues);
    }

    //Broadcast - slide_round_end_info
    [ClientRpc]
    public void RpcSlideRoundEndInfo(List<PlayerRatingNetData> playerRatings)
    {
        GameManager.Instance.playerFlowManager.slidesRound.UpdateCaseRatings(playerRatings);
        GameManager.Instance.gameDataHandler.CmdTransitionCondition("ratings_loaded:" + SettingsManager.Instance.birdName.ToString());
    }

    [ClientRpc]
    public void RpcCreateSlidesFromCase(int caseID)
    {
        GameManager.Instance.playerFlowManager.slidesRound.CreateSlidesFromCase(caseID);
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

    public void RpcSendHatStoreItemWrapper(HatStoreItemData hatItemData)
    {
        HatStoreItemNetData hatNetData = new HatStoreItemNetData(hatItemData);
        RpcSendHatStoreItem(hatNetData);
    }

    [ClientRpc]
    public void RpcSendHatStoreItem(HatStoreItemNetData netData)
    {
        HatStoreItemData hatData = new HatStoreItemData(netData);
        GameManager.Instance.playerFlowManager.storeRound.CreateStoreItem(hatData);
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

    public void RpcSendCertificationStoreItemWrapper(CaseCertificationStoreItemData certificationItemData)
    {
        CaseCertificationStoreItemNetData certificationNetData = new CaseCertificationStoreItemNetData(certificationItemData);
        RpcSendCertificationStoreItem(certificationNetData);
    }

    [ClientRpc]
    public void RpcSendCertificationStoreItem(CaseCertificationStoreItemNetData netData)
    {
        CaseCertificationStoreItemData certificationData = new CaseCertificationStoreItemData(netData);
        GameManager.Instance.playerFlowManager.storeRound.CreateStoreItem(certificationData);
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
        GameManager.Instance.pauseModTools.DisconnectPlayer(player);
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
    public void RpcDeactivateCasePile()
    {
        GameManager.Instance.playerFlowManager.drawingRound.newCaseCabinet.Deactivate();
    }
    [ClientRpc]
    public void RpcUpdateNumberOfCases(int numberOfCases)
    {
        GameManager.Instance.playerFlowManager.drawingRound.UpdateNumberOfCases(numberOfCases);
    }

    [ClientRpc]
    public void RpcSendEndgameCaseCount(int numberOfCases)
    {
        GameManager.Instance.playerFlowManager.slidesRound.expectedCaseCount = numberOfCases;
        CmdTransitionCondition("endgame_cases_expected:" + SettingsManager.Instance.birdName.ToString());
        CmdRequestEndgameCases(SettingsManager.Instance.birdName);
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
        //Host handles this when it gets sent to prevent issues
        if(SettingsManager.Instance.isHost)
        {
            return;
        }
        CaseUpgradeStoreItemData upgradeData = GameDataManager.Instance.GetMatchingCaseUpgradeStoreItem(itemName);
        if(upgradeData != null)
        {
            foreach (StoreItemData upgrade in upgradeData.unlocks)
            {
                if(upgrade.itemType == StoreItem.StoreItemType.case_upgrade)
                {
                    GameDataManager.Instance.AddUpgradeOption((CaseUpgradeStoreItemData)upgrade);
                }
            }
            GameDataManager.Instance.UpgradeCaseChoice(upgradeData);
        }
    }

    [ClientRpc]
    public void RpcUpdateFrequencyStoreOption(string identifier, int selectionFrequency, int currentFrequencyRampIndex)
    {
        GameManager.Instance.playerFlowManager.storeRound.UpdateCaseFrequency(identifier, selectionFrequency, currentFrequencyRampIndex);
    }

    [ClientRpc]
    public void RpcRemoveFrequencyStoreOption(string identifier)
    {
        GameManager.Instance.playerFlowManager.storeRound.RemoveCaseFromFrequencyPool(identifier);
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

    [ClientRpc]
    public void RpcUpdateSlidesCaseCount(int startingCaseIndex, int totalCases)
    {
        GameManager.Instance.playerFlowManager.slidesRound.startingCaseIndex = startingCaseIndex;
        GameManager.Instance.playerFlowManager.slidesRound.totalCasesInSlidesRound = totalCases;
    }

    [ClientRpc]
    public void RpcInitializeStoreUnlock(StoreChoiceOptionData storeOptionChoiceA, StoreChoiceOptionData storeOptionChoiceB, BirdName unionRep, bool defaultChoiceA)
    {
        GameManager.Instance.playerFlowManager.storeRound.ClientInitializeUnlocks(storeOptionChoiceA, storeOptionChoiceB, unionRep, defaultChoiceA);
    }

    [ClientRpc]
    public void RpcInitializeStore()
    {
        GameManager.Instance.playerFlowManager.storeRound.ClientInitializeStore();
    }

    [ClientRpc]
    public void RpcSetPlayerHat(BirdName player, BirdHatData.HatType birdHat)
    {
        GameManager.Instance.playerFlowManager.SetBirdHatType(player, birdHat);
    }

    [ClientRpc]
    public void RpcUnlockMiddleStoreRow()
    {
        GameManager.Instance.playerFlowManager.storeRound.UnlockMiddleRow();
    }

    [ClientRpc]
    public void RpcUnlockBottomStoreRow()
    {
        GameManager.Instance.playerFlowManager.storeRound.UnlockBottomRow();
    }

    [ClientRpc]
    public void RpcSetStoreChoice(StoreChoiceOptionData choiceOption)
    {
        GameManager.Instance.playerFlowManager.timeInDay += choiceOption.timeRamp;
        GameManager.Instance.playerFlowManager.currentGoal = (int)(choiceOption.birdbucksPerPlayer * SettingsManager.Instance.GetPlayerNameCount());
        GameManager.Instance.playerFlowManager.storeRound.SetStoreChoiceOption(choiceOption);
    }

    [ClientRpc]
    public void RpcStoreIncreaseModifierForCase(string caseChoiceIdentifier)
    {
        GameManager.Instance.playerFlowManager.storeRound.IncreaseModifierForCase(caseChoiceIdentifier);
    }

    [ClientRpc]
    public void RpcStoreIncreaseBirdbucksForCase(string caseChoiceIdentifier)
    {
        GameManager.Instance.playerFlowManager.storeRound.IncreaseBirdbucksForCase(caseChoiceIdentifier);
    }

    [ClientRpc]
    public void RpcStoreIncreaseFrequencyForCase(string caseChoiceIdentifier)
    {
        GameManager.Instance.playerFlowManager.storeRound.IncreaseFrequencyForCase(caseChoiceIdentifier);
    }

    [ClientRpc]
    public void RpcAddCaseCertification(string identifier, string certificationIdentifier)
    {
        if(SettingsManager.Instance.isHost)
        {
            //We handle this on the server-side
            return;
        }
        GameManager.Instance.playerFlowManager.AddCaseCertification(identifier, certificationIdentifier);
    }

    [ClientRpc]
    public void RpcShowStoreCaseExpiry(string identifier, int index)
    {
        GameManager.Instance.playerFlowManager.storeRound.ShowExpiryEffectIndicator(identifier, index);
    }

    [ClientRpc]
    public void RpcProgressTutorial(BirdName player, int slideIndex, string tutorialIdentifier)
    {
        switch(tutorialIdentifier)
        {
            case "deadline":
                GameManager.Instance.gameFlowManager.deadlineTutorialSequence.ProgressPlayerIndicator(player, slideIndex);
                break;
            case "slides":
                GameManager.Instance.gameFlowManager.slideTutorialSequence.ProgressPlayerIndicator(player, slideIndex);
                break;
            case "store":
                GameManager.Instance.gameFlowManager.storeTutorialSequence.ProgressPlayerIndicator(player, slideIndex);
                break;
        }
    }

    [ClientRpc]
    public void RpcPropagateDailyValues(int tomorrowOnlyCasesIncrease, int tomorrowOnlyQuotaDecrease, float tomorrowOnlyTimeIncrease,
                                                                        int baseCasesIncrease, int baseQuotaDecrement, float baseTimeIncrease,
                                                                        float caseIncreaseRatio, float quotaDecreaseRatio, float timeIncreaseRatio,
                                                                        int currentGoal)
    {
        GameManager.Instance.playerFlowManager.tomorrowOnlyCasesIncrease = tomorrowOnlyCasesIncrease;
        GameManager.Instance.playerFlowManager.tomorrowOnlyQuotaDecrease = tomorrowOnlyQuotaDecrease;
        GameManager.Instance.playerFlowManager.tomorrowOnlyTimeIncrease = tomorrowOnlyTimeIncrease;
        GameManager.Instance.playerFlowManager.baseCasesIncrease = baseCasesIncrease;
        GameManager.Instance.playerFlowManager.baseQuotaDecrement = baseQuotaDecrement;
        GameManager.Instance.playerFlowManager.baseTimeIncrease = baseTimeIncrease;
        GameManager.Instance.playerFlowManager.caseIncreaseRatio = caseIncreaseRatio;
        GameManager.Instance.playerFlowManager.quotaDecreaseRatio = quotaDecreaseRatio;
        GameManager.Instance.playerFlowManager.timeIncreaseRatio = timeIncreaseRatio;
        GameManager.Instance.playerFlowManager.currentGoal = currentGoal;
    }

    [ClientRpc]
    public void RpcSetCompetitionChoice(int caseID, BirdName player)
    {
        GameManager.Instance.playerFlowManager.slidesRound.SetCompetitionChoice(caseID, player);
    }

    public void TargetInitialCabinetPromptContentsWrapper(NetworkConnectionToClient target, int caseID, int round, string correctPrompt, Dictionary<int,string> correctWordIdentifiersMap)
    {
        List<int> correctWordKeys = new List<int>();
        List<string> correctWordValues = new List<string>();
        foreach(KeyValuePair<int,string> correctWord in correctWordIdentifiersMap)
        {
            correctWordKeys.Add(correctWord.Key);
            correctWordValues.Add(correctWord.Value);
        }
        TargetInitialCabinetPromptContents(target, caseID, round, correctPrompt, correctWordKeys, correctWordValues);
    }

    //Target - initial_cabinet_prompt_contents
    [TargetRpc]
    public void TargetInitialCabinetPromptContents(NetworkConnectionToClient target, int caseID, int round, string correctPrompt, List<int> correctWordKeys, List<string> correctWordValues)
    {
        Dictionary<int, string> correctWordIdentifiersMap = new Dictionary<int, string>();
        for(int i = 0; i < correctWordKeys.Count; i++)
        {
            correctWordIdentifiersMap.Add(correctWordKeys[i], correctWordValues[i]);
        }
        GameManager.Instance.playerFlowManager.drawingRound.SetInitialPromptForTask(caseID, round, correctPrompt, correctWordIdentifiersMap);
    }

    [TargetRpc]
    public void TargetSendDrawingForTask(NetworkConnectionToClient target, int round, DrawingData drawingData)
    {
        if(!GameManager.Instance.playerFlowManager.drawingRound.caseMap.ContainsKey(drawingData.caseID))
        {
            Debug.LogError("Could not set drawing for task because case["+drawingData.caseID.ToString()+"] does not exist in the case map.");
            return;
        }
        ChainData currentCase = GameManager.Instance.playerFlowManager.drawingRound.caseMap[drawingData.caseID];
        if(!currentCase.drawings.ContainsKey(drawingData.round))
        {
            currentCase.drawings.Add(drawingData.round, drawingData);
        }

        
        if(currentCase.waitingOnTasks.Contains(drawingData.round))
        {
            currentCase.waitingOnTasks.Remove(drawingData.round);
            if(currentCase.waitingOnTasks.Count == 0)
            {
                CmdTaskIsReady(SettingsManager.Instance.birdName, drawingData.caseID, round);
            }
        }
    }

    [TargetRpc]
    public void TargetSendPromptForTask(NetworkConnectionToClient target, int caseID, int round, PlayerTextInputData promptData, int promptRound)
    {
        if (!GameManager.Instance.playerFlowManager.drawingRound.caseMap.ContainsKey(caseID))
        {
            Debug.LogError("Could not set prompt for task because case[" + caseID.ToString() + "] does not exist in the case map.");
            return;
        }
        ChainData currentCase = GameManager.Instance.playerFlowManager.drawingRound.caseMap[caseID];
        if (!currentCase.prompts.ContainsKey(promptRound))
        {
            currentCase.prompts.Add(promptRound, promptData);
        }

        if (currentCase.waitingOnTasks.Contains(promptRound))
        {
            currentCase.waitingOnTasks.Remove(promptRound);
            if (currentCase.waitingOnTasks.Count == 0)
            {
                CmdTaskIsReady(SettingsManager.Instance.birdName, caseID, round);
            }
        }
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
    public void TargetStartChoiceDrawing(NetworkConnectionToClient target, int cabinetIndex, int caseID, string prompt, TaskData taskData, float currentModifier, float maxScoreModifier, float modifierDecrement, List<TaskData.TaskModifier> taskModifiers, string caseTypeName, List<BirdName> playerOrder)
    {
        GameManager.Instance.playerFlowManager.drawingRound.StartChoiceCaseDrawing(cabinetIndex, caseID, prompt, taskData.duration, currentModifier, maxScoreModifier, modifierDecrement, taskModifiers, caseTypeName, playerOrder);
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

    [TargetRpc]
    public void TargetKickPlayer(NetworkConnectionToClient target)
    {
        NetworkManager.singleton.StopClient();
    }

    
    public void TargetSendEndgameCaseDataWrapper(NetworkConnectionToClient target, EndgameCaseData caseData)
    {
        TargetSendEndgameCaseData(target, new EndgameCaseNetData(caseData));
    }

    [TargetRpc]
    public void TargetSendEndgameCaseData(NetworkConnectionToClient target, EndgameCaseNetData netDataCase)
    {
        EndgameCaseData endgameCase = new EndgameCaseData(netDataCase);
        if (GameManager.Instance.playerFlowManager.slidesRound.caseDataMap.ContainsKey(netDataCase.identifier))
        {
            return;
        }
        GameManager.Instance.playerFlowManager.slidesRound.caseDataMap.Add(netDataCase.identifier, endgameCase);

        bool caseContainsPlayer = endgameCase.ContainsBird(SettingsManager.Instance.birdName);
        if (caseContainsPlayer)
        {
            int birdBucksEarned = endgameCase.GetPointsForPlayerOnTask(SettingsManager.Instance.birdName);
            bool caseHasShareholdersCertification = GameManager.Instance.playerFlowManager.CaseHasCertification(endgameCase.caseTypeName, "Shareholders");
            if (caseHasShareholdersCertification)
            {
                FloatCertificationData shareholderCertification = (FloatCertificationData)GameDataManager.Instance.GetCertification("Shareholders");
                if (shareholderCertification != null)
                {
                    birdBucksEarned = (int)(birdBucksEarned * (1-shareholderCertification.value));
                }
            }
            GameManager.Instance.playerFlowManager.storeRound.IncreaseCurrentMoney(birdBucksEarned);
        }
        CmdCaseDataReceived(SettingsManager.Instance.birdName, netDataCase.identifier);
    }

    [TargetRpc]
    public void TargetSendEndgameDrawing(NetworkConnectionToClient target, DrawingData drawingData)
    {
        if(!GameManager.Instance.playerFlowManager.slidesRound.caseDataMap.ContainsKey(drawingData.caseID))
        {
            //Log an error
            Debug.LogError("Could not receive endgame drawing because caseID["+drawingData.caseID.ToString()+"] is missing from caseDataMap.");
            return;
        }
        EndgameCaseData currentCaseData = GameManager.Instance.playerFlowManager.slidesRound.caseDataMap[drawingData.caseID];
        if (!currentCaseData.taskDataMap.ContainsKey(drawingData.round))
        {
            //Log an error
            Debug.LogError("Could not receive endgame drawing because caseID["+drawingData.caseID.ToString()+"] is missing task["+(drawingData.round).ToString()+"].");
            return;
        }
        EndgameTaskData currentTaskData = currentCaseData.taskDataMap[drawingData.round];
        currentTaskData.drawingData = drawingData;
        currentTaskData.expectingDrawing = false;

        if (GameManager.Instance.playerFlowManager.slidesRound.caseDataMap.Count != GameManager.Instance.playerFlowManager.slidesRound.expectedCaseCount)
        {
            return;
        }

        //Check to see if everything has been received
        foreach (EndgameCaseData caseData in GameManager.Instance.playerFlowManager.slidesRound.caseDataMap.Values)
        {
            foreach(EndgameTaskData taskData in  caseData.taskDataMap.Values)
            {
                if(taskData.expectingDrawing)
                {
                    return;
                }
            }
        }

        //Everything has been received, let the server know
        CmdTransitionCondition("endgame_data_loaded:" + SettingsManager.Instance.birdName.ToString());
    }

    [TargetRpc]
    public void TargetCheckForAllEndgameDataReceived(NetworkConnectionToClient target)
    {
        if(GameManager.Instance.playerFlowManager.slidesRound.caseDataMap.Count != GameManager.Instance.playerFlowManager.slidesRound.expectedCaseCount)
        {
            return;
        }
        //Check to see if everything has been received
        foreach (EndgameCaseData caseData in GameManager.Instance.playerFlowManager.slidesRound.caseDataMap.Values)
        {
            foreach (EndgameTaskData taskData in caseData.taskDataMap.Values)
            {
                if (taskData.expectingDrawing)
                {
                    return;
                }
            }
        }

        //Everything has been received, let the server know
        CmdTransitionCondition("endgame_data_loaded:" + SettingsManager.Instance.birdName.ToString());
    }

    [TargetRpc]
    public void TargetPrepareForTask(NetworkConnectionToClient target, FolderUpdateData folderData)
    {
        DrawingRound drawingRound = GameManager.Instance.playerFlowManager.drawingRound;
        ChainData currentCase;
        if (!drawingRound.caseMap.ContainsKey(folderData.caseID))
        {
            currentCase = new ChainData();
            currentCase.identifier = folderData.caseID;
            drawingRound.caseMap.Add(folderData.caseID, currentCase);
        }


        drawingRound.UpdateQueuedFolder(folderData.caseID, folderData.roundNumber, folderData.currentState, folderData.wordCategory, folderData.playerOrder);
        currentCase = drawingRound.caseMap[folderData.caseID];
        currentCase.PopulateFromFolderUpdateData(folderData);
        currentCase.caseTypeName = folderData.caseTypeName;
        currentCase.requiredTasks = folderData.requiredTasks;

        List<int> missingTasks = new List<int>();
        foreach (int taskRound in folderData.requiredTasks)
        {
            if(taskRound == 0)
            {
                if(currentCase.correctWordIdentifierMap.Count == 0)
                {
                    missingTasks.Add(taskRound);
                }
                continue;
            }
            
            if(currentCase.taskQueue.Count < taskRound)
            {
                missingTasks.Add(taskRound);
            }
            else
            {
                TaskData previousTask = currentCase.taskQueue[taskRound - 1];
                switch (previousTask.taskType)
                {
                    case TaskType.base_drawing:
                    case TaskType.compile_drawing:
                    case TaskType.copy_drawing:
                    case TaskType.add_drawing:
                    case TaskType.prompt_drawing:
                    case TaskType.blender_drawing:
                        if (!currentCase.drawings.ContainsKey(taskRound))
                        {
                            missingTasks.Add(taskRound);
                        }
                        break;
                    case TaskType.prompting:
                        if(!currentCase.prompts.ContainsKey(taskRound))
                        {
                            missingTasks.Add(taskRound);
                        }
                        break;
                    case TaskData.TaskType.morph_guessing:
                    case TaskType.base_guessing:
                    case TaskData.TaskType.competition_guessing:
                        //not sure what to do in this situation?
                        //Currently shouldn't be happening
                        break;
                }
            }
        }
        currentCase.waitingOnTasks = missingTasks;

        if (missingTasks.Count > 0)
        {
            //Request the missing tasks
            GameManager.Instance.gameDataHandler.CmdRequestMissingTasksForTask(SettingsManager.Instance.birdName, folderData.caseID, folderData.roundNumber, missingTasks);
        }
        else
        {
            //Let the server know that the task is ready
            GameManager.Instance.gameDataHandler.CmdTaskIsReady(SettingsManager.Instance.birdName, folderData.caseID, folderData.roundNumber);
        }
    }

    [TargetRpc]
    public void TargetOpenStoreFrequencyPanel(NetworkConnectionToClient target)
    {
        GameManager.Instance.playerFlowManager.storeRound.OpenStoreFrequencyPanel();
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

        if (SettingsManager.Instance.isHost && SettingsManager.Instance.saveAdminReviewData)
        {
            ChainData currentCase = GameManager.Instance.playerFlowManager.drawingRound.caseMap[caseID];
            bool isQueuedForPlayer = currentCase.playerOrder.ContainsKey(tab + 1) ? currentCase.playerOrder[tab + 1] == SettingsManager.Instance.birdName : false;
            SettingsManager.Instance.SavePromptingData(caseID, tab, new PlayerTextInputData() { author = author, text = prompt, isQueuedForPlayer = isQueuedForPlayer });
        }
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
    public void CmdPlayerStats(PlayerStatData statData)
    {
        GameManager.Instance.playerFlowManager.accoladesRound.playerStatsManager.AddPlayersideStats(statData);

    }

    [Command(requiresAuthority = false)]
    public void CmdRequestCaseChoice(BirdName birdName)
    {
        
        List<CaseChoiceData> allCaseChoices = GameDataManager.Instance.GetCaseChoices(GameManager.Instance.playerFlowManager.unlockedCaseChoiceIdentifiers);
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
        GameManager.Instance.playerFlowManager.casesRemaining--;
        RpcUpdateNumberOfCases(GameManager.Instance.playerFlowManager.casesRemaining);
        

    }

    [Command(requiresAuthority = false)]
    public void CmdRerollCaseChoice(BirdName birdName)
    {
        List<CaseChoiceData> allCaseChoices = GameDataManager.Instance.GetCaseChoices(GameManager.Instance.playerFlowManager.unlockedCaseChoiceIdentifiers);
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
        if (SettingsManager.Instance.isHost && SettingsManager.Instance.saveAdminReviewData)
        {
            bool isQueuedForPlayer = currentCase.playerOrder.ContainsKey(drawingData.round + 1) ? currentCase.playerOrder[drawingData.round + 1] == SettingsManager.Instance.birdName : false;
            SettingsManager.Instance.SaveDrawingData(drawingData, isQueuedForPlayer);
        }
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

        //Start the case immediately instead of queueing it
        if(!GameManager.Instance.gameFlowManager.playerCabinetMap.ContainsKey(birdName))
        {
            Debug.LogError("ERROR[CmdChooseCase]: Could not find matching cabinet for player["+birdName.ToString()+"]");
            return;
        }
        int birdCabinetIndex = GameManager.Instance.gameFlowManager.playerCabinetMap[birdName];
        string promptText = "";
        if(baseTaskData.modifiers.Contains(TaskModifier.hidden_prefix))
        {
            CaseWordData promptWord = GameDataManager.Instance.GetWord(newCase.correctWordIdentifierMap[2]);
            if (promptWord != null)
            {
                promptText = promptWord.value;
            }
        }
        else if (baseTaskData.modifiers.Contains(TaskModifier.hidden_noun))
        {
            CaseWordData promptWord = GameDataManager.Instance.GetWord(newCase.correctWordIdentifierMap[1]);
            if (promptWord != null)
            {
                promptText = promptWord.value;
            }
        }
        else
        {
            promptText = choiceData.correctPrompt;
        }
        List<BirdName> playerOrder = new List<BirdName>();
        for(int i = 0; i < newCase.playerOrder.Count; i++)
        {
            playerOrder.Add(newCase.playerOrder[i + 1]);
        }

        if(GameManager.Instance.playerFlowManager.CaseHasCertification(choiceData.caseChoiceIdentifier, "Assembly"))
        {
            CaseChoiceData template = GameDataManager.Instance.GetCaseChoice(choiceData.caseChoiceIdentifier);
            if(template != null)
            {
                template.IncrementFrequency();
            }
        }

        TargetStartChoiceDrawing(SettingsManager.Instance.GetConnection(birdName), birdCabinetIndex, caseID, promptText, baseTaskData, newCase.currentScoreModifier, choiceData.maxScoreModifier, choiceData.scoreModifierDecrement, baseTaskData.modifiers, newCase.caseTypeName, playerOrder);
        
    }

    [Command(requiresAuthority =false)]
    public void CmdSkipCaseChoice()
    {
        GameManager.Instance.gameFlowManager.IncreaseNumberOfCompletedCases();
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
    public void CmdIncreaseCaseBirdbucks(int caseID, int increment)
    {
        if (!GameManager.Instance.playerFlowManager.drawingRound.caseMap.ContainsKey(caseID))
        {
            Debug.LogError("ERROR[CmdIncreaseCaseBirdbucks]: CaseMap does not contain case[" + caseID.ToString() + "]");
            return;
        }
        ChainData transitioningCase = GameManager.Instance.playerFlowManager.drawingRound.caseMap[caseID];
        transitioningCase.pointsPerCorrectWord += increment;
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

    [Command(requiresAuthority =false)]
    public void CmdRequestRestock()
    {
        GameManager.Instance.playerFlowManager.storeRound.CreateRestockItem();
    }

    [Command(requiresAuthority =false)]
    public void CmdChooseContract(StoreChoiceOptionData choiceOption, bool endRound)
    {
        GameManager.Instance.playerFlowManager.storeRound.hasChosen = true;
        foreach (ContractCaseUnlockData unlock in choiceOption.unlocks)
        {
            GameDataManager.Instance.UnlockCaseChoice(unlock.identifier, unlock.certificationIdentifier);
        }
        if (endRound)
        {
            GameManager.Instance.gameFlowManager.timeRemainingInPhase = 0f;
        }
        RpcSetStoreChoice(choiceOption);
    }

    [Command(requiresAuthority =false)]
    public void CmdRequestEndgameCases(BirdName birdName)
    {
        foreach(EndgameCaseData caseData in GameManager.Instance.playerFlowManager.slidesRound.caseDataMap.Values)
        {
            if(caseData.hasBeenShown)
            {
                continue;
            }
            TargetSendEndgameCaseDataWrapper(SettingsManager.Instance.GetConnection(birdName), caseData);
        }
    }

    [Command(requiresAuthority =false)]
    public void CmdCaseDataReceived(BirdName birdName, int caseID)
    {
        //Send drawings to the player
        EndgameCaseData endgameCase = GameManager.Instance.playerFlowManager.slidesRound.caseDataMap[caseID];
        bool hasReceivedAllCaseData = true;
        foreach(KeyValuePair<int,EndgameTaskData> task in endgameCase.taskDataMap)
        {
            if(task.Value.drawingData != null)
            {
                TargetSendEndgameDrawing(SettingsManager.Instance.GetConnection(birdName), task.Value.drawingData);
                hasReceivedAllCaseData = false;
            }
        }
        //Rare situation in which there's no drawing - we would instantly transition to the next round
        if(hasReceivedAllCaseData)
        {
            TargetCheckForAllEndgameDataReceived(SettingsManager.Instance.GetConnection(birdName));
        }
    }

    [Command(requiresAuthority =false)]
    public void CmdUnlockMiddleStoreRow()
    {
        GameManager.Instance.playerFlowManager.storeRound.RestockColumnItem(100);
        //Create a column item
        GameManager.Instance.playerFlowManager.storeRound.CreateColumnStoreItemData(101);
        GameManager.Instance.playerFlowManager.storeRound.ServerUnlockRow();
        RpcUnlockMiddleStoreRow();
    }

    [Command(requiresAuthority =false)]
    public void CmdUnlockBottomStoreRow()
    {
        GameManager.Instance.playerFlowManager.storeRound.RestockColumnItem(100);
        GameManager.Instance.playerFlowManager.storeRound.RestockColumnItem(101);
        //Create a column item
        GameManager.Instance.playerFlowManager.storeRound.CreateColumnStoreItemData(102);
        GameManager.Instance.playerFlowManager.storeRound.ServerUnlockRow();
        RpcUnlockBottomStoreRow();
    }


    [Command(requiresAuthority =false)]
    public void CmdTaskIsReady(BirdName player, int caseID, int roundNumber)
    {
        //Get the player cabinet
        if (!GameManager.Instance.gameFlowManager.playerCabinetMap.ContainsKey(player))
        {
            Debug.LogError("Could not set task as ready, cabinet map did not contain a cabinet for player["+player.ToString()+"]");
            return;
        }
        int cabinetIndex = GameManager.Instance.gameFlowManager.playerCabinetMap[player];
        if(cabinetIndex == -1)
        {
            Debug.LogError("Could not set task as ready, cabinet is not properly set for player["+player.ToString()+"].");
            return;
        }
        GameManager.Instance.gameDataHandler.RpcUpdateFolderAsReady(caseID, cabinetIndex, player);
    }

    [Command(requiresAuthority=false)]
    public void CmdRequestMissingTasksForTask(BirdName player, int caseID, int roundNumber, List<int> missingTasks)
    {
        if(!GameManager.Instance.playerFlowManager.drawingRound.caseMap.ContainsKey(caseID))
        {
            Debug.LogError("Cannot request missing tasks for task["+roundNumber.ToString()+"] in case["+caseID.ToString()+"] because the case is missing from the case map.");
        }
        ChainData currentCase = GameManager.Instance.playerFlowManager.drawingRound.caseMap[caseID];
        NetworkConnectionToClient playerConnection = SettingsManager.Instance.GetConnection(player);
        foreach (int missingTask in missingTasks)
        {
            if(missingTask == 0)
            {
                TargetInitialCabinetPromptContentsWrapper(playerConnection, caseID, roundNumber, currentCase.correctPrompt, currentCase.correctWordIdentifierMap);
            }
            else
            {
                if(currentCase.taskQueue.Count <= missingTask)
                {
                    Debug.LogError("Cannot request missing task["+missingTask+"] because task queue doesn't contain it.");
                    continue;
                }
                else
                {
                    TaskData missingTaskData = currentCase.taskQueue[missingTask-1];
                    switch(missingTaskData.taskType)
                    {
                        case TaskType.base_drawing:
                        case TaskType.copy_drawing:
                        case TaskType.add_drawing:
                        case TaskType.compile_drawing:
                        case TaskType.prompt_drawing:
                        case TaskType.blender_drawing:
                            //Send the drawing to the player
                            TargetSendDrawingForTask(playerConnection, roundNumber, currentCase.drawings[missingTask]);
                            break;
                        case TaskType.prompting:
                            //Send the prompt to the player
                            TargetSendPromptForTask(playerConnection, caseID, roundNumber, currentCase.prompts[missingTask], missingTask);
                            break;
                        case TaskData.TaskType.morph_guessing:
                        case TaskType.base_guessing:
                        case TaskData.TaskType.competition_guessing:
                            //Unclear what should be done in this situation
                            break;
                    }
                }
            }
        }
    }

    [Command(requiresAuthority =false)]
    public void CmdProgressTutorial(BirdName player, int slideIndex, string tutorialIdentifier)
    {
        RpcProgressTutorial(player, slideIndex, tutorialIdentifier);
    }

    [Command(requiresAuthority =false)]
    public void CmdIncreaseCaseFrequency(string caseChoiceIdentifier)
    {
        CaseChoiceData caseChoice = GameDataManager.Instance.GetCaseChoice(caseChoiceIdentifier);
        if(caseChoice != null)
        {
            caseChoice.ApplyFrequencyRamp();
        }
    }

    [Command(requiresAuthority =false)]
    public void CmdSetCompetitionChoice(int caseID, BirdName player)
    {
        RpcSetCompetitionChoice(caseID, player);
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
