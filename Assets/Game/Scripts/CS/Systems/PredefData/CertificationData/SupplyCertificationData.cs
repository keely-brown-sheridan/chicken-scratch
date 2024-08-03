using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ChickenScratch
{
    [CreateAssetMenu(fileName = "SupplyCertification", menuName = "GameData/Create Supply Certification")]
    public class SupplyCertificationData : CertificationData
    {
        public int dayIndex;
    }
}
