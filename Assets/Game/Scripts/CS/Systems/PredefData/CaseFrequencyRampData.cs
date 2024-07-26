using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ChickenScratch
{
    [CreateAssetMenu(fileName = "CaseFrequencyRamp", menuName = "GameData/Create Case Frequency Ramp")]
    public class CaseFrequencyRampData : ScriptableObject
    {
        public string identifier;
        public List<int> incrementValues;
    }
}
