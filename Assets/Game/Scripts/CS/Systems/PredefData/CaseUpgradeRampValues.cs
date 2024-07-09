using UnityEngine;

namespace ChickenScratch
{
    [CreateAssetMenu(fileName = "Case Upgrade Ramp", menuName = "GameData/Create Case Upgrade Ramp")]
    public class CaseUpgradeRampValues : ScriptableObject
    {
        public string rampType;
        public float modifierIncrease;
        public int pointsPerCorrectWordIncrease;
        public int bonusPointsIncrease;
    }
}
