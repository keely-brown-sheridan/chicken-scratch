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
            standard, rush, contribution, shrunk, thirds, top_bottom, corners, curveball, blind
        }
        public CaseFormat format = CaseFormat.standard;

        public List<TaskData> queuedTasks = new List<TaskData>();
        public List<string> startingWordIdentifiers = new List<string>();

        public int baseCost;
        public int basePointsPerCorrectWord;
        public int baseBonusPoints;
        public int penalty;
        public string caseTypeName;
        public Color caseTypeColour;
        public float taskFalloff;

        public float startingScoreModifier;

        public CaseTemplateData()
        {

        }

        public CaseTemplateData(CaseChoiceData choice)
        {
            name = choice.name;
            format = choice.caseFormat;
            baseCost = choice.cost;
            basePointsPerCorrectWord = choice.pointsPerCorrectWord;
            baseBonusPoints = choice.bonusPoints;
            taskFalloff = choice.taskFalloff;
            penalty = choice.penalty;
            startingScoreModifier = choice.startingScoreModifier;
            caseTypeName = choice.identifier;
            caseTypeColour = choice.colour;

            CreateTasks(choice);
        }

        private void CreateTasks(CaseChoiceData choice)
        {
            float taskDuration;
            //Create the tasks
            switch (format)
            {
                case CaseFormat.standard:
                    queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.base_drawing });
                    if (choice.numberOfTasks > 2)
                    {
                        queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.prompting });
                    }
                    if (choice.numberOfTasks > 3)
                    {
                        queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.prompt_drawing });
                    }
                    if (choice.numberOfTasks > 4)
                    {
                        queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.prompting });
                    }
                    if (choice.numberOfTasks > 5)
                    {
                        queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.prompt_drawing });
                    }
                    queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.base_guessing });

                    foreach (TaskData queuedTask in queuedTasks)
                    {
                        taskDuration = choice.GetTaskTiming(queuedTask.taskType);
                        queuedTask.duration = taskDuration;
                    }
                    break;
                case CaseFormat.rush:

                    taskDuration = choice.GetTaskTiming(TaskData.TaskType.base_drawing);
                    queuedTasks.Add(new TaskData() 
                    { 
                        taskType = TaskData.TaskType.base_drawing, 
                        duration = taskDuration
                    });
                    taskDuration = choice.GetTaskTiming(TaskData.TaskType.copy_drawing);
                    while (queuedTasks.Count < choice.numberOfTasks - 1)
                    {
                        //Decrease the amount of time for the task every time
                        taskDuration *= taskFalloff;
                        queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.copy_drawing, duration = taskDuration });
                    }
                    taskDuration = choice.GetTaskTiming(TaskData.TaskType.base_guessing);
                    queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.base_guessing, duration = taskDuration });
                    break;
                case CaseFormat.contribution:
                    queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.base_drawing });
                    while (queuedTasks.Count < choice.numberOfTasks - 1)
                    {
                        queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.add_drawing });
                    }
                    
                    foreach (TaskData queuedTask in queuedTasks)
                    {
                        taskDuration = choice.GetTaskTiming(queuedTask.taskType);
                        //Reduce the time that the player takes for each of these tasks
                        queuedTask.duration = taskDuration * SettingsManager.Instance.gameMode.contributionTaskRatio;
                    }
                    taskDuration = choice.GetTaskTiming(TaskData.TaskType.base_guessing);
                    queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.base_guessing, duration = taskDuration });
                    break;
                case CaseFormat.curveball:
                    queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.base_drawing, modifiers = new List<TaskData.TaskModifier>() { TaskData.TaskModifier.hidden_prefix } });
                    queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.add_drawing } );

                    foreach (TaskData queuedTask in queuedTasks)
                    {
                        taskDuration = choice.GetTaskTiming(queuedTask.taskType);
                        //Reduce the time that the player takes for each of these tasks
                        queuedTask.duration = taskDuration * SettingsManager.Instance.gameMode.contributionTaskRatio;
                    }
                    taskDuration = choice.GetTaskTiming(TaskData.TaskType.base_guessing);
                    queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.base_guessing, duration = taskDuration });
                    break;
                case CaseFormat.shrunk:
                    List<TaskData.TaskModifier> shrunkModifier = new List<TaskData.TaskModifier>() { TaskData.TaskModifier.shrunk };
                    queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.base_drawing, modifiers = shrunkModifier } );
                    if (choice.numberOfTasks > 2)
                    {
                        queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.prompting });
                    }
                    if (choice.numberOfTasks > 3)
                    {
                        queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.prompt_drawing, modifiers = shrunkModifier });
                    }
                    if (choice.numberOfTasks > 4)
                    {
                        queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.prompting });
                    }
                    if (choice.numberOfTasks > 5)
                    {
                        queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.prompt_drawing, modifiers = shrunkModifier });
                    }
                    queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.base_guessing });

                    foreach (TaskData queuedTask in queuedTasks)
                    {
                        taskDuration = choice.GetTaskTiming(queuedTask.taskType);
                        queuedTask.duration = taskDuration;
                    }
                    break;
                case CaseFormat.blind:
                    List<TaskData.TaskModifier> blindModifier = new List<TaskData.TaskModifier>() { TaskData.TaskModifier.blind };
                    queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.base_drawing, modifiers = blindModifier });
                    if (choice.numberOfTasks > 2)
                    {
                        queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.prompting });
                    }
                    if (choice.numberOfTasks > 3)
                    {
                        queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.prompt_drawing, modifiers = blindModifier });
                    }
                    if (choice.numberOfTasks > 4)
                    {
                        queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.prompting });
                    }
                    if (choice.numberOfTasks > 5)
                    {
                        queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.prompt_drawing, modifiers = blindModifier });
                    }
                    queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.base_guessing });

                    foreach (TaskData queuedTask in queuedTasks)
                    {
                        taskDuration = choice.GetTaskTiming(queuedTask.taskType);
                        queuedTask.duration = taskDuration;
                    }
                    break;
                case CaseFormat.thirds:
                    queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.base_drawing, modifiers = new List<TaskData.TaskModifier>() { TaskData.TaskModifier.thirds_first } });
                    queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.add_drawing, modifiers = new List<TaskData.TaskModifier>() { TaskData.TaskModifier.thirds_second } });
                    queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.add_drawing, modifiers = new List<TaskData.TaskModifier>() { TaskData.TaskModifier.thirds_third } });
                    queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.base_guessing });
                    foreach (TaskData queuedTask in queuedTasks)
                    {
                        taskDuration = choice.GetTaskTiming(queuedTask.taskType);
                        queuedTask.duration = taskDuration;
                    }
                    break;
                case CaseFormat.top_bottom:
                    queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.base_drawing, modifiers = new List<TaskData.TaskModifier>() { TaskData.TaskModifier.top } });
                    queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.add_drawing, modifiers = new List<TaskData.TaskModifier>() { TaskData.TaskModifier.bottom } });
                    queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.base_guessing });
                    foreach (TaskData queuedTask in queuedTasks)
                    {
                        taskDuration = choice.GetTaskTiming(queuedTask.taskType);
                        queuedTask.duration = taskDuration;
                    }
                    break;
                case CaseFormat.corners:
                    queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.base_drawing, modifiers = new List<TaskData.TaskModifier>() { TaskData.TaskModifier.top_left } });
                    queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.add_drawing, modifiers = new List<TaskData.TaskModifier>() { TaskData.TaskModifier.top_right } });
                    queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.add_drawing, modifiers = new List<TaskData.TaskModifier>() { TaskData.TaskModifier.bottom_right } });
                    queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.add_drawing, modifiers = new List<TaskData.TaskModifier>() { TaskData.TaskModifier.bottom_left } });
                    queuedTasks.Add(new TaskData() { taskType = TaskData.TaskType.base_guessing });
                    foreach (TaskData queuedTask in queuedTasks)
                    {
                        taskDuration = choice.GetTaskTiming(queuedTask.taskType);
                        queuedTask.duration = taskDuration;
                    }
                    break;
            }

            
        }
    }
}
