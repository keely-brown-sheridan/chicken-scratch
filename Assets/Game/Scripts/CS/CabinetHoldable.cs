using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ChickenScratch.DeskHoldable;

namespace ChickenScratch
{
    public class CabinetHoldable : MonoBehaviour
    {
        public HoldableType type = HoldableType.invalid;
        public bool held = false;
    }
}