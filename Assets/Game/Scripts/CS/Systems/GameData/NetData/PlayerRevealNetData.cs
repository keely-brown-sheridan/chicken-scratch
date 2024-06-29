using System;
using UnityEngine;

namespace ChickenScratch
{
    [Serializable]
    public class PlayerRevealNetData
    {
        public ColourManager.BirdName playerBird;
        public string playerName;
        public RoleData.RoleType roleType;
    }
}
