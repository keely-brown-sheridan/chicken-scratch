
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;
using static ChickenScratch.ColourManager;

namespace ChickenScratch
{
    public class AccoladesRound : PlayerRound
    {
        public AccoladesStatManager playerStatsManager;

        public bool isActive = false;

        [SerializeField]
        private List<AccoladesBirdRow> allAccoladeBirdRows;

        private Dictionary<BirdName, AccoladesBirdRow> accoladeBirdRowMap = new Dictionary<BirdName, AccoladesBirdRow>();

        [SerializeField]
        private float cardPlacementWaitVariance;

        [SerializeField]
        private List<GameObject> awardPlaquePrefabs = new List<GameObject>();

        [SerializeField]
        private GameObject mostLikedAwardPrefab;

        [SerializeField]
        private Transform awardsParent;

        [SerializeField]
        private float moveSpeed;

        [SerializeField]
        private float arrivalDistance;

        [SerializeField]
        private float placingCardsTime;
        [SerializeField]
        private float finishingTime;

        [SerializeField]
        private Transform accoladesUITransform;

        private enum State
        {
            placing_cards, move_to_awards, finished
        }
        private State currentState = State.placing_cards;

        

        private Dictionary<int, GameObject> awardPlaquePrefabMap = new Dictionary<int, GameObject>();
        private List<AccoladeBirdAward> spawnedAwards = new List<AccoladeBirdAward>();
        private int currentAwardIndex = 0;
        private Vector3 initialAccoladesUIPosition;
        private float targetX;
        private float timeWaiting = 0f;

        

        private void Start()
        {
            initialAccoladesUIPosition = accoladesUITransform.position;
            foreach(GameObject awardPlaquePrefab in awardPlaquePrefabs)
            {
                AccoladeBirdAward award = awardPlaquePrefab.GetComponent<AccoladeBirdAward>();
                awardPlaquePrefabMap.Add(award.rank, awardPlaquePrefab);
            }

            //Test();
            
        }

        private void Test()
        {

            initializeAccoladeBirdRow(0, BirdName.red);
            initializeAccoladeBirdRow(1, BirdName.blue);
            initializeAccoladeBirdRow(2, BirdName.green);
            initializeAccoladeBirdRow(3, BirdName.teal);
            initializeAccoladeBirdRow(4, BirdName.orange);
            initializeAccoladeBirdRow(5, BirdName.yellow);
            initializeAccoladeBirdRow(6, BirdName.pink);
            initializeAccoladeBirdRow(7, BirdName.maroon);
            EndgameCaseData testCase = new EndgameCaseData();

            //create task queue
            EndgameTaskData testTask = new EndgameTaskData();
            testTask.taskType = TaskData.TaskType.base_guessing;
            testTask.ratingData = new PlayerRatingData() { likeCount = 1, target = BirdName.red };
            testTask.assignedPlayer = BirdName.red;
            GameManager.Instance.playerFlowManager.playerNameMap.Add(BirdName.red, "beebodeebo");
            GameManager.Instance.playerFlowManager.playerNameMap.Add(BirdName.blue, "beebodeebo");
            GameManager.Instance.playerFlowManager.playerNameMap.Add(BirdName.green, "beebodeebo");
            GameManager.Instance.playerFlowManager.playerNameMap.Add(BirdName.teal, "beebodeebo");
            GameManager.Instance.playerFlowManager.playerNameMap.Add(BirdName.orange, "beebodeebo");
            GameManager.Instance.playerFlowManager.playerNameMap.Add(BirdName.yellow, "beebodeebo");
            GameManager.Instance.playerFlowManager.playerNameMap.Add(BirdName.pink, "beebodeebo");
            GameManager.Instance.playerFlowManager.playerNameMap.Add(BirdName.maroon, "beebodeebo");
            SettingsManager.Instance.AssignBirdToPlayer(BirdName.red, "beebodeebo");
            SettingsManager.Instance.AssignBirdToPlayer(BirdName.blue, "beebodeebo");
            SettingsManager.Instance.AssignBirdToPlayer(BirdName.green, "beebodeebo");
            SettingsManager.Instance.AssignBirdToPlayer(BirdName.teal, "beebodeebo");
            SettingsManager.Instance.AssignBirdToPlayer(BirdName.orange, "beebodeebo");
            SettingsManager.Instance.AssignBirdToPlayer(BirdName.yellow, "beebodeebo");
            SettingsManager.Instance.AssignBirdToPlayer(BirdName.pink, "beebodeebo");
            SettingsManager.Instance.AssignBirdToPlayer(BirdName.maroon, "beebodeebo");
            testCase.taskDataMap.Add(1, testTask);
            testCase.correctWordIdentifierMap = new Dictionary<int, string>() { { 1, "prefixes-DRAGGING" },{ 2, "nouns-AARDVARK" } };

            GameManager.Instance.playerFlowManager.slidesRound.caseDataMap.Add(1, testCase);
            StartRound();
        }

        public override void StartRound()
        {
            base.StartRound();

            initializeAccoladesRound();
        }

        private void Update()
        {
            if (isActive)
            {
                switch(currentState)
                {
                    case State.placing_cards:
                        timeWaiting += Time.deltaTime;
                        if(timeWaiting > placingCardsTime)
                        {
                            currentState = State.move_to_awards;
                            timeWaiting = 0f;
                        }
                        break;
                    case State.move_to_awards:
                        accoladesUITransform.position -= Vector3.right* moveSpeed * Time.deltaTime;
                        float distanceFromStart = Mathf.Abs(initialAccoladesUIPosition.x - accoladesUITransform.position.x);

                        if(currentAwardIndex < spawnedAwards.Count)
                        {
                            if(distanceFromStart > spawnedAwards[currentAwardIndex].xOffset)
                            {
                                spawnedAwards[currentAwardIndex].Lift();
                                currentAwardIndex++;
                            }
                        }

                        if (distanceFromStart > targetX)
                        {
                            currentState = State.finished;
                        }
                        break;
                    case State.finished:
                        timeWaiting += Time.deltaTime;
                        if(timeWaiting > finishingTime)
                        {
                            isActive = false;
                            timeWaiting = 0f;
                        }
                        break;
                }

            }

        }

        public void SetPlayerAccoladeCards(Dictionary<BirdName, AccoladesStatManager.StatRole> statRoleMap)
        {
            foreach (KeyValuePair<BirdName, AccoladesStatManager.StatRole> statRole in statRoleMap)
            {
                List<AccoladesBirdRow> matchingBirdRows = allAccoladeBirdRows.Where(br => br.birdName == statRole.Key).ToList();
                if(matchingBirdRows.Count == 0)
                {
                    Debug.LogError("ERROR[SetPlayerAccoladeCards]: Could not find matching accolade bird row for player[" + statRole.Key.ToString() + "]");
                    return;
                }
                AccoladesBirdRow currentCard = matchingBirdRows[0];
                currentCard.statRoleText.text = statRole.Value.name;
                currentCard.statDescriptionText.text = statRole.Value.description;
            }
        }

        

        public void initializeAccoladeBirdRow(int index, BirdName birdName)
        {
            if (accoladeBirdRowMap.ContainsKey(birdName))
            {
                return;
            }
            if (allAccoladeBirdRows.Count <= index)
            {
                Debug.LogError("Could not initialize accolade bird row because index["+index.ToString()+"] is outside of the range of allAccoladeBirdRows["+allAccoladeBirdRows.Count.ToString()+"]");
                return;
            }
            AccoladesBirdRow birdRow = allAccoladeBirdRows[index];
            birdRow.birdName = birdName;
            BirdData accoladeBird = GameDataManager.Instance.GetBird(birdName);
            if(accoladeBird == null)
            {
                Debug.LogError("Could not initialize accolade bird row colours because bird["+birdName.ToString()+"] has not been mapped in the ColourManager.");
            }
            else
            {
                birdRow.pinImage.color = accoladeBird.colour;
                BirdHatData.HatType birdHat = GameManager.Instance.playerFlowManager.GetBirdHatType(birdName);
                birdRow.birdHeadImage.Initialize(birdName, birdHat);
            }
            
            birdRow.gameObject.SetActive(true);
            accoladeBirdRowMap.Add(birdName, birdRow);
            birdRow.isInitialized = true;
        }

        private void initializeAccoladesRound()
        {
            Dictionary<BirdName, int> playerBirdbuckMap = new Dictionary<BirdName, int>();
            Dictionary<BirdName, int> playerLikeMap = new Dictionary<BirdName, int>();
            BirdName mostLikedPlayerCandidate = BirdName.none;

            foreach (KeyValuePair<ColourManager.BirdName, string> player in GameManager.Instance.playerFlowManager.playerNameMap)
            {
                playerBirdbuckMap.Add(player.Key, 0);
                playerLikeMap.Add(player.Key, 0);

            }
            foreach (EndgameCaseData currentCase in GameManager.Instance.playerFlowManager.slidesRound.caseDataMap.Values)
            {
                foreach(EndgameTaskData currentTask in currentCase.taskDataMap.Values)
                {
                    PlayerRatingData rating = currentTask.ratingData;
                    if (playerLikeMap.ContainsKey(rating.target))
                    {
                        playerLikeMap[rating.target] += rating.likeCount;
                    }
                    if(playerBirdbuckMap.ContainsKey(currentTask.assignedPlayer))
                    {
                        playerBirdbuckMap[currentTask.assignedPlayer] += currentCase.GetPointsForPlayerOnTask(currentTask.assignedPlayer);
                    }
                }
            }

            BirdData currentBird;
            //Iterate over each player to determine candidates for eotm and ufr
            foreach (KeyValuePair<BirdName, int> playerLikes in playerLikeMap)
            {
                if(mostLikedPlayerCandidate == BirdName.none)
                {
                    if (playerLikes.Value != 0)
                    {
                        mostLikedPlayerCandidate = playerLikes.Key;
                        PlayerFlowManager.employeeOfTheMonth = mostLikedPlayerCandidate;
                    }
                }
                else
                {
                    if (playerLikeMap[mostLikedPlayerCandidate] < playerLikes.Value)
                    {
                        mostLikedPlayerCandidate = playerLikes.Key;
                        PlayerFlowManager.employeeOfTheMonth = mostLikedPlayerCandidate;
                    }
                }

                AccoladesBirdRow currentRow;
                if (accoladeBirdRowMap.ContainsKey(playerLikes.Key))
                {
                    currentRow = accoladeBirdRowMap[playerLikes.Key];

                    //Set the stats for the corkboard
                    currentRow.gameObject.SetActive(true);
                    currentBird = GameDataManager.Instance.GetBird(playerLikes.Key);
                    if(currentBird == null)
                    {
                        Debug.LogError("Could not set stats for the review bird[" + playerLikes.Key.ToString() + "] because it has not been mapped in the Colour Manager.");
                    }
                    else
                    {
                        currentRow.playerNameText.color = currentBird.colour;
                    }
                    
                    currentRow.playerNameText.text = GameManager.Instance.playerFlowManager.playerNameMap[playerLikes.Key];
                    float randomizedPlacementWait = Random.Range(0, cardPlacementWaitVariance);

                    currentRow.StartPlacing(randomizedPlacementWait);
                }
            }

            //Set the accolades
            List<BirdName> orderedBirds = SettingsManager.Instance.GetAllActiveBirds().OrderBy(b => playerBirdbuckMap[b]).ToList();
            List<int> orderedEarnings = new List<int>();
            List<GameObject> awardsToSpawn = new List<GameObject>();
            foreach(BirdName bird in orderedBirds)
            {
                orderedEarnings.Add(playerBirdbuckMap[bird]);
            }

            int highestRank = 6;
            if(SettingsManager.Instance.GetPlayerNameCount() > 6)
            {
                highestRank = 8;
            }
            else if(SettingsManager.Instance.GetPlayerNameCount() > 4)
            {
                highestRank = 7;
            }

            //Work backwards down from the highest rank to determine what needs to be spawned
            int currentRank = highestRank;
            awardsToSpawn.Insert(0, awardPlaquePrefabMap[currentRank]);

            //The top award should not be repeated
            currentRank--;
            awardsToSpawn.Insert(0, awardPlaquePrefabMap[currentRank]);

            for (int i = orderedBirds.Count - 3; i >= 0; i--)
            {
                //Repeat the award if the birdbucks are equal to the previous bird's birdbucks
                if (orderedEarnings[i] != orderedEarnings[i+1])
                {
                    currentRank--;
                }
                awardsToSpawn.Insert(0, awardPlaquePrefabMap[currentRank]);
            }

            //Iterate over the prefabs and spawn them at appropriate intervals in terms of their position
            targetX = 0;
            float spacingBetweenAwards = 10f;
            GameObject spawnedAwardObject;
            AccoladeBirdAward award;
            for (int i = 0; i < awardsToSpawn.Count; i++)
            {
                spawnedAwardObject = Instantiate(awardsToSpawn[i], new Vector3(targetX + awardsParent.position.x, awardsParent.position.y, 0f), Quaternion.identity, awardsParent);

                award = spawnedAwardObject.GetComponent<AccoladeBirdAward>();
                award.Initialize(orderedBirds[i], GameManager.Instance.playerFlowManager.playerNameMap[orderedBirds[i]], orderedEarnings[i], targetX);
                spawnedAwards.Add(award);
                targetX += award.width + spacingBetweenAwards;
            }
            if(mostLikedPlayerCandidate != BirdName.none)
            {
                spawnedAwardObject = Instantiate(mostLikedAwardPrefab, new Vector3(targetX + awardsParent.position.x, awardsParent.position.y, 0f), Quaternion.identity, awardsParent);

                award = spawnedAwardObject.GetComponent<AccoladeBirdAward>();
                award.Initialize(mostLikedPlayerCandidate, GameManager.Instance.playerFlowManager.playerNameMap[mostLikedPlayerCandidate], playerLikeMap[mostLikedPlayerCandidate], targetX);
                spawnedAwards.Add(award);
                targetX += award.width + spacingBetweenAwards;
            }
            

            isActive = true;
        }
    }

    [System.Serializable]
    public class CameraDock
    {
        public enum CameraState
        {
            stats_rest, stats_to_accs, accs_rest, accs_to_result, result, reset
        }
        public CameraState state;
        public CameraState nextState;

        public Vector3 position;
        public Vector3 zoom;

        public float transitionDuration;

        public float restingTime;
        public bool restingFinished = false;

        public void setRelativePositions()
        {
            position = Camera.main.ViewportToWorldPoint(position);
            zoom = Camera.main.ViewportToWorldPoint(position);
        }
    }
}