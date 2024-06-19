using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    [CreateAssetMenu(fileName = "Result Data", menuName = "GameData/Create Result")]
    public class ResultData : ScriptableObject
    {
        public Color goalTextColour = Color.black;
        public Color sheetColour = new Color(0.5f, 0.5f, 0.0f);
        public Color resultTextColour = Color.black;
        public Color slideProgressBGColour;
        public Color slideProgressFillColour;
        public Material lineMaterial = null;
        public string resultName = "";
        public string shortFormIdentifier = "";
        public Sprite bossFaceReaction = null;
        public FinalEndgameResultManager.State finalFaceState;
        public string bossMessage = "";
        public string sfxToPlay = "";
        public List<GameModeRequiredPointThreshold> requiredPointThresholds;
        public WorkingGoalsManager.GoalType goal;

        public float getRequiredPointThreshold(string gameModeName)
        {
            foreach (GameModeRequiredPointThreshold requiredPointThreshold in requiredPointThresholds)
            {
                if (gameModeName == requiredPointThreshold.gameModeName)
                {
                    float requiredPoints = requiredPointThreshold.requiredPointThresholdPerPlayer * SettingsManager.Instance.GetPlayerNameCount();
                    return requiredPoints;
                }
            }
            return -1;
        }

        [System.Serializable]
        public class GameModeRequiredPointThreshold
        {
            public string gameModeName = "";
            public float requiredPointThresholdPerPlayer = -1;
        }
    }
}
