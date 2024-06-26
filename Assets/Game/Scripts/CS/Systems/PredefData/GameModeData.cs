using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    [CreateAssetMenu(fileName = "GameMode", menuName = "GameData/Create Game Mode")]
    public class GameModeData : ScriptableObject
    {
        public string title;
        public enum CaseDeliveryMode
        {
            queue, free_for_all
        }
        public enum WordDistributionMode
        {
            random
        }

        public List<DayData> days;

        public int minimumNumberOfPlayers = 2;

        public WordDistributionMode wordDistributionMode = WordDistributionMode.random;
        public CaseDeliveryMode caseDeliveryMode = CaseDeliveryMode.queue;

        public CaseTemplateData baseTemplateData;
        public List<string> caseChoiceIdentifiers = new List<string>();

        public int casesRemaining;
        public float contributionTaskRatio;

        public string description;
        public float totalGameTime;
        public int maxDays;

    }
}