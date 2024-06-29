using ChickenScratch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ChickenScratch.ColourManager;

public class AccusationReveal : MonoBehaviour
{
    [SerializeField]
    private GameObject accusedBirdObject, accuserBirdObject;
    [SerializeField]
    private GameObject votePrefab;

    [SerializeField]
    private Transform accuserVoteHolder, accusedVoteHolder;

    [SerializeField]
    private Image accusedBirdImage, accuserBirdImage;

    [SerializeField]
    private GameObject votingObject;

    [SerializeField]
    private GameObject revealObject;

    [SerializeField]
    private Image revealBirdImage;

    [SerializeField]
    private GameObject revealBirdObject;

    [SerializeField]
    private TMPro.TMP_Text revealBirdNameText, teaseText, revealText, noRevealText;

    [SerializeField]
    private List<RevealStateData> revealStates;

    [SerializeField]
    private float initializingTime;

    [SerializeField]
    private string voteSFX;

    public enum State
    {
        inactive, initializing, initialized, show_targets, show_votes, pause, no_reveal, show_revealing_player, tease, reveal, finish
    }

    private List<GameObject> accusedVoteObjects = new List<GameObject>();
    private List<GameObject> accuserVoteObjects = new List<GameObject>();
    private State currentState = State.inactive;

    private float timeInCurrentState = 0f;
    private PlayerRevealNetData revealingData = null;
    private Dictionary<State, RevealStateData> revealStateMap = new Dictionary<State, RevealStateData>();
    private int currentVoteIndex = 0;

    private void Start()
    {
        //test();
    }

    private void test()
    {
        GameManager.Instance.GenerateTestingData();
        Dictionary<BirdName, List<BirdName>> testVoteMap = new Dictionary<BirdName, List<BirdName>>();
        testVoteMap.Add(BirdName.red, new List<BirdName>());
        testVoteMap[BirdName.red].Add(BirdName.blue);
        testVoteMap.Add(BirdName.teal, new List<BirdName>());
        testVoteMap[BirdName.red].Add(BirdName.green);
        PlayerRevealNetData revealData = new PlayerRevealNetData() { playerBird = BirdName.red, playerName = "beebodeebo", roleType = RoleData.RoleType.botcher };
        Initialize(BirdName.red, BirdName.teal, testVoteMap, revealData);
        Activate();
    }

    //This is used if there isn't a reveal
    public void Initialize(BirdName accuser, BirdName accused, Dictionary<BirdName,List<BirdName>> voteMap)
    {
        Reset();
        revealingData = null;
        CreateVoteMap(accuser, accused, voteMap);
    }

    //This is used if a player is going to be revealed
    public void Initialize(BirdName accuser, BirdName accused, Dictionary<BirdName,List<BirdName>> voteMap, PlayerRevealNetData revealData)
    {
        Reset();
        revealingData = revealData;
        CreateVoteMap(accuser, accused, voteMap);
        CreateReveal(revealData);
    }

    private void Reset()
    {
        revealStateMap.Clear();
        foreach (RevealStateData revealState in revealStates)
        {
            revealStateMap.Add(revealState.currentState, revealState);
        }
        revealObject.SetActive(false);
        votingObject.SetActive(true);
        revealBirdNameText.gameObject.SetActive(false);
        teaseText.gameObject.SetActive(false);
        revealText.gameObject.SetActive(false);
        noRevealText.gameObject.SetActive(false);
        revealBirdObject.SetActive(false);
        accusedBirdObject.SetActive(false);
        accuserBirdObject.SetActive(false);
    }

    private void CreateVoteMap(BirdName accuser, BirdName accused, Dictionary<BirdName,List<BirdName>> voteMap)
    {
        currentVoteIndex = 0;
        BirdData accuserBird = GameDataManager.Instance.GetBird(accuser);
        accuserBirdImage.gameObject.SetActive(false);
        if(accuserBird != null)
        {
            accuserBirdImage.sprite = accuserBird.faceSprite;
        }
        BirdData accusedBird = GameDataManager.Instance.GetBird(accused);
        accusedBirdImage.gameObject.SetActive(false);
        if(accusedBird != null)
        {
            accusedBirdImage.sprite = accusedBird.faceSprite;
        }
        //Clear any existing votes from a previous reveal
        List<Transform> transformsToDestroy = new List<Transform>();
        foreach(Transform child in accusedVoteHolder)
        {
            transformsToDestroy.Add(child);
        }
        foreach (Transform child in accuserVoteHolder)
        {
            transformsToDestroy.Add(child);
        }
        for(int i = transformsToDestroy.Count - 1; i >= 0; i--)
        {
            Destroy(transformsToDestroy[i].gameObject);
        }

        accusedVoteObjects.Clear();
        accuserVoteObjects.Clear();

        BirdData voteBird;
        GameObject voteObject;
        BirdVoteVisualization birdVoteVisual;
        //Create vote objects for each of the votes
        foreach (BirdName vote in voteMap[accused])
        {
            voteBird = GameDataManager.Instance.GetBird(vote);
            if(voteBird != null)
            {
                voteObject = Instantiate(votePrefab, accusedVoteHolder);
                birdVoteVisual = voteObject.GetComponent<BirdVoteVisualization>();
                birdVoteVisual.Initialize(voteBird);
                accusedVoteObjects.Add(voteObject);
            }
        }

        foreach(BirdName vote in voteMap[accuser])
        {
            voteBird = GameDataManager.Instance.GetBird(vote);
            if(voteBird != null)
            {
                voteObject = Instantiate(votePrefab, accuserVoteHolder);
                birdVoteVisual = voteObject.GetComponent<BirdVoteVisualization>();
                birdVoteVisual.Initialize(voteBird);
                accuserVoteObjects.Add(voteObject);
            }
        }
    }

    private void CreateReveal(PlayerRevealNetData revealData)
    {
        BirdData revealBird = GameDataManager.Instance.GetBird(revealData.playerBird);
        if(revealBird != null)
        {
            revealBirdImage.sprite = revealBird.faceSprite;
            revealBirdNameText.text = revealData.playerName;
            revealBirdNameText.color = revealBird.colour;
        }
        RoleData revealRole = GameDataManager.Instance.GetRole(revealData.roleType);
        if(revealRole != null)
        {
            revealText.text = "a " + revealRole.roleName;
            revealText.color = revealRole.roleColour;
        }
        
    }

    public void Activate()
    {
        gameObject.SetActive(true);
        currentState = State.initializing;
        timeInCurrentState = initializingTime;

    }

    // Update is called once per frame
    void Update()
    {
        if(currentState == State.inactive || currentState == State.finish)
        {
            return;
        }

        timeInCurrentState -= Time.deltaTime;
        if(timeInCurrentState < 0)
        {
            TransitionState();
        }
    }

    private void TransitionState()
    {
        switch(currentState)
        {
            case State.show_votes:
                bool aVoteWasRevealed = false;
                if(accusedVoteObjects.Count > currentVoteIndex)
                {
                    accusedVoteObjects[currentVoteIndex].SetActive(true);
                    aVoteWasRevealed = true;
                }
                if(accuserVoteObjects.Count > currentVoteIndex)
                {
                    accuserVoteObjects[currentVoteIndex].SetActive(true);
                    aVoteWasRevealed = true;
                }
                currentVoteIndex++;
                
                if(aVoteWasRevealed)
                {
                    //reset the timer
                    timeInCurrentState = revealStateMap[currentState].duration;

                    //play a sound effect
                    AudioManager.Instance.PlaySound(voteSFX);
                    
                    return;
                }
                else
                {
                    votingObject.SetActive(false);
                    revealObject.SetActive(true);
                }
                break;
            case State.pause:
                
                if(revealingData != null)
                {
                    currentState = State.show_revealing_player;
                    timeInCurrentState = revealStateMap[currentState].duration;
                    revealBirdObject.SetActive(true);
                    revealBirdImage.gameObject.SetActive(true);
                    revealBirdNameText.gameObject.SetActive(true);
                    return;
                }
                else
                {
                    currentState = State.no_reveal;
                    timeInCurrentState = revealStateMap[currentState].duration;
                    noRevealText.gameObject.SetActive(true);
                    return;
                }
            case State.no_reveal:
                
                break;
            case State.show_revealing_player:
                teaseText.gameObject.SetActive(true);
                break;
            case State.tease:
                revealText.gameObject.SetActive(true);
                break;
            case State.reveal:
                
                break;
        }
        RevealStateData previousStateData = revealStateMap[currentState];
        currentState = previousStateData.nextState;
        if(currentState == State.finish)
        {
            GameManager.Instance.gameDataHandler.CmdTransitionCondition("reveal_complete:" + SettingsManager.Instance.birdName.ToString());
            return;
        }
        RevealStateData nextStateData = revealStateMap[currentState];
        timeInCurrentState = nextStateData.duration;

        switch(currentState)
        {
            case State.show_targets:
                accuserBirdObject.SetActive(true);
                accusedBirdObject.SetActive(true);
                accusedBirdImage.gameObject.SetActive(true);
                accuserBirdImage.gameObject.SetActive(true);
                break;

        }
    }
}
