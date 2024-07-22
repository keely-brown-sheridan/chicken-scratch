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

        [HideInInspector]
        public int pointsPerCorrectWord;
        [HideInInspector]
        public int bonusPoints;
        [HideInInspector]
        public int cost;

        [SerializeField]
        private int basePointsPerCorrectWord;
        [SerializeField]
        private int baseBonusPoints;
        [SerializeField]
        private float baseStartingModifier;
        [SerializeField]
        private int baseFrequency;

        public List<TemplateTaskData> taskTemplates = new List<TemplateTaskData>();

        public DifficultyDescriptor difficulty;
        public int penalty;
        public float startingScoreModifier;
        public float modifierDecrement;
        public float taskFalloff;
        public Color colour;
        public Color backgroundFontColour;
        public Color importantFontColour;
        public List<WordPromptTemplateData> startingWordIdentifiers = new List<WordPromptTemplateData>();

        public int percentageChanceOfGoodCertification => _percentageChanceOfGoodCertification;
        public int percentageChanceOfBadCertification => _percentageChanceOfBadCertification;
        [SerializeField]
        private int _percentageChanceOfGoodCertification, _percentageChanceOfBadCertification;

        public int maxNumberOfSeals => _maxNumberOfSeals;
        [SerializeField]
        private int _maxNumberOfSeals;

        public List<string> currentSealIdentifiers => _currentSealIdentifiers;
        private List<string> _currentSealIdentifiers = new List<string>();

        [HideInInspector]
        public int selectionFrequency;

        [HideInInspector]
        public float maxScoreModifier;

        [SerializeField]
        private List<TaskTimingData> taskTimingData = new List<TaskTimingData>();
        public string description;

        public List<CaseUpgradeStoreItemData> upgrades = new List<CaseUpgradeStoreItemData>();

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

        public void Reset()
        {
            startingScoreModifier = baseStartingModifier;
            bonusPoints = baseBonusPoints;
            pointsPerCorrectWord = basePointsPerCorrectWord;
            maxScoreModifier = startingScoreModifier;
            selectionFrequency = baseFrequency;
            _currentSealIdentifiers = new List<string>();
        }
    }
}
