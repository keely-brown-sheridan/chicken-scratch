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
        public List<string> caseChoiceIdentifiers = new List<string>();
        public List<TaskTimingData> taskTimingData;
        public float scoreModifierDecrement;
        public float casesPerPlayer;
        public int casesRemaining;
        public float contributionTaskRatio;

        public string description;
        public int goalPointsPerCharacter;
        public float baseTimeInDrawingRound = 120;
        public float totalGameTime;

    }
}