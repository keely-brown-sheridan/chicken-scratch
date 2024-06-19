using System;
using UnityEngine;

namespace ChickenScratch
{
    [Serializable]
    public class CaseWordData
    {
        public string identifier;
        public string value = "";
        public int difficulty = -1;
        public string category = "";
        public WordGroupData.WordType wordType;
    }
}
