using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ChickenScratch
{
    [CreateAssetMenu(fileName = "OverheadCertification", menuName = "GameData/Create Overhead Certification")]
    public class OverheadCertificationData : CertificationData
    {
        public int initialCost;
        public float modifierIncrement;
        public int correctWordBirdbucksIncrement;
        public int bonusBirdbucksIncrement;
    }
}
