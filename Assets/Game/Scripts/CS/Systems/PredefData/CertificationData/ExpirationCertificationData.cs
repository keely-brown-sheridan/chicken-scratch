using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ChickenScratch
{
    [CreateAssetMenu(fileName = "ExpirationCertification", menuName = "GameData/Create Expiration Certification")]
    public class ExpirationCertificationData : CertificationData
    {
        public float modifierIncrement;
        public int correctWordBirdbucksIncrement;
        public int bonusBirdbucksIncrement;
    }
}
