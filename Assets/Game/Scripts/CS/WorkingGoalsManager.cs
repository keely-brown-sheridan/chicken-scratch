using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ChickenScratch.ColourManager;

namespace ChickenScratch
{
    public class WorkingGoalsManager : MonoBehaviour
    {
        public bool active = false;
        public bool graphingActive = true;
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

        public GameObject resultVisualPrefab;
        public GameObject guessContainer;
        public List<Text> allWordTexts = new List<Text>();
        public Dictionary<int, Text> wordTextsMap = new Dictionary<int, Text>();
        public Image authorImage;

        
        [SerializeField] private ParticleSystem progressParticleEffect;
        [SerializeField] private Transform thresholdLineHolderTransform;
        [SerializeField]
        private LineRenderer yAxisLineRenderer;

        private Animator bossAnimator;

        public GameObject resizableObjectsHolder;
        private Dictionary<int, Vector3> guessOffsetMap = new Dictionary<int, Vector3>();
        private Dictionary<int, float> guessRotationMap = new Dictionary<int, float>();

        private LineRenderer currentLineRenderer;
        private float _timeShowingPoint = 0f;
        private float _heightReached = 0f;
        private int _currentCaseIndex = 0;
        private List<GoalData> _goals = new List<GoalData>();
        private List<PlayerPointsData> _pointsPerCase = new List<PlayerPointsData>();
        private bool _isInitialized = false;
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
        private GoalData _nextGoal;
        private int _initialPointsForGraphHeight = 0;
        private int _initialCasesForGraphWidth = 0;
        private Dictionary<string, WorkingGoal> _activeWorkingGoalsMap = new Dictionary<string, WorkingGoal>();
        private int _pointsRequiredForNextTransition = 0;
        private float _widthRatio = 1.0f;
        private float _heightRatio = 1.0f;

        public void Start()
        {
            initialize();
            test();
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
            GameDataManager.Instance.RefreshWords(new List<CaseWordData>());
            _goals = new List<GoalData>();
            GoalData newGoal = new GoalData(10);
            newGoal.name = "Pizza Party";
            _goals.Add(newGoal);
            _pointsPerCase = new List<PlayerPointsData>();
            newGoal = new GoalData(300);
            newGoal.name = "Up for Promotion";
            _goals.Add(newGoal);
            _pointsPerCase = new List<PlayerPointsData>();
            EndgameCaseData test = new EndgameCaseData();
            test.correctWordIdentifierMap.Add(1, "prefixes-NEUTRAL-ATTACHED");
            test.correctWordIdentifierMap.Add(2, "nouns-ANIMAL-AARDVARK");
            test.guessData.prefix = "bot";
            test.guessData.noun = "potato";
            _pointsPerCase.Add(new PlayerPointsData(BirdName.red, 8, test));
            test = new EndgameCaseData();
            test.correctWordIdentifierMap.Add(1, "prefixes-NEUTRAL-ATTACHED");
            test.correctWordIdentifierMap.Add(2, "nouns-ANIMAL-AARDVARK");
            test.guessData.prefix = "ATTACHED";
            test.guessData.noun = "AARDVARK";
            _pointsPerCase.Add(new PlayerPointsData(BirdName.green, 4, test));
            test = new EndgameCaseData();
            test.correctWordIdentifierMap.Add(1, "prefixes-NEUTRAL-ATTACHED");
            test.correctWordIdentifierMap.Add(2, "nouns-ANIMAL-AARDVARK");
            test.guessData.prefix = "ATTACHED";
            test.guessData.noun = "totato";
            _pointsPerCase.Add(new PlayerPointsData(BirdName.blue, 2, test));
            test = new EndgameCaseData();
            test.correctWordIdentifierMap.Add(1, "prefixes-NEUTRAL-ATTACHED");
            test.correctWordIdentifierMap.Add(2, "nouns-ANIMAL-AARDVARK");
            test.guessData.prefix = "hot";
            test.guessData.noun = "AARDVARK";
            _pointsPerCase.Add(new PlayerPointsData(BirdName.orange, 25, test));
            _pointsPerCase.Add(new PlayerPointsData(BirdName.orange, 25, test));
            _pointsPerCase.Add(new PlayerPointsData(BirdName.orange, 25, test));
            _pointsPerCase.Add(new PlayerPointsData(BirdName.orange, 25, test));

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
                    updateResolutionState();
                }
            }
        }

        public void initializeWorkingGoals(List<GoalData> goals, List<PlayerPointsData> playerPoints)
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
                CaseWordData correctPrefix = GameDataManager.Instance.GetWord(_pointsPerCase[_currentCaseIndex].caseData.correctWordIdentifierMap[1]);
                if (correctPrefix.value == _pointsPerCase[_currentCaseIndex].caseData.guessData.prefix)
                {
                    numberOfCorrectWords++;
                }
                CaseWordData correctNoun = GameDataManager.Instance.GetWord(_pointsPerCase[_currentCaseIndex].caseData.correctWordIdentifierMap[2]);
                if (correctNoun.value == _pointsPerCase[_currentCaseIndex].caseData.guessData.noun)
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
            foreach (KeyValuePair<int, string> correctWordIdentifier in currentCaseData.correctWordIdentifierMap)
            {
                CaseWordData correctWord = GameDataManager.Instance.GetWord(correctWordIdentifier.Value);
                if (!wordTextsMap.ContainsKey(correctWordIdentifier.Key))
                {
                    Debug.LogError("Could not show word[" + correctWordIdentifier.Value + "] from cabinet[" + correctWordIdentifier.Key.ToString() + "] in the working goals manager because the index was missing from the wordTextsMap.");
                    continue;
                }
                Text wordText = wordTextsMap[correctWordIdentifier.Key];
                string guess = correctWordIdentifier.Key == 1 ? currentCaseData.guessData.prefix : currentCaseData.guessData.noun;

                wordText.text = guess;
                wordText.color = guess == correctWord.value ? new Color(0.25f, 0.75f, 0.25f) : Color.red;
                ParticleSystem.MainModule settings = progressParticleEffect.main;
                settings.startColor = currentBird.colour;
            }
        }

        private int getNumberOfCasesToGoal(GoalData goal)
        {
            int numberOfCasesToGoal = 0;
            int cumulativePoints = 0;
            foreach (PlayerPointsData pointsGroup in _pointsPerCase)
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

        

        private void updateResolutionState()
        {
            _currentTimeWaitingForEndgameResolution += Time.deltaTime;
            if (_currentTimeWaitingForEndgameResolution > _timeUntilEndgameResolution)
            {
                showFinalResult(getHighestGoalUnderPoints(_currentPoints));
                return;
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
            foreach (PlayerPointsData point in _pointsPerCase)
            {
                _totalPoints += point.points;
            }
            _totalPoints = Mathf.Max(_totalPoints, 1);
            _currentPoints = 0;

            //Determine the goal with the lowest height under current points
            GoalData lowestGoal = getLowestGoalOverPoints(_currentPoints);

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

        private float getCurrentWidthPerCase(GoalData goal)
        {
            int numberOfCases = 0;
            int numberOfPointsReached = 0;
            foreach (PlayerPointsData pointsGroup in _pointsPerCase)
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

        private float getCurrentHeightPerPoint(GoalData goal)
        {
            float currentHeightPerPoint = Mathf.Max(graphHeight / goal.requiredPoints, _minimumHeightPerSegment);
            _initialPointsForGraphHeight = goal.requiredPoints;
            return currentHeightPerPoint;
        }

        private void createGoalVisualsMap(float heightPerPoint)
        {
            _activeWorkingGoalsMap.Clear();
            foreach (GoalData goal in _goals)
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
                selectedPrefab = resultVisualPrefab;

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
                    ResultData matchingResult = null;
                    //Get the matching result
                    foreach (ResultData result in SettingsManager.Instance.resultPossibilities)
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

        private GoalData getHighestGoalUnderPoints(int points)
        {
            GoalData highestGoal = null;

            foreach (GoalData goal in _goals)
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

        private GoalData getLowestGoalOverPoints(int points)
        {
            GoalData lowestGoal = null;

            foreach (GoalData goal in _goals)
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
            if(lowestGoal == null)
            {
                //Then we need to choose the highest goal because we've broken the record :O
                foreach (GoalData goal in _goals)
                {
                    if (lowestGoal == null)
                    {
                        lowestGoal = goal;
                    }
                    else if (goal.requiredPoints > lowestGoal.requiredPoints)
                    {
                        lowestGoal = goal;
                    }
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
            foreach (KeyValuePair<int, string> correctWordIdentifier in currentCaseData.correctWordIdentifierMap)
            {
                CaseWordData correctWord = GameDataManager.Instance.GetWord(correctWordIdentifier.Value);
                if (!wordTextsMap.ContainsKey(correctWordIdentifier.Key))
                {
                    Debug.LogError("Could not show word[" + correctWordIdentifier.Value + "] from cabinet[" + correctWordIdentifier.Key.ToString() + "] in the working goals manager because the index was missing from the wordTextsMap.");
                    continue;
                }
                Text wordText = wordTextsMap[correctWordIdentifier.Key];
                string guess = correctWordIdentifier.Key == 1 ? currentCaseData.guessData.prefix : currentCaseData.guessData.noun;

                wordText.text = guess;
                wordText.color = guess == correctWord.value ? new Color(0.25f, 0.75f, 0.25f) : Color.red;
            }
            _heightReached = startingPosition.y;
            int soundIndexToPlay = Mathf.Clamp(_pointsPerCase[0].points, 0, 3);
            AudioManager.Instance.PlaySound(graphingSounds[soundIndexToPlay], true);
            progressParticleEffect.gameObject.SetActive(true);
        }

        private void showFinalResult(GoalData goal)
        {
            //Get the matching result
            
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