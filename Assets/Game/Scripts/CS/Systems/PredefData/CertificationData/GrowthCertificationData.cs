using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ChickenScratch
{
    [CreateAssetMenu(fileName = "GrowthCertification", menuName = "GameData/Create Growth Certification")]
    public class GrowthCertificationData : CertificationData
    {
        public float initialMultiplierDecrement;
        public float modifierRamp;
    }
}
