using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    [CreateAssetMenu(fileName = "Case Choice", menuName = "GameData/Create Case Choice")]
    public class CaseChoiceData : ScriptableObject
    {
        public string identifier;
        public string description;
        public CaseTemplateData.CaseFormat caseFormat;

        public enum PromptFormat
        {
            standard, location, variant
        }
        public PromptFormat promptFormat;

        public int numberOfTasks;
        public List<WordPromptTemplateData> startingWordIdentifiers = new List<WordPromptTemplateData>();
        public List<TemplateTaskData> taskTemplates = new List<TemplateTaskData>();

        //***********************************************************************//
        //STATS STUFF
        //***********************************************************************//
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
        [HideInInspector]
        public float maxScoreModifier;
        public float startingScoreModifier;
        public float modifierDecrement;

        [SerializeField]
        private int baseFrequency;
        [SerializeField]
        private CaseFrequencyRampData frequencyRampData;
        [HideInInspector]
        public int selectionFrequency;
        [HideInInspector]
        public int currentFrequencyRampIndex;
        //***********************************************************************//
       
        //***********************************************************************//
        //CERTIFICATION STUFF
        //***********************************************************************//
        public int percentageChanceOfGoodCertification => _percentageChanceOfGoodCertification;
        public int percentageChanceOfBadCertification => _percentageChanceOfBadCertification;
        [SerializeField]
        private int _percentageChanceOfGoodCertification, _percentageChanceOfBadCertification;

        public int maxNumberOfSeals => _maxNumberOfSeals;
        [SerializeField]
        private int _maxNumberOfSeals;

        //***********************************************************************//

        public Color colour;
        public Color backgroundFontColour;
        public Color importantFontColour;
        public List<CaseUpgradeStoreItemData> upgrades = new List<CaseUpgradeStoreItemData>();

        public void SendFrequencyToClients()
        {
            GameManager.Instance.gameDataHandler.RpcUpdateFrequencyStoreOption(identifier, selectionFrequency, currentFrequencyRampIndex);
        }

        public void ApplyFrequencyRamp()
        {
            if (frequencyRampData.incrementValues.Count <= currentFrequencyRampIndex)
            {
                //Cannot increase frequency any more using the ramp, remove it from the store options
                GameManager.Instance.gameDataHandler.RpcRemoveFrequencyStoreOption(identifier);
                return;
            }
            selectionFrequency += frequencyRampData.incrementValues[currentFrequencyRampIndex];
            
            currentFrequencyRampIndex++;

            //Broadcast the updated selection frequency
            SendFrequencyToClients();
        }

        public void IncrementFrequency()
        {
            selectionFrequency++;
            SendFrequencyToClients();
        }

        public int GetFrequencyIncreaseValue(int rampIndex)
        {
            if (frequencyRampData.incrementValues.Count <= currentFrequencyRampIndex)
            {
                return 0;
            }
            return frequencyRampData.incrementValues[currentFrequencyRampIndex];
        }


        public void Reset()
        {
            startingScoreModifier = baseStartingModifier;
            bonusPoints = baseBonusPoints;
            pointsPerCorrectWord = basePointsPerCorrectWord;
            maxScoreModifier = startingScoreModifier;
            selectionFrequency = baseFrequency;
            currentFrequencyRampIndex = 0;
        }
    }
}
