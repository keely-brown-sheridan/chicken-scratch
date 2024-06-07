using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class DeskHoldable : MonoBehaviour
    {
        public enum HoldableType
        {
            invalid, stress_ball
        }

        public HoldableType type = HoldableType.invalid;
    }

}