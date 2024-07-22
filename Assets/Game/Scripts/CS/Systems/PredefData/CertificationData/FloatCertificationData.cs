using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ChickenScratch
{
    [CreateAssetMenu(fileName = "FloatCertification", menuName = "GameData/Create Float Certification")]
    public class FloatCertificationData : CertificationData
    {
        public float value;
    }
}
