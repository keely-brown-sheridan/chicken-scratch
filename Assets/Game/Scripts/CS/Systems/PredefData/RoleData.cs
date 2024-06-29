using System;
using UnityEngine;

namespace ChickenScratch
{
    [CreateAssetMenu(fileName = "Role", menuName = "GameData/Create Role")]
    public class RoleData : ScriptableObject
    {
        public enum RoleType
        {
            worker, botcher
        }
        public RoleType roleType;

        public Color roleColour;

        public enum Team
        {
            worker, botcher
        }
        public Team team;

        public string roleName;
    }
}
