using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ChickenScratch.ColourManager;

namespace ChickenScratch
{
    public class WorkingGoalsManager : MonoBehaviour
    {
        public enum GoalType
        {
            worker_win, extra_vote, employee_review, endgame_result_state, invalid
        }

        public class Goal
        {
            public GoalType type = GoalType.invalid;
            public int requiredPoints = -1;
            public string name = "";

            public Goal(GoalType inType, int inRequiredPoints, string inName = "")
            {
                type = inType;
                requiredPoints = inRequiredPoints;
                name = inName;
            }
        }

        public class PlayerPoints
        {
            public BirdName player = BirdName.none;
            public int points = -1;
            public EndgameCaseData caseData;

            public PlayerPoints(BirdName inPlayer, int inPoints, EndgameCaseData inCaseData)
            {
                player = inPlayer;
                points = inPoints;
                caseData = inCaseData;
            }
        }

        public bool active = false;
        public bool graphingActive = true;
        public bool celebrationActive = false;
        public bool didWorkersWin = false;
        public float graphWidth = 5f;
        public float graphHeight = 5f;
        public GameObject linePrefab;
        public GameObject bgLinePrefab;
        public GameObject thresholdLinePrefab;
        public Vector3 startingPosition;

        public Vector3 flatGuessOffset, risingGuessOffset, spikingGuessOffset;
        public float flatGuessRotation, risingGuessRotation, spikingGuessRotation;
        public float timeToShowPoint = 1.0f;
        public List<string> graphingSounds;

        public GameObject extraVoteVisualPrefab, evaluateVisualPrefab, workerWinVisualPrefab, endlessModeResultVisualPrefab;
        public GameObject guessContainer;
        public List<Text> allWordTexts = new List<Text>();
        public Dictionary<int, Text> wordTextsMap = new Dictionary<int, Text>();
        public Image authorImage;

        [SerializeField] private Animator bossAnimator;
        [SerializeField] private ParticleSystem progressParticleEffect;
        [SerializeField] private Animator confettiCannon1, confettiCannon2, confettiCannon3, confettiCannon4;
        [SerializeField] private GameObject confettiWinEffect;
        [SerializeField] private Animator pinkSlipAnimator;
        [SerializeField] private EndlessModeResultForm endlessModeResultForm;
        [SerializeField] private float endlessModeSpeedupIncrement;
        [SerializeField] private int endlessModeSpeedupStartRound;
        [SerializeField] private float endlessModeSpeedupCap;
        [SerializeField] private Transform thresholdLineHolderTransform;
        [SerializeField]
        private LineRenderer yAxisLineRenderer;
        [SerializeField]
        private FinalEndgameResultManager finalResultManager;
        public GameObject resizableObjectsHolder;
        private Dictionary<int, Vector3> guessOffsetMap = new Dictionary<int, Vector3>();
        private Dictionary<int, float> guessRotationMap = new Dictionary<int, float>();

        private LineRenderer currentLineRenderer;
        private float _timeShowingPoint = 0f;
        private float _heightReached = 0f;
        private int _currentCaseIndex = 0;
        private List<Goal> _goals = new List<Goal>();
        private List<PlayerPoints> _pointsPerCase = new List<PlayerPoints>();
        private bool _isInitialized = false;
        private float _timeSinceLastConfettiCannonFired = 0f;
        private float _timeBetweenConfettiCannonFires = 1.0f;
        private int _currentConfettiCannonIndex = 0;
        private float _timeUntilEndgameResolution = 2.5f;
        private float _currentTimeWaitingForEndgameResolution = 0.0f;
        private float _currentWidthPerCase = 1.0f;
        private float _currentHeightPerPoint = 1.0f;
        private int _currentPoints = 0;
        private int _totalPoints = 0;
        private float _targetWidthForGraph = 0.0f;
        private float _targetHeightForGraph = 0.0f;
        private float _preTransitionWidthForGraph = 0.0f;
        private float _preTransitionHeightForGraph = 0.0f;
        private float _timeBetweenGraphResizingTransitions = 2.5f;
        private float _currentTimeResizingGraph = 0.0f;
        private float _minimumHeightPerSegment = 0.01f;
        private Goal _nextGoal;
        private int _initialPointsForGraphHeight = 0;
        private int _initialCasesForGraphWidth = 0;
        private Dictionary<string, WorkingGoal> _activeWorkingGoalsMap = new Dictionary<string, WorkingGoal>();
        private int _pointsRequiredForNextTransition = 0;
        private float _widthRatio = 1.0f;
        private float _heightRatio = 1.0f;

        public void Start()
        {
            initialize();
            //test();
        }

        private void initialize()
        {
            if (_isInitialized) return;

            foreach (Text wordText in allWordTexts)
            {
                IndexMap wordTextIndexMap = wordText.gameObject.GetComponent<IndexMap>();
                if (wordTextIndexMap)
                {
                    if (wordTextsMap.ContainsKey(wordTextIndexMap.index))
                    {
                        Debug.LogError("Could not add word text to word texts map in working goals manager because the index[" + wordTextIndexMap.index.ToString() + "] appeared more than once.");
                        continue;
                    }
                    wordTextsMap.Add(wordTextIndexMap.index, wordText);
                }
                else
                {
                    Debug.LogError("Attempted to add word text to the word text map in working goals manager but there was no index map for it.");
                }
            }

            guessOffsetMap.Clear();
            guessRotationMap.Clear();
            guessOffsetMap.Add(0, flatGuessOffset);
            guessRotationMap.Add(0, flatGuessRotation);
            guessOffsetMap.Add(1, risingGuessOffset);
            guessRotationMap.Add(1, risingGuessRotation);
            guessOffsetMap.Add(2, risingGuessOffset);
            guessRotationMap.Add(2, risingGuessRotation);
            guessOffsetMap.Add(3, spikingGuessOffset);
            guessRotationMap.Add(3, spikingGuessRotation);

            _isInitialized = true;
        }

        private void test()
        {
            _goals = new List<Goal>();
            Goal newGoal = new Goal(GoalType.endgame_result_state, 10);
            newGoal.name = "Pizza Party";
            _goals.Add(newGoal);
            _pointsPerCase = new List<PlayerPoints>();
            newGoal = new Goal(GoalType.endgame_result_state, 300);
            newGoal.name = "Up for Promotion";
            _goals.Add(newGoal);
            _pointsPerCase = new List<PlayerPoints>();
            EndgameCaseData test = new EndgameCaseData();
            test.correctWordsMap.Add(1, new CaseWordData("hot", null, -1));
            test.correctWordsMap.Add(2, new CaseWordData("potato", null, -1));
            test.guessesMap.Add(1, "bot");
            test.guessesMap.Add(2, "potato");
            _pointsPerCase.Add(new PlayerPoints(BirdName.red, 8, test));
            test = new EndgameCaseData();
            test.correctWordsMap.Add(1, new CaseWordData("hot", null, -1));
            test.correctWordsMap.Add(2, new CaseWordData("potato", null, -1));
            test.guessesMap.Add(1, "hot");
            test.guessesMap.Add(2, "tomato");
            _pointsPerCase.Add(new PlayerPoints(BirdName.green, 4, test));
            test = new EndgameCaseData();
            test.correctWordsMap.Add(1, new CaseWordData("hot", null, -1));
            test.correctWordsMap.Add(2, new CaseWordData("potato", null, -1));
            test.guessesMap.Add(1, "bot");
            test.guessesMap.Add(2, "totato");
            _pointsPerCase.Add(new PlayerPoints(BirdName.blue, 2, test));
            test = new EndgameCaseData();
            test.correctWordsMap.Add(1, new CaseWordData("hot", null, -1));
            test.correctWordsMap.Add(2, new CaseWordData("potato", null, -1));
            test.guessesMap.Add(1, "hot");
            test.guessesMap.Add(2, "potato");
            _pointsPerCase.Add(new PlayerPoints(BirdName.orange, 25, test));
            _pointsPerCase.Add(new PlayerPoints(BirdName.orange, 25, test));
            _pointsPerCase.Add(new PlayerPoints(BirdName.orange, 25, test));
            _pointsPerCase.Add(new PlayerPoints(BirdName.orange, 25, test));

            startShowingWorkingGoals(true);
        }

        public void Update()
        {
            if (active)
            {
                if (graphingActive)
                {
                    _timeShowingPoint += Time.deltaTime;
                    updateGraphLine(_currentWidthPerCase, _currentHeightPerPoint);
                    if (_currentTimeResizingGraph > 0)
                    {
                        updateGraphResizing();
                    }
                    if (_timeShowingPoint > timeToShowPoint)
                    {
                        startNextCase(_currentHeightPerPoint, _currentWidthPerCase);
                    }
                }

                else
                {
                    if (celebrationActive)
                    {
                        updateCelebrationState();
                    }
                    else
                    {
                        updateResolutionState();
                    }
                }
            }
        }

        public void initializeWorkingGoals(List<Goal> goals, List<PlayerPoints> playerPoints)
        {
            _goals = goals;
            _pointsPerCase = playerPoints;
        }

        private void updateGraphLine(float widthPerCase, float heightPerPoint)
        {
            float newX = startingPosition.x + widthPerCase * (_currentCaseIndex + (_timeShowingPoint / timeToShowPoint));
            float newY = _heightReached + (heightPerPoint * (_timeShowingPoint / timeToShowPoint) * _pointsPerCase[_currentCaseIndex].points);
            Vector3 newPosition = new Vector3(newX, newY, -2);
            currentLineRenderer.SetPosition(1, newPosition);
            progressParticleEffect.transform.localPosition = newPosition;
        }

        private void updateGraphResizing()
        {
            _currentTimeResizingGraph += Time.deltaTime;

            float transitionProgress = _currentTimeResizingGraph / _timeBetweenGraphResizingTransitions;

            float totalTransitionHeightDelta = (_targetHeightForGraph - _preTransitionHeightForGraph);
            float transitioningHeight = (_preTransitionHeightForGraph + totalTransitionHeightDelta * transitionProgress) / graphHeight;

            float totalTransitionWidthDelta = (_targetWidthForGraph - _preTransitionWidthForGraph);
            float transitioningWidth = (_preTransitionWidthForGraph + totalTransitionWidthDelta * transitionProgress) / graphWidth;

            Vector3 transitioningScale = new Vector3(transitioningWidth, transitioningHeight, 1.0f);

            resizableObjectsHolder.transform.localScale = transitioningScale;

            // int clampedPointsValue = Mathf.Min(_pointsPerCase[_currentCaseIndex].points,3);
            // float guessXPosition = (startingPosition.x + _currentWidthPerCase * _currentCaseIndex) * _widthRatio;
            // float guessYPosition = (_heightReached ) * _heightRatio;
            // float guessZPosition = -2;
            // Vector3 guessPosition = new Vector3(guessXPosition,guessYPosition,guessZPosition); 
            // guessContainer.transform.position = Camera.main.WorldToScreenPoint(guessPosition + guessOffsetMap[clampedPointsValue]);
            // guessContainer.transform.eulerAngles = new Vector3(0, 0, guessRotationMap[clampedPointsValue]);

            foreach (KeyValuePair<string, WorkingGoal> activeGoal in _activeWorkingGoalsMap)
            {

                Vector3 startingPoint = activeGoal.Value.lineRenderer.GetPosition(0);
                Vector3 endingPoint = activeGoal.Value.lineRenderer.GetPosition(1);
                float transitioningLineTotalDelta = activeGoal.Value.preTransitionHeight - activeGoal.Value.targetTransitionHeight;
                float transitioningLineHeight = activeGoal.Value.preTransitionHeight - transitioningLineTotalDelta * transitionProgress;
                Vector3 adjustedStartingPoint = new Vector3(startingPoint.x, transitioningLineHeight, startingPoint.z);
                Vector3 adjustedEndingPoint = new Vector3(endingPoint.x, transitioningLineHeight, endingPoint.z);
                activeGoal.Value.lineRenderer.SetPosition(0, adjustedStartingPoint);
                activeGoal.Value.lineRenderer.SetPosition(1, adjustedEndingPoint);
            }
            showGoalsUnderHeight(yAxisLineRenderer.GetPosition(1).y);

            if (_currentTimeResizingGraph > _timeBetweenGraphResizingTransitions)
            {
                _currentTimeResizingGraph = 0.0f;
                return;
            }
        }

        private void startNextCase(float heightPerSegment, float widthPerSegment)
        {
            _timeShowingPoint = 0.0f;
            _currentPoints += _pointsPerCase[_currentCaseIndex].points;
            _currentCaseIndex++;
            if (_pointsPerCase.Count > _currentCaseIndex)
            {
                Bird currentBird = ColourManager.Instance.birdMap[_pointsPerCase[_currentCaseIndex].player];
                currentLineRenderer = Instantiate(linePrefab, resizableObjectsHolder.transform).GetComponent<LineRenderer>();
                currentLineRenderer.material = currentBird.material;

                _heightReached += heightPerSegment * _pointsPerCase[_currentCaseIndex - 1].points;
                Vector3 endPoint = new Vector3(startingPosition.x + widthPerSegment * _currentCaseIndex, _heightReached, -2);
                currentLineRenderer.SetPosition(0, new Vector3(startingPosition.x + widthPerSegment * _currentCaseIndex, _heightReached, -2));
                currentLineRenderer.SetPosition(1, endPoint);

                int numberOfCorrectWords = 0;
                bossAnimator.SetTrigger("Reset");
                bossAnimator.SetBool("Mad", false);
                bossAnimator.SetBool("Happy", false);
                if (_pointsPerCase[_currentCaseIndex].caseData.correctWordsMap[1].value == _pointsPerCase[_currentCaseIndex].caseData.guessesMap[1])
                {
                    numberOfCorrectWords++;
                }
                if (_pointsPerCase[_currentCaseIndex].caseData.correctWordsMap[2].value == _pointsPerCase[_currentCaseIndex].caseData.guessesMap[2])
                {
                    numberOfCorrectWords++;
                }
                if (numberOfCorrectWords == 0)
                {
                    bossAnimator.SetBool("Mad", true);
                }
                else if (numberOfCorrectWords == 2)
                {
                    bossAnimator.SetBool("Happy", true);
                }

                //If the point threshold has been reached to move to the next case then start resizing
                int pointsAfterNextCase = _currentPoints + _pointsPerCase[_currentCaseIndex].points;
                if (pointsAfterNextCase > _pointsRequiredForNextTransition)
                {
                    //Start resizing
                    _preTransitionWidthForGraph = resizableObjectsHolder.transform.localScale.x * graphWidth;
                    _preTransitionHeightForGraph = resizableObjectsHolder.transform.localScale.y * graphHeight;

                    _nextGoal = getLowestGoalOverPoints(pointsAfterNextCase);
                    if (_nextGoal == null)
                    {
                        //If the points are still higher than the highest goal, then the height will be based on the next case
                        //The target width will be expanding out just to this current next case
                        _heightRatio = (float)_initialPointsForGraphHeight / (float)pointsAfterNextCase;
                        _targetHeightForGraph = graphHeight * _heightRatio;
                        _widthRatio = (float)_initialCasesForGraphWidth / (float)(_currentCaseIndex + 2);
                        _targetWidthForGraph = graphWidth * _widthRatio;
                    }
                    else
                    {
                        //Else the height will be the highest goal's threshold
                        //The target width will be number of cases it takes to get to the highest goal's threshold
                        _heightRatio = (float)_initialPointsForGraphHeight / (float)_nextGoal.requiredPoints;
                        _targetHeightForGraph = graphHeight * _heightRatio;
                        int casesToNextGoal = getNumberOfCasesToGoal(_nextGoal);
                        _widthRatio = (float)_initialCasesForGraphWidth / (float)casesToNextGoal;
                        _targetWidthForGraph = graphWidth * _widthRatio;
                    }

                    foreach (WorkingGoal workingGoal in _activeWorkingGoalsMap.Values)
                    {
                        float originalGoalHeight = startingPosition.y + _currentHeightPerPoint * workingGoal.pointThreshold;
                        float scaledGoalHeight = originalGoalHeight * _heightRatio;
                        workingGoal.targetTransitionHeight = scaledGoalHeight;
                        workingGoal.preTransitionHeight = workingGoal.lineRenderer.GetPosition(0).y;
                    }
                    _currentTimeResizingGraph = Time.deltaTime;
                }

                foreach (string sound in graphingSounds)
                {
                    AudioManager.Instance.StopSound(sound);
                }
                authorImage.sprite = currentBird.faceSprite;
                // int clampedPointsValue = Mathf.Min(_pointsPerCase[_currentCaseIndex].points,3);
                // float guessXPosition = (startingPosition.x + widthPerSegment * _currentCaseIndex + currentLineRenderer.transform.position.x) * _widthRatio;
                // float guessYPosition = (_heightReached + currentLineRenderer.transform.position.y) * _heightRatio;
                // float guessZPosition = -2;
                // Vector3 guessPosition = new Vector3(guessXPosition,guessYPosition,guessZPosition); 
                // guessContainer.transform.position = Camera.main.WorldToScreenPoint(guessPosition + guessOffsetMap[clampedPointsValue]);
                // guessContainer.transform.eulerAngles = new Vector3(0, 0, guessRotationMap[clampedPointsValue]);

                setCurrentCaseWords(currentBird);
                int soundIndexToPlay = Mathf.Min(_pointsPerCase[_currentCaseIndex].points, 3);
                AudioManager.Instance.PlaySound(graphingSounds[soundIndexToPlay], true);

            }
            else
            {
                foreach (string sound in graphingSounds)
                {
                    AudioManager.Instance.StopSound(sound);
                }
                guessContainer.SetActive(false);
                progressParticleEffect.gameObject.SetActive(false);
                graphingActive = false;
            }
        }

        private void setCurrentCaseWords(Bird currentBird)
        {
            EndgameCaseData currentCaseData = _pointsPerCase[_currentCaseIndex].caseData;
            foreach (KeyValuePair<int, CaseWordData> correctWord in currentCaseData.correctWordsMap)
            {
                if (!wordTextsMap.ContainsKey(correctWord.Key))
                {
                    Debug.LogError("Could not show word[" + correctWord.Value + "] from cabinet[" + correctWord.Key.ToString() + "] in the working goals manager because the index was missing from the wordTextsMap.");
                    continue;
                }
                Text wordText = wordTextsMap[correctWord.Key];
                string guess = currentCaseData.guessesMap.ContainsKey(correctWord.Key) ? currentCaseData.guessesMap[correctWord.Key] : "";

                wordText.text = guess;
                wordText.color = guess == correctWord.Value.value ? new Color(0.25f, 0.75f, 0.25f) : Color.red;
                ParticleSystem.MainModule settings = progressParticleEffect.main;
                settings.startColor = currentBird.colour;
            }
        }

        private int getNumberOfCasesToGoal(Goal goal)
        {
            int numberOfCasesToGoal = 0;
            int cumulativePoints = 0;
            foreach (PlayerPoints pointsGroup in _pointsPerCase)
            {
                cumulativePoints += pointsGroup.points;
                numberOfCasesToGoal++;
                if (cumulativePoints > goal.requiredPoints)
                {
                    return numberOfCasesToGoal;
                }
            }
            return numberOfCasesToGoal;
        }

        private void updateCelebrationState()
        {
            _timeSinceLastConfettiCannonFired += Time.deltaTime;
            if (_timeBetweenConfettiCannonFires < _timeSinceLastConfettiCannonFired)
            {
                _currentConfettiCannonIndex++;
                switch (_currentConfettiCannonIndex)
                {
                    case 1:
                        confettiCannon1.gameObject.SetActive(true);
                        AudioManager.Instance.PlaySound("ConfettiCannon");
                        confettiCannon1.SetTrigger("Fire");
                        break;
                    case 2:
                        confettiCannon2.gameObject.SetActive(true);
                        AudioManager.Instance.PlaySound("ConfettiCannon");
                        confettiCannon2.SetTrigger("Fire");
                        break;
                    case 3:
                        confettiCannon3.gameObject.SetActive(true);
                        AudioManager.Instance.PlaySound("ConfettiCannon");
                        confettiCannon3.SetTrigger("Fire");
                        break;
                    case 4:
                        confettiCannon4.gameObject.SetActive(true);
                        AudioManager.Instance.PlaySound("ConfettiCannon");
                        confettiCannon4.SetTrigger("Fire");
                        break;
                    case 5:
                        celebrationActive = false;
                        active = false;
                        break;
                }
                _timeSinceLastConfettiCannonFired = 0.0f;
            }
        }


        private void updateResolutionState()
        {
            _currentTimeWaitingForEndgameResolution += Time.deltaTime;
            if (_currentTimeWaitingForEndgameResolution > _timeUntilEndgameResolution)
            {
                showFinalResult(getHighestGoalUnderPoints(_currentPoints));
                return;
                if (didWorkersWin)
                {
                    celebrationActive = true;
                    AudioManager.Instance.PlaySound("Celebration");
                    confettiWinEffect.SetActive(true);
                }
                else
                {
                    AudioManager.Instance.PlaySound("Defeat");
                    pinkSlipAnimator.gameObject.SetActive(true);
                    pinkSlipAnimator.SetTrigger("Slide");
                    active = false;
                }
            }
        }

        public void startShowingWorkingGoals(bool inDidWorkersWin)
        {

            initialize();
            AudioManager.Instance.PlaySound("sfx_game_env_boss_rise");
            bossAnimator.SetBool("Slide", true);
            didWorkersWin = inDidWorkersWin;
            guessContainer.SetActive(true);
            _currentCaseIndex = 0;
            foreach (PlayerPoints point in _pointsPerCase)
            {
                _totalPoints += point.points;
            }
            _totalPoints = Mathf.Max(_totalPoints, 1);
            _currentPoints = 0;

            //Determine the goal with the lowest height under current points
            Goal lowestGoal = getLowestGoalOverPoints(_currentPoints);

            if (_pointsPerCase.Count == 0)
            {
                showFinalResult(lowestGoal);
                return;
            }


            _currentWidthPerCase = getCurrentWidthPerCase(lowestGoal);
            _currentHeightPerPoint = getCurrentHeightPerPoint(lowestGoal);

            _nextGoal = getLowestGoalOverPoints(_currentPoints);
            if (_nextGoal == null)
            {
                _nextGoal = getHighestGoalUnderPoints(_currentPoints);
            }
            //Create the objects/visuals for each of the goals
            createGoalVisualsMap(_currentHeightPerPoint);

            //only show the goals currently under the max height
            showGoalsUnderHeight(yAxisLineRenderer.GetPosition(1).y + yAxisLineRenderer.transform.position.y);

            initializeGraphingLine();
            active = true;
        }

        private float getCurrentWidthPerCase(Goal goal)
        {
            int numberOfCases = 0;
            int numberOfPointsReached = 0;
            foreach (PlayerPoints pointsGroup in _pointsPerCase)
            {
                numberOfPointsReached += pointsGroup.points;
                numberOfCases++;
                if (numberOfPointsReached > goal.requiredPoints)
                {
                    break;
                }
            }
            _initialCasesForGraphWidth = numberOfCases;
            float currentWidthPerCase = graphWidth / numberOfCases;
            return currentWidthPerCase;
        }

        private float getCurrentHeightPerPoint(Goal goal)
        {
            float currentHeightPerPoint = Mathf.Max(graphHeight / goal.requiredPoints, _minimumHeightPerSegment);
            _initialPointsForGraphHeight = goal.requiredPoints;
            return currentHeightPerPoint;
        }

        private void createGoalVisualsMap(float heightPerPoint)
        {
            _activeWorkingGoalsMap.Clear();
            foreach (Goal goal in _goals)
            {
                //Instantiate a threshold line at the corresponding height
                GameObject lineObject = Instantiate(thresholdLinePrefab, new Vector3(0f, 0f, 0), Quaternion.identity, thresholdLineHolderTransform);
                lineObject.transform.localPosition = Vector3.zero;
                //lineObject.transform.localPosition = new Vector3(-644.76f,-362.47f,0f);
                LineRenderer thresholdLine = lineObject.GetComponent<LineRenderer>();

                thresholdLine.SetPosition(0, new Vector3(startingPosition.x, startingPosition.y + heightPerPoint * goal.requiredPoints, -2));
                thresholdLine.SetPosition(1, new Vector3(startingPosition.x + graphWidth, startingPosition.y + heightPerPoint * goal.requiredPoints, -2));


                GameObject selectedPrefab;

                //Instantiate the goal image at the corresponding height
                switch (goal.type)
                {
                    case GoalType.employee_review:
                        selectedPrefab = evaluateVisualPrefab;
                        break;
                    case GoalType.extra_vote:
                        selectedPrefab = extraVoteVisualPrefab;
                        break;
                    case GoalType.worker_win:
                        selectedPrefab = workerWinVisualPrefab;
                        break;
                    case GoalType.endgame_result_state:
                        selectedPrefab = endlessModeResultVisualPrefab;
                        break;
                    default:
                        continue;
                }
                Vector3 screenPoint = Camera.main.WorldToScreenPoint(new Vector3(startingPosition.x, startingPosition.y + heightPerPoint * goal.requiredPoints, -2));
                GameObject newObject = Instantiate(selectedPrefab, Vector3.zero, Quaternion.identity, resizableObjectsHolder.transform);
                newObject.transform.localPosition = screenPoint;
                lineObject.SetActive(false);
                WorkingGoal newWorkingGoal = new WorkingGoal();
                newWorkingGoal.goalObject = lineObject;
                newWorkingGoal.lineRenderer = thresholdLine;
                newWorkingGoal.pointThreshold = goal.requiredPoints;
                newWorkingGoal.preTransitionHeight = newWorkingGoal.goalObject.transform.position.y;

                _activeWorkingGoalsMap.Add(goal.name, newWorkingGoal);

                if (goal.requiredPoints == 0)
                {
                    ThresholdLineLabel thresholdLineLabel = lineObject.gameObject.GetComponentInChildren<ThresholdLineLabel>();
                    if (thresholdLineLabel != null)
                    {
                        thresholdLineLabel.gameObject.SetActive(false);
                    }
                }
                else
                {
                    SettingsManager.EndgameResult matchingResult = null;
                    //Get the matching result
                    foreach (SettingsManager.EndgameResult result in SettingsManager.Instance.resultPossibilities)
                    {
                        if (result.resultName == goal.name)
                        {
                            matchingResult = result;
                            break;
                        }
                    }
                    if (matchingResult != null)
                    {
                        ThresholdLineLabel thresholdLineLabel = lineObject.gameObject.GetComponentInChildren<ThresholdLineLabel>();
                        if (thresholdLineLabel != null)
                        {
                            thresholdLineLabel.SetLabelText(matchingResult.resultName, matchingResult.goalTextColour);
                            //Set values on the resulting object
                            thresholdLine.material = matchingResult.lineMaterial;
                            thresholdLineLabel.gameObject.SetActive(true);
                            thresholdLineLabel.SetLineRenderer(thresholdLine);
                        }
                        else
                        {
                            Debug.LogError("Threshold line label was null for goal[" + goal.name + "]");
                        }

                    }
                    else
                    {
                        Debug.LogError("Could not find matching result for goal[" + goal.name + "].");
                    }
                }
            }
        }

        private Goal getHighestGoalUnderPoints(int points)
        {
            Goal highestGoal = null;

            foreach (Goal goal in _goals)
            {
                if (highestGoal == null && goal.requiredPoints <= points)
                {
                    highestGoal = goal;
                }
                else if (goal.requiredPoints <= points && goal.requiredPoints > highestGoal.requiredPoints)
                {
                    highestGoal = goal;
                }
            }

            return highestGoal;
        }

        private Goal getLowestGoalOverPoints(int points)
        {
            Goal lowestGoal = null;

            foreach (Goal goal in _goals)
            {
                if (lowestGoal == null && goal.requiredPoints > points)
                {
                    lowestGoal = goal;
                }
                else if (goal.requiredPoints > points && goal.requiredPoints < lowestGoal.requiredPoints)
                {
                    lowestGoal = goal;
                }
            }

            return lowestGoal;
        }

        private void showGoalsUnderHeight(float height)
        {
            foreach (KeyValuePair<string, WorkingGoal> activeGoal in _activeWorkingGoalsMap)
            {
                if (!activeGoal.Value.goalObject.activeSelf)
                {
                    float goalLineHeight = activeGoal.Value.lineRenderer.GetPosition(0).y + activeGoal.Value.lineRenderer.transform.position.y;
                    //Debug.LogError("Comparing goal["+activeGoal.Key+"] height["+goalLineHeight.ToString()+"] against height["+height.ToString()+"].");
                    if (goalLineHeight < height)
                    {
                        //Debug.LogError("Showing goal["+activeGoal.Key+"].");
                        activeGoal.Value.goalObject.SetActive(true);
                    }
                }

            }
        }

        private void showGoalsUnderPointThreshold(int maximumPointThreshold)
        {
            foreach (KeyValuePair<string, WorkingGoal> activeGoal in _activeWorkingGoalsMap)
            {
                if (activeGoal.Value.pointThreshold < maximumPointThreshold)
                {
                    activeGoal.Value.goalObject.SetActive(true);
                }
            }
        }

        private void initializeGraphingLine()
        {
            _timeShowingPoint = 0.0f;
            Bird currentBird = ColourManager.Instance.birdMap[_pointsPerCase[0].player];
            currentLineRenderer = Instantiate(linePrefab, resizableObjectsHolder.transform).GetComponent<LineRenderer>();
            currentLineRenderer.material = currentBird.material;
            currentLineRenderer.SetPosition(0, startingPosition);

            authorImage.sprite = currentBird.faceSprite;

            ParticleSystem.MainModule settings = progressParticleEffect.main;
            settings.startColor = currentBird.colour;
            int clampedPointsValue = Mathf.Min(_pointsPerCase[0].points, 3);
            float guessXPosition = (startingPosition.x + _currentWidthPerCase * _currentCaseIndex + currentLineRenderer.transform.position.x) * _widthRatio;
            float guessYPosition = (_heightReached + currentLineRenderer.transform.position.y) * _heightRatio;
            float guessZPosition = -2;
            Vector3 guessPosition = new Vector3(guessXPosition, guessYPosition, guessZPosition);
            //guessContainer.transform.position = Camera.main.WorldToScreenPoint(guessPosition + guessOffsetMap[clampedPointsValue]);
            //guessContainer.transform.eulerAngles = new Vector3(0, 0, guessRotationMap[clampedPointsValue]);
            EndgameCaseData currentCaseData = _pointsPerCase[0].caseData;
            foreach (KeyValuePair<int, CaseWordData> correctWord in currentCaseData.correctWordsMap)
            {
                if (!wordTextsMap.ContainsKey(correctWord.Key))
                {
                    Debug.LogError("Could not show word[" + correctWord.Value + "] from cabinet[" + correctWord.Key.ToString() + "] in the working goals manager because the index was missing from the wordTextsMap.");
                    continue;
                }
                Text wordText = wordTextsMap[correctWord.Key];
                string guess = currentCaseData.guessesMap[correctWord.Key];

                wordText.text = guess;
                wordText.color = guess == correctWord.Value.value ? new Color(0.25f, 0.75f, 0.25f) : Color.red;
            }
            _heightReached = startingPosition.y;
            int soundIndexToPlay = Mathf.Clamp(_pointsPerCase[0].points, 0, 3);
            AudioManager.Instance.PlaySound(graphingSounds[soundIndexToPlay], true);
            progressParticleEffect.gameObject.SetActive(true);
        }

        private void showFinalResult(Goal goal)
        {
            //Get the matching result
            SettingsManager.EndgameResult matchingResult = null;
            foreach (SettingsManager.EndgameResult result in SettingsManager.Instance.resultPossibilities)
            {
                if (result.resultName == goal.name)
                {
                    matchingResult = result;
                }
            }

            if (matchingResult == null)
            {
                Debug.LogError("Could not show endless mode result, could not match a result to the provided goal[" + goal.name + "].");
            }
            //endlessModeResultForm.bossReactionImage.sprite = matchingResult.bossFaceReaction;
            //endlessModeResultForm.formImage.color = matchingResult.sheetColour;
            endlessModeResultForm.resultMessageText.text = matchingResult.bossMessage;
            endlessModeResultForm.resultNameText.color = matchingResult.resultTextColour;
            endlessModeResultForm.resultNameText.text = matchingResult.resultName;
            //AudioManager.Instance.PlaySound(matchingResult.sfxToPlay);
            endlessModeResultForm.gameObject.SetActive(true);
            AudioManager.Instance.PlaySound("sfx_game_env_boss_lower");
            bossAnimator.SetBool("Slide", false);
            finalResultManager.chosenReactionState = matchingResult.finalFaceState;
            finalResultManager.responseSoundClip = matchingResult.sfxToPlay;
            finalResultManager.Play();
            enabled = false;
            //endlessModeResultForm.formAnimator.SetTrigger("Slide");
            //active = false;
        }

        public class WorkingGoal
        {
            public GameObject goalObject;
            public float preTransitionHeight;
            public float targetTransitionHeight;
            public int pointThreshold;
            public LineRenderer lineRenderer;
        }

    }
}