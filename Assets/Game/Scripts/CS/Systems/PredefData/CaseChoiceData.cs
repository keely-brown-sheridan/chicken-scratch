using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    [CreateAssetMenu(fileName = "Case Choice", menuName = "GameData/Create Case Choice")]
    public class CaseChoiceData : ScriptableObject
    {
        public enum DifficultyDescriptor
        {
            Easy, Mild, Average, Tricky, Distressing
        }
        public string identifier;
        public CaseTemplateData.CaseFormat caseFormat;
        public int numberOfTasks;
        public int pointsPerCorrectWord;
        public int bonusPoints;
        public int cost;
        public DifficultyDescriptor difficulty;
        public int penalty;
        public float startingScoreModifier;
        public float maxScoreModifier;
        public float taskFalloff;
        public Color colour;
        public List<string> startingWordIdentifiers = new List<string>();
        public int selectionFrequency;
       

        [SerializeField]
        private List<TaskTimingData> taskTimingData = new List<TaskTimingData>();

        public enum TaskSprite
        {
            drawing, prompt, guess
        }
        public List<TaskSprite> queuedTaskSprites;
        public string description;


        public float GetTaskTiming(TaskData.TaskType taskType)
        {
            foreach(TaskTimingData taskTiming in taskTimingData)
            {
                if(taskTiming.taskType == taskType)
                {
                    return taskTiming.duration;
                }
            }

            return 0f;
        }
    }
}
