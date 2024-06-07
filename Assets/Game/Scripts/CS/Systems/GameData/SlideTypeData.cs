using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ChickenScratch
{
    [Serializable]
    public class SlideTypeData
    {
        public enum SlideType
        {
            intro, drawing, prompt, guess, summary, invalid
        }
        public SlideType slideType;

        public float showDuration;
        public GameObject prefab;
    }
}
