using ChickenScratch;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ChickenScratch.CaseEmail;
using static ChickenScratch.ColourManager;

public class AccusationRound : PlayerRound
{
    public enum AccuseRole
    {
        accuser, accused, bystander
    }
    public AccuseRole playerAccuseRole;

    public enum RoundState
    {
        accusation, defense, voting, loading, reveal, transition, finished
    }
    public RoundState currentState;

    [SerializeField]
    private List<GameObject> taskEmailSectionPrefabs;

    [SerializeField]
    private ArgumentPanel accuserPanel;

    [SerializeField]
    private ArgumentPanel accusedPanel;

    [SerializeField]
    private Transform accuserHolder, accusedHolder;

    [SerializeField]
    private GameObject votingButtonsParent;

    [SerializeField]
    private Transform evidenceHolder;

    [SerializeField]
    private List<AccuseStateTimingData> accuseStateTimingDatas = new List<AccuseStateTimingData>();

    [SerializeField]
    private List<Transform> leftVoteArmHolders = new List<Transform>();

    [SerializeField]
    private List<Transform> rightVoteArmHolders = new List<Transform>();

    [SerializeField]
    private Transform accusedArmTarget, accuserArmTarget;

    [SerializeField]
    private AccusationReveal accusationReveal;

    private BirdName accuserBird, accusedBird;
    private Dictionary<CaseEmailTaskType, GameObject> taskEmailSectionPrefabMap = new Dictionary<CaseEmailTaskType, GameObject>();
    private Dictionary<RoundState, AccuseStateTimingData> accuseStateTimingMap = new Dictionary<RoundState, AccuseStateTimingData>();
    private Dictionary<BirdName, StretchArm> leftVoteArms = new Dictionary<BirdName, StretchArm>();
    private Dictionary<BirdName, StretchArm> rightVoteArms = new Dictionary<BirdName, StretchArm>();
    private Dictionary<BirdName, List<BirdName>> voteMap = new Dictionary<BirdName, List<BirdName>>();
    private float timeInState = 0f;
    private int totalVoters = 0;
    private int voteHammerThreshold = 0;

    public BirdName eliminatedPlayer => _eliminatedPlayer;
    private BirdName _eliminatedPlayer;

    private void Start()
    {
        //test();
    }
    private void test()
    {
        GameManager.Instance.GenerateTestingData();
        GameManager.Instance.playerFlowManager.reviewRound.SetAccusation(BirdName.green, BirdName.teal);
        StartRound();
    }

    public override void StartRound()
    {
        base.StartRound();
        _eliminatedPlayer = BirdName.none;
        voteMap.Clear();
        taskEmailSectionPrefabMap.Clear();
        foreach (GameObject taskEmailSectionPrefab in taskEmailSectionPrefabs)
        {
            CaseEmailSection caseEmailSection = taskEmailSectionPrefab.GetComponent<CaseEmailSection>();
            taskEmailSectionPrefabMap.Add(caseEmailSection.taskType, taskEmailSectionPrefab);
        }
        accuseStateTimingMap.Clear();
        foreach(AccuseStateTimingData stateTiming in accuseStateTimingDatas)
        {
            accuseStateTimingMap.Add(stateTiming.state, stateTiming);
        }
        totalVoters = SettingsManager.Instance.GetAllActiveBirds().Count - 2;
        voteHammerThreshold = totalVoters / 2 + 1;

        accuserBird = GameManager.Instance.playerFlowManager.reviewRound.accuserBird;
        accusedBird = GameManager.Instance.playerFlowManager.reviewRound.accusedBird;
        voteMap.Add(accuserBird, new List<BirdName>());
        voteMap.Add(accusedBird, new List<BirdName>());

        ClearAccuseBirds();
        BirdData accuserBirdData = GameDataManager.Instance.GetBird(accuserBird);
        if(accuserBirdData != null)
        {
            Instantiate(accuserBirdData.accuseBirdPrefab, accuserHolder);
        }
        BirdData accusedBirdData = GameDataManager.Instance.GetBird(accusedBird);
        if(accusedBirdData != null)
        {
            Instantiate(accusedBirdData.accuseBirdPrefab, accusedHolder);
        }

        accuserPanel.gameObject.SetActive(false);
        accusedPanel.gameObject.SetActive(false);
        votingButtonsParent.SetActive(false);
        ClearEvidence();

        if (accuserBird == SettingsManager.Instance.birdName)
        {
            playerAccuseRole = AccuseRole.accuser;
            InitializeAccuser();
        }
        else if (accusedBird == SettingsManager.Instance.birdName)
        {
            playerAccuseRole = AccuseRole.accused;
            InitializeAccused();
        }
        else
        {
            playerAccuseRole = AccuseRole.bystander;
            InitializeBystander();
        }

        GenerateArms();

        //Add transition conditions for all players
        foreach (BirdName bird in SettingsManager.Instance.GetAllActiveBirds())
        {
            GameManager.Instance.gameFlowManager.addTransitionCondition("accusations_loaded:" + bird.ToString());
            GameManager.Instance.gameFlowManager.addTransitionCondition("reveal_complete:" + bird.ToString());
        }
    }

    private void InitializeAccuser()
    {
        accuserPanel.Initialize(accusedBird, accuserBird);
        accuserPanel.gameObject.SetActive(true);
    }

    private void InitializeAccused()
    {
        accusedPanel.Initialize(accusedBird, accuserBird);
        accusedPanel.gameObject.SetActive(true);
    }

    private void InitializeBystander()
    {

    }

    private void GenerateArms()
    {
        leftVoteArms.Clear();
        rightVoteArms.Clear();
        //Delete existing arms
        List<Transform> transformsToDestroy = new List<Transform>();
        foreach(Transform armHolder in leftVoteArmHolders)
        {
            foreach(Transform child in armHolder)
            {
                transformsToDestroy.Add(child);
            }
        }
        foreach(Transform armHolder in rightVoteArmHolders)
        {
            foreach(Transform child in armHolder)
            {
                transformsToDestroy.Add(child);
            }
        }
        for(int i = transformsToDestroy.Count - 1; i >=0; i--)
        {
            Destroy(transformsToDestroy[i].gameObject);
        }

        leftVoteArmHolders = leftVoteArmHolders.OrderBy(x => Guid.NewGuid()).ToList();
        rightVoteArmHolders = rightVoteArmHolders.OrderBy(x => Guid.NewGuid()).ToList();

        List<BirdName> allBirds = SettingsManager.Instance.GetAllActiveBirds();
        int iterator = 0;
        foreach(BirdName bird in allBirds)
        {
            if(bird == accusedBird || bird == accuserBird)
            {
                continue;
            }
            BirdData playerBirdData = GameDataManager.Instance.GetBird(bird);
            if(playerBirdData == null)
            {
                continue;
            }

            //Instantiate the left arm
            GameObject leftArmObject = Instantiate(playerBirdData.accuseArmPrefab, leftVoteArmHolders[iterator]);
            StretchArm leftArm = leftArmObject.GetComponent<StretchArm>();
            leftVoteArms.Add(bird, leftArm);

            //Instantiate the right arm
            GameObject rightArmObject = Instantiate(playerBirdData.accuseArmPrefab, rightVoteArmHolders[iterator]);
            StretchArm rightArm = rightArmObject.GetComponent<StretchArm>();
            rightVoteArms.Add(bird, rightArm);
            iterator++;
        }
    }

    public void ShowEvidence(int caseID, int taskID)
    {
        ClearEvidence();
        EndgameCaseData caseData = GameManager.Instance.playerFlowManager.slidesRound.caseDataMap[caseID];
        EndgameTaskData taskData = caseData.taskDataMap[taskID];
        switch (taskData.taskType)
        {
            case TaskData.TaskType.base_drawing:
            case TaskData.TaskType.copy_drawing:
            case TaskData.TaskType.add_drawing:
            case TaskData.TaskType.prompt_drawing:
            case TaskData.TaskType.compile_drawing:
                GameObject drawingCaseEmailSectionObject = Instantiate(taskEmailSectionPrefabMap[CaseEmailTaskType.drawing], evidenceHolder);
                DrawingCaseEmailSection drawingCaseEmailSection = drawingCaseEmailSectionObject.GetComponent<DrawingCaseEmailSection>();
                drawingCaseEmailSection.Initialize(taskData.drawingData, taskData.ratingData, 1f);
                break;
            case TaskData.TaskType.prompting:
                GameObject promptCaseEmailSectionObject = Instantiate(taskEmailSectionPrefabMap[CaseEmailTaskType.prompt], evidenceHolder);
                PromptCaseEmailSection promptCaseEmailSection = promptCaseEmailSectionObject.GetComponent<PromptCaseEmailSection>();
                promptCaseEmailSection.Initialize(taskData.promptData, taskData.ratingData);
                break;
            case TaskData.TaskType.base_guessing:
                GameObject guessCaseEmailSectionObject = Instantiate(taskEmailSectionPrefabMap[CaseEmailTaskType.guess], evidenceHolder);
                GuessCaseEmailSection guessCaseEmailSection = guessCaseEmailSectionObject.GetComponent<GuessCaseEmailSection>();
                guessCaseEmailSection.Initialize(caseData.correctWordIdentifierMap, caseData.guessData, taskData.ratingData);
                break;

        }
    }

    private void ClearEvidence()
    {
        List<Transform> transformsToDestroy = new List<Transform>();
        foreach(Transform child in evidenceHolder)
        {
            transformsToDestroy.Add(child);
        }
        for(int i = transformsToDestroy.Count - 1; i >= 0; i--)
        {
            Destroy(transformsToDestroy[i].gameObject);
        }
    }

    private void ClearAccuseBirds()
    {
        List<Transform> transformsToDestroy = new List<Transform>();
        foreach (Transform child in accusedHolder)
        {
            transformsToDestroy.Add(child);
        }
        foreach (Transform child in accuserHolder)
        {
            transformsToDestroy.Add(child);
        }
        for (int i = transformsToDestroy.Count - 1; i >= 0; i--)
        {
            Destroy(transformsToDestroy[i].gameObject);
        }
    }

    public void UpdateState(AccuseStateTimingData stateData)
    {
        currentState = stateData.state;
        timeInRound = stateData.duration;
        ClearEvidence();
        switch(currentState)
        {
            case RoundState.accusation:
                if(SettingsManager.Instance.birdName == accuserBird)
                {
                    accuserPanel.gameObject.SetActive(true);
                    accuserPanel.ShowChoiceButton();
                }
                else if(SettingsManager.Instance.birdName == accusedBird)
                {
                    accusedPanel.gameObject.SetActive(true);
                    accusedPanel.HideChoiceButton();
                }
                break;
            case RoundState.defense:
                if (SettingsManager.Instance.birdName == accuserBird)
                {
                    accuserPanel.HideChoiceButton();
                }
                else if (SettingsManager.Instance.birdName == accusedBird)
                {
                    accusedPanel.ShowChoiceButton();
                }
                break;
            case RoundState.voting:
                if (SettingsManager.Instance.birdName == accuserBird)
                {
                    accuserPanel.HideChoiceButton();
                }
                else if (SettingsManager.Instance.birdName == accusedBird)
                {
                    accusedPanel.HideChoiceButton();
                }
                else
                {
                    votingButtonsParent.SetActive(true);
                }

                break;
            case RoundState.reveal:
                break;
        }
    }

    public void VoteAccuser()
    {
        GameManager.Instance.gameDataHandler.CmdAccuseVote(SettingsManager.Instance.birdName, accuserBird);
        SetVoteArmTarget(SettingsManager.Instance.birdName, accuserBird);
        votingButtonsParent.SetActive(false);
    }

    public void VoteAccused()
    {
        GameManager.Instance.gameDataHandler.CmdAccuseVote(SettingsManager.Instance.birdName, accusedBird);
        votingButtonsParent.SetActive(false);
    }

    public void SetVoteArmTarget(BirdName voter, BirdName votee)
    {
        if(votee == accuserBird)
        {
            leftVoteArms[voter].targetPosition = accuserArmTarget.position;
        }
        else if(votee == accusedBird)
        {
            rightVoteArms[voter].targetPosition = accusedArmTarget.position;
        }
    }

    public void AddVote(BirdName voter, BirdName target)
    {
        voteMap[target].Add(voter);
        if (voteMap[target].Count >= voteHammerThreshold)
        {
            //End the round with the target being eliminated
            BroadcastAccusationData();
        }

    }

    private void BroadcastAccusationData()
    {
        if (voteMap[accusedBird].Count != voteMap[accuserBird].Count)
        {
            if (voteMap[accusedBird].Count > voteMap[accuserBird].Count)
            {
                _eliminatedPlayer = accusedBird;
            }
            else
            {
                _eliminatedPlayer = accuserBird;
            }

            //Create PlayerRevealData and send it
            string playerName = GameManager.Instance.playerFlowManager.playerNameMap[eliminatedPlayer];
            PlayerData player = GameManager.Instance.gameFlowManager.gamePlayers[eliminatedPlayer];
            player.isEliminated = true;
            RoleData.RoleType playerRole = player.playerRoleType;
            PlayerRevealNetData revealData = new PlayerRevealNetData() { playerBird = eliminatedPlayer, playerName = playerName, roleType = playerRole };
            GameManager.Instance.gameDataHandler.RpcInitializeAccuseRevealWrapper(accuserBird, accusedBird, voteMap, revealData);
        }
        else
        {
            //Broadcast the reveal data
            GameManager.Instance.gameDataHandler.RpcInitializeAccuseRevealWrapper(accuserBird, accusedBird,voteMap);
        }
        
    }

    public void StartReveal(BirdName inAccuser, BirdName inAccused, Dictionary<BirdName,List<BirdName>> inVoteMap)
    {
        accusationReveal.Initialize(inAccuser, inAccused, inVoteMap);
        accusationReveal.Activate();
        currentState = RoundState.reveal;
        GameManager.Instance.gameDataHandler.CmdTransitionCondition("accusations_loaded:" + SettingsManager.Instance.birdName);
    }

    public void StartReveal(BirdName inAccuser, BirdName inAccused, Dictionary<BirdName, List<BirdName>> inVoteMap, PlayerRevealNetData revealData)
    {
        accusationReveal.Initialize(inAccuser, inAccused, inVoteMap, revealData);
        accusationReveal.Activate();
        currentState = RoundState.reveal;
        GameManager.Instance.gameDataHandler.CmdTransitionCondition("accusations_loaded:" + SettingsManager.Instance.birdName);
    }

    void Update()
    {
        if(currentState == RoundState.loading)
        {
            return;
        }
        if(timeInState > 0f)
        {
            timeInState -= Time.deltaTime;

            if(timeInState > 0)
            {
                accusedPanel.gameObject.SetActive(false);
                accuserPanel.gameObject.SetActive(false);
                votingButtonsParent.SetActive(false);
                ClearEvidence();

                if (SettingsManager.Instance.isHost)
                {
                    //Broadcast the transition to the next phase
                    AccuseStateTimingData currentStateTiming = accuseStateTimingMap[currentState];
                    AccuseStateTimingData nextStateTiming = accuseStateTimingMap[currentStateTiming.nextState];
                    GameManager.Instance.gameDataHandler.RpcUpdateAccusationState(nextStateTiming);
                }
                else
                {
                    currentState = RoundState.transition;
                }
            }
        }
        

    }
}
