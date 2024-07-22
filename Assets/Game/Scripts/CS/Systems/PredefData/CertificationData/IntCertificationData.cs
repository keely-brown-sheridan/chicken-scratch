using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ChickenScratch
{
    [CreateAssetMenu(fileName = "IntCertification", menuName = "GameData/Create Int Certification")]
    public class IntCertificationData : CertificationData
    {
        public int value;
    }
}
