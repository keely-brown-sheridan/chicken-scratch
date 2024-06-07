using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class EndlessModeSettings : MonoBehaviour
    {
        public int maxNounDifficulty = 5;
        public int maxDescriptorDifficulty = 5;
        public int maxNumberOfPossibleDescriptors = 8;
        public int maxNumberOfPossibleNouns = 10;

        public int numberOfChainsPerPlayerUntilDifficultyRamp = 2;

        public int currentNounDifficultyModifier = 0;
        public int currentDescriptorDifficultyModifier = 0;
        public int currentPossibleDescriptorModifier = 0;
        public int currentPossibleNounModifier = 0;
        public int numberOfChainsSinceLastRamp = 0;

        public float timeDetractionForFailure = 10f;
        public float timeBonusPerCorrectWord = 10f;
        public float timeBonusForAllCorrectWords = 10f;
        public int maxModifierValue = 8;

        public void reset()
        {
            currentPossibleDescriptorModifier = 0;
            currentNounDifficultyModifier = 0;
            currentPossibleDescriptorModifier = 0;
            currentPossibleNounModifier = 0;
            numberOfChainsSinceLastRamp = 0;
        }
    }
}