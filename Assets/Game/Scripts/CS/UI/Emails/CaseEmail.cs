using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class CaseEmail : MonoBehaviour
    {
        [SerializeField]
        private TMPro.TMP_Text originalPromptText;

        [SerializeField]
        private Transform emailSectionParent;

        public enum CaseEmailTaskType
        {
            drawing, prompt, guess, invalid
        }
        [SerializeField]
        private List<GameObject> taskEmailSectionPrefabs;

        private Dictionary<CaseEmailTaskType, GameObject> taskEmailSectionPrefabMap = new Dictionary<CaseEmailTaskType, GameObject>();
        private List<GameObject> taskEmailSectionObjects;

        private bool isInitialized = false;

        public void initialize(EndgameCaseData caseData)
        {
            if (isInitialized) return;
            foreach(GameObject taskEmailSectionPrefab in taskEmailSectionPrefabs)
            {
                CaseEmailSection caseEmailSection = taskEmailSectionPrefab.GetComponent<CaseEmailSection>();
                taskEmailSectionPrefabMap.Add(caseEmailSection.taskType, taskEmailSectionPrefab);
            }
            originalPromptText.text = caseData.correctPrompt;
            foreach (KeyValuePair<int, EndgameTaskData> taskData in caseData.taskDataMap)
            {
                switch (taskData.Value.taskType)
                {
                    case TaskData.TaskType.base_drawing:
                    case TaskData.TaskType.copy_drawing:
                    case TaskData.TaskType.add_drawing:
                    case TaskData.TaskType.prompt_drawing:
                        GameObject drawingCaseEmailSectionObject = Instantiate(taskEmailSectionPrefabMap[CaseEmailTaskType.drawing], emailSectionParent);
                        DrawingCaseEmailSection drawingCaseEmailSection = drawingCaseEmailSectionObject.GetComponent<DrawingCaseEmailSection>();
                        drawingCaseEmailSection.Initialize(taskData.Value.drawingData, taskData.Value.ratingData);
                        break;
                    case TaskData.TaskType.prompting:
                        GameObject promptCaseEmailSectionObject = Instantiate(taskEmailSectionPrefabMap[CaseEmailTaskType.prompt], emailSectionParent);
                        PromptCaseEmailSection promptCaseEmailSection = promptCaseEmailSectionObject.GetComponent<PromptCaseEmailSection>();
                        promptCaseEmailSection.Initialize(taskData.Value.promptData, taskData.Value.ratingData);
                        break;
                    case TaskData.TaskType.base_guessing:
                        GameObject guessCaseEmailSectionObject = Instantiate(taskEmailSectionPrefabMap[CaseEmailTaskType.guess], emailSectionParent);
                        GuessCaseEmailSection guessCaseEmailSection = guessCaseEmailSectionObject.GetComponent<GuessCaseEmailSection>();
                        guessCaseEmailSection.Initialize(caseData.correctWordsMap, caseData.guessesMap, taskData.Value.assignedPlayer, taskData.Value.ratingData);
                        break;
                }
            }
            isInitialized = true;
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