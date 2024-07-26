using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class CaseEmail : MonoBehaviour
    {
        public enum CaseEmailTaskType
        {
            drawing, prompt, guess, original, invalid
        }
        [SerializeField]
        private List<GameObject> taskEmailSectionPrefabs;

        [SerializeField]
        private Transform leftSpawnObject, middleSpawnObject, rightSpawnObject;

        [SerializeField]
        private GameObject shiftLeftButtonObject, shiftRightButtonObject;

        private GameObject currentLeftSectionObject, currentMiddleSectionObject, currentRightSectionObject;

        private Dictionary<CaseEmailTaskType, GameObject> taskEmailSectionPrefabMap = new Dictionary<CaseEmailTaskType, GameObject>();


        private GuessData guessData = new GuessData();
        private Dictionary<int, string> correctWordIdentifiersMap = new Dictionary<int, string>();
        private List<EndgameTaskData> caseTasks = new List<EndgameTaskData>();
        private bool isInitialized = false;
        private int currentTaskIndex = 0;

        public void initialize(EndgameCaseData caseData)
        {
            if (!isInitialized)
            {
                foreach (GameObject taskEmailSectionPrefab in taskEmailSectionPrefabs)
                {
                    CaseEmailSection caseEmailSection = taskEmailSectionPrefab.GetComponent<CaseEmailSection>();
                    taskEmailSectionPrefabMap.Add(caseEmailSection.taskType, taskEmailSectionPrefab);
                }
                isInitialized = true;
            }

            guessData = caseData.guessData;
            correctWordIdentifiersMap = caseData.correctWordIdentifierMap;
            caseTasks = caseData.taskDataMap.Values.ToList();
            UpdateCaseEmailSections();
            
        }

        private void UpdateCaseEmailSections()
        {
            if(currentLeftSectionObject != null)
            {
                Destroy(currentLeftSectionObject);
            }
            if(currentRightSectionObject != null)
            {
                Destroy(currentRightSectionObject);
            }
            if(currentMiddleSectionObject != null)
            {
                Destroy(currentMiddleSectionObject);
            }
            if(currentTaskIndex > 0)
            {
                //Create left case
                currentLeftSectionObject = CreateCaseEmailSection(caseTasks[currentTaskIndex - 1], leftSpawnObject, 0.6f);
            }
            else if(currentTaskIndex == 0)
            {
                //The original prompt is not based on a task
                currentLeftSectionObject = CreateOriginalCaseEmailSection(leftSpawnObject);
            }

            if(currentTaskIndex < caseTasks.Count - 1)
            {
                //Create right case
                currentRightSectionObject = CreateCaseEmailSection(caseTasks[currentTaskIndex + 1], rightSpawnObject, 0.6f);
            }

            if(currentTaskIndex > -1)
            {
                currentMiddleSectionObject = CreateCaseEmailSection(caseTasks[currentTaskIndex], middleSpawnObject, 1f);
            }
            else if(currentTaskIndex == -1)
            {
                //The original prompt is not based on a task
                currentMiddleSectionObject = CreateOriginalCaseEmailSection(middleSpawnObject);
            }
            
        }

        private GameObject CreateOriginalCaseEmailSection(Transform emailSectionParent)
        {
            GameObject originalCaseEmailSectionObject = Instantiate(taskEmailSectionPrefabMap[CaseEmailTaskType.original], emailSectionParent);
            OriginalCaseEmailSection originalCaseEmailSection = originalCaseEmailSectionObject.GetComponent<OriginalCaseEmailSection>();
            CaseWordData correctPrefix = GameDataManager.Instance.GetWord(correctWordIdentifiersMap[1]);
            CaseWordData correctNoun = GameDataManager.Instance.GetWord(correctWordIdentifiersMap[2]);
            originalCaseEmailSection.Initialize(correctPrefix.value, correctNoun.value);
            return originalCaseEmailSectionObject;
        }


        private GameObject CreateCaseEmailSection(EndgameTaskData taskData, Transform emailSectionParent, float drawingRatio)
        {
            switch (taskData.taskType)
            {
                case TaskData.TaskType.base_drawing:
                case TaskData.TaskType.copy_drawing:
                case TaskData.TaskType.add_drawing:
                case TaskData.TaskType.prompt_drawing:
                case TaskData.TaskType.compile_drawing:
                case TaskData.TaskType.blender_drawing:
                    GameObject drawingCaseEmailSectionObject = Instantiate(taskEmailSectionPrefabMap[CaseEmailTaskType.drawing], emailSectionParent);
                    DrawingCaseEmailSection drawingCaseEmailSection = drawingCaseEmailSectionObject.GetComponent<DrawingCaseEmailSection>();
                    drawingCaseEmailSection.Initialize(taskData.drawingData, taskData.ratingData, drawingRatio);
                    return drawingCaseEmailSectionObject;
                case TaskData.TaskType.prompting:
                    GameObject promptCaseEmailSectionObject = Instantiate(taskEmailSectionPrefabMap[CaseEmailTaskType.prompt], emailSectionParent);
                    PromptCaseEmailSection promptCaseEmailSection = promptCaseEmailSectionObject.GetComponent<PromptCaseEmailSection>();
                    promptCaseEmailSection.Initialize(taskData.promptData, taskData.ratingData);
                    return promptCaseEmailSectionObject;
                case TaskData.TaskType.morph_guessing:
                case TaskData.TaskType.base_guessing:
                case TaskData.TaskType.competition_guessing:
                    GameObject guessCaseEmailSectionObject = Instantiate(taskEmailSectionPrefabMap[CaseEmailTaskType.guess], emailSectionParent);
                    GuessCaseEmailSection guessCaseEmailSection = guessCaseEmailSectionObject.GetComponent<GuessCaseEmailSection>();
                    guessCaseEmailSection.Initialize(correctWordIdentifiersMap, guessData, taskData.ratingData);
                    return guessCaseEmailSectionObject;

            }
            return null;
        }

        public void ShiftTasksLeft()
        {
            AudioManager.Instance.PlaySoundVariant("sfx_vote_int_gen_click_owl");
            currentTaskIndex--;
            
            if (currentTaskIndex < -1)
            {
                currentTaskIndex = -1;
            }
            else
            {
                shiftRightButtonObject.SetActive(true);
                shiftLeftButtonObject.SetActive(currentTaskIndex != -1);
                UpdateCaseEmailSections();
            }
            
        }

        public void ShiftTasksRight()
        {
            AudioManager.Instance.PlaySoundVariant("sfx_vote_int_gen_click_owl");
            currentTaskIndex++;
            
            if(currentTaskIndex >= caseTasks.Count)
            {
                currentTaskIndex = caseTasks.Count - 1;
            }
            else
            {
                shiftLeftButtonObject.SetActive(true);
                shiftRightButtonObject.SetActive(currentTaskIndex != caseTasks.Count - 1);
                UpdateCaseEmailSections();
            }
            
        }

        //public void setLikeReviewForWord(int wordIndex)
        //{
        //    AnalyticsEvent.Custom("Rating", new Dictionary<string, object>() { { "Like", wordTextsMap[wordIndex].text } });

        //    disableButton(prefixDislikeObject);
        //    disableButton(prefixLikeObject);

        //    AudioManager.Instance.PlaySound("Like");
        //}

        //public void setDislikeReviewForWord(int wordIndex)
        //{
        //    AnalyticsEvent.Custom("Rating", new Dictionary<string, object>() { { "Dislike", wordTextsMap[wordIndex].text } });

        //    disableButton(prefixDislikeObject);
        //    disableButton(prefixLikeObject);

        //    AudioManager.Instance.PlaySound("Sus" + UnityEngine.Random.Range(1, 5));
        //}

        public void saveGifOfChain()
        {
            //GameManager.Instance.playerFlowManager.saveGifOfChain();
        }

    }
}