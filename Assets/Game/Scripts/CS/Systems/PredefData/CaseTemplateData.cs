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
            standard, rush, contribution, shrunk, thirds, top_bottom, corners, curveball, blind, blender, morph, location, competition
        }
        public CaseFormat format = CaseFormat.standard;

        public List<TaskData> queuedTasks = new List<TaskData>();
        public List<string> startingWordIdentifiers = new List<string>();

        public int baseCost;
        public int basePointsPerCorrectWord;
        public int baseBonusPoints;
        public string caseTypeName;
        public Color caseTypeColour;

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
            startingScoreModifier = choice.startingScoreModifier;
            caseTypeName = choice.identifier;
            caseTypeColour = choice.colour;

            CreateTasks(choice);
        }

        private void CreateTasks(CaseChoiceData choice)
        {
            foreach(TemplateTaskData task in choice.taskTemplates)
            {
                queuedTasks.Add(new TaskData() { taskType = task.taskType, duration = task.duration, modifiers = task.taskModifiers, requiredRounds = task.requiredRoundTasks });
            }
        }
    }
}
