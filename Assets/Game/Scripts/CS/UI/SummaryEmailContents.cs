using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ChickenScratch.ColourManager;

namespace ChickenScratch
{
    public class SummaryEmailContents : MonoBehaviour
    {
        public Text outcomeText;
        public List<GameObject> voteRoundObjects = new List<GameObject>();
        [SerializeField]
        private TimeBarGraph timeBarGraph;
        [SerializeField]
        private PointsPieChart pointsPieChart;
        [SerializeField]
        private Image mostLikedImage;

        private Dictionary<int, GameObject> voteRoundObjectMap = new Dictionary<int, GameObject>();
        private int currentRoundIndex = 1;

        public void Initialize()
        {
            IndexMap currentIndex;
            foreach (GameObject voteRoundObject in voteRoundObjects)
            {
                currentIndex = voteRoundObject.GetComponent<IndexMap>();
                voteRoundObjectMap.Add(currentIndex.index, voteRoundObject);
            }
        }

        public void setSummaryContents()
        {
            mostLikedImage.sprite = ColourManager.Instance.birdMap[PlayerFlowManager.employeeOfTheMonth].faceSprite;
            Dictionary<BirdName, float> pointsMap = new Dictionary<BirdName, float>();
            Dictionary<BirdName, float> timeMap = new Dictionary<BirdName, float>();

            foreach (EndgameCaseData currentCase in GameManager.Instance.playerFlowManager.slidesRound.caseDataMap.Values)
            {
                BirdName guesser = currentCase.GetGuesser();
                if (guesser != BirdName.none)
                {
                    if (!pointsMap.ContainsKey(guesser))
                    {
                        pointsMap.Add(guesser, 0);
                    }
                    pointsMap[guesser] += currentCase.GetTotalPoints();
                }
                foreach(EndgameTaskData currentTask in currentCase.taskDataMap.Values)
                {
                    if(currentTask.assignedPlayer == BirdName.none)
                    {
                        continue;
                    }
                    if (!timeMap.ContainsKey(currentTask.assignedPlayer))
                    {
                        timeMap.Add(currentTask.assignedPlayer, 0.0f);
                    }
                    switch(currentTask.taskType)
                    {
                        case TaskData.TaskType.base_drawing:
                        case TaskData.TaskType.add_drawing:
                        case TaskData.TaskType.compile_drawing:
                        case TaskData.TaskType.copy_drawing:
                        case TaskData.TaskType.prompt_drawing:
                            timeMap[currentTask.assignedPlayer] += currentTask.drawingData.timeTaken;
                            break;
                        case TaskData.TaskType.prompting:
                            timeMap[currentTask.assignedPlayer] += currentTask.promptData.timeTaken;
                            break;
                        case TaskData.TaskType.base_guessing:
                            timeMap[currentTask.assignedPlayer] += currentCase.guessData.timeTaken;
                            break;
                    }
                    //timeMap[currentTask.assignedPlayer] += currentTask.timeTaken;
                }
            }
            timeBarGraph.SetTimeValues(timeMap);
            pointsPieChart.SetPointValues(pointsMap);
        }



        public void setNumberOfVoteRounds(int numberOfVoteRounds)
        {
            if (numberOfVoteRounds == -1 || numberOfVoteRounds == 0)
            {
                return;
            }
            int iterator = 0;
            IndexMap currentIndex;

            voteRoundObjectMap.Clear();
            foreach (GameObject voteRoundObject in voteRoundObjects)
            {
                if (iterator == numberOfVoteRounds)
                {
                    break;
                }
                iterator++;
                currentIndex = voteRoundObject.GetComponent<IndexMap>();
                voteRoundObjectMap.Add(currentIndex.index, voteRoundObject);
            }
        }

        public void shiftPageLeft()
        {

            if (voteRoundObjectMap.ContainsKey(currentRoundIndex))
            {
                AudioManager.Instance.PlaySoundVariant("sfx_vote_int_gen_click_owl");
                voteRoundObjectMap[currentRoundIndex].SetActive(false);
                currentRoundIndex--;
                if (currentRoundIndex <= 0)
                {
                    currentRoundIndex = voteRoundObjectMap.Count;
                }
                voteRoundObjectMap[currentRoundIndex].SetActive(true);
            }
        }

        public void shiftPageRight()
        {
            if (voteRoundObjectMap.ContainsKey(currentRoundIndex))
            {
                AudioManager.Instance.PlaySoundVariant("sfx_vote_int_gen_click_owl");
                voteRoundObjectMap[currentRoundIndex].SetActive(false);
                currentRoundIndex++;
                if (currentRoundIndex > voteRoundObjectMap.Count)
                {
                    currentRoundIndex = 1;
                }
                voteRoundObjectMap[currentRoundIndex].SetActive(true);
            }

        }
    }
}