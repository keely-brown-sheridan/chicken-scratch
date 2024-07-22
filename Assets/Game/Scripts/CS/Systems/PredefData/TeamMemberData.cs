using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace ChickenScratch
{
    [CreateAssetMenu(fileName = "Team Member", menuName = "GameData/Create Team Member")]
    public class TeamMemberData : ScriptableObject
    {
        public Sprite sprite;
        public string name;
        public string description;
    }
}
