using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    [System.Serializable]
    public class CaseTemplateData
    {
        public string name;
        public enum CaseFormat
        {
            standard, rush, contribution
        }
        public CaseFormat format = CaseFormat.standard;

        public List<TaskData> queuedTasks = new List<TaskData>();
        public List<CaseWordTemplateData> startingWords = new List<CaseWordTemplateData>();

        public int baseCost;
        public int baseReward;
        public int penalty;

        public float startingScoreModifier;

        public CaseTemplateData()
        {

        }

        public CaseTemplateData(CaseChoiceData choice)
        {
            name = choice.name;
            format = choice.caseFormat;
            baseCost = choice.cost;
            baseReward = choice.reward;
            penalty = choice.penalty;
            startingScoreModifier = choice.startingScoreModifier;

            CreateTasks(choice.numberOfTasks);
        }

        private void CreateTasks(int numberOfTasks)
        {
            float taskDuration;
            //Create the tasks
            switch (format)
            {
                case CaseFormat.standard:
                    queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.base_drawing });
                    if (numberOfTasks > 2)
                    {
                        queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.prompting });
                    }
                    if (numberOfTasks > 3)
                    {
                        queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.prompt_drawing });
                    }
                    if (numberOfTasks > 4)
                    {
                        queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.prompting });
                    }
                    if (numberOfTasks > 5)
                    {
                        queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.prompt_drawing });
                    }
                    queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.base_guessing });

                    foreach (TaskData queuedTask in queuedTasks)
                    {
                        taskDuration = SettingsManager.Instance.gameMode.GetTaskDuration(queuedTask.taskType);
                        queuedTask.duration = taskDuration;
                    }
                    break;
                case CaseFormat.rush:

                    taskDuration = SettingsManager.Instance.gameMode.GetTaskDuration(TaskData.TaskType.base_drawing);
                    queuedTasks.Add(new TaskData() 
                    { 
                        taskType = TaskData.TaskType.base_drawing, 
                        duration = taskDuration
                    });
                    taskDuration = SettingsManager.Instance.gameMode.GetTaskDuration(TaskData.TaskType.copy_drawing);
                    float taskFalloff = SettingsManager.Instance.gameMode.rushTaskFalloff;
                    while (queuedTasks.Count < numberOfTasks -1)
                    {
                        //Decrease the amount of time for the task every time
                        taskDuration *= taskFalloff;
                        queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.copy_drawing, duration = taskDuration });
                    }
                    taskDuration = SettingsManager.Instance.gameMode.GetTaskDuration(TaskData.TaskType.base_guessing);
                    queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.base_guessing, duration = taskDuration });
                    break;
                case CaseFormat.contribution:
                    queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.base_drawing });
                    while (queuedTasks.Count < numberOfTasks - 1)
                    {
                        queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.add_drawing });
                    }
                    
                    foreach (TaskData queuedTask in queuedTasks)
                    {
                        taskDuration = SettingsManager.Instance.gameMode.GetTaskDuration(queuedTask.taskType);
                        //Reduce the time that the player takes for each of these tasks
                        queuedTask.duration = taskDuration * SettingsManager.Instance.gameMode.contributionTaskRatio;
                    }
                    taskDuration = SettingsManager.Instance.gameMode.GetTaskDuration(TaskData.TaskType.base_guessing);
                    queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.base_guessing, duration = taskDuration });
                    break;
            }

            
        }
    }
}
