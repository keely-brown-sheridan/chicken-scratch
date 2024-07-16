using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ChickenScratch
{
    [CreateAssetMenu(fileName = "Case Word Template", menuName = "GameData/Create Case Word Template")]
    public class CaseWordTemplateData : ScriptableObject
    {
        public enum CaseWordType
        {
            descriptor, noun, variant, invalid
        }
        public CaseWordType type = CaseWordType.invalid;

        public string identifier;

        public int difficultyMinimum = 1;
        public int difficultyMaximum = 5;
        public int cost = 0;
        public int reward = 0;
        public int numberOfOptionsForGuessing = 4;
    }
}
