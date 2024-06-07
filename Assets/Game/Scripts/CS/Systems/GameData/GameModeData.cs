using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    [System.Serializable]
    public class GameModeData
    {
        public string name;
        public enum CaseDeliveryMode
        {
            queue, free_for_all
        }
        public enum WordDistributionMode
        {
            random
        }

        public int minimumNumberOfPlayers = 2;

        public WordDistributionMode wordDistributionMode = WordDistributionMode.random;
        public CaseDeliveryMode caseDeliveryMode = CaseDeliveryMode.queue;

        public CaseTemplateData baseTemplateData;
        public List<CaseChoiceData> pileCaseChoices = new List<CaseChoiceData>();
        public List<TaskTimingData> taskTimingData;
        public float scoreModifierDecrement;
        public int numberOfCases;
        private Dictionary<TaskData.TaskType, float> taskTimingMap = new Dictionary<TaskData.TaskType, float>();
        public float rushTaskFalloff;
        public float contributionTaskRatio;

        public float GetTaskDuration(TaskData.TaskType taskType)
        {
            if(taskTimingMap.Count == 0)
            {
                foreach(TaskTimingData taskTiming in taskTimingData)
                {
                    if(!taskTimingMap.ContainsKey(taskTiming.taskType))
                    {
                        taskTimingMap.Add(taskTiming.taskType, taskTiming.duration);
                    }
                }
            }

            if(taskTimingMap.ContainsKey(taskType))
            {
                return taskTimingMap[taskType];
            }
            Debug.LogError("Task timing map does not contain task type: " + taskType.ToString());
            return 0f;
        }


        public string description;
        public int goalPointsPerCharacter;
        public float baseTimeInDrawingRound = 120;
        public float totalGameTime;

    }
}