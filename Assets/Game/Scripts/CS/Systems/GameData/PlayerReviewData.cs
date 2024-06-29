using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChickenScratch
{
    public class PlayerReviewData
    {
        public List<int> caseIndices = new List<int>();
        public int currentCaseIndex;

        public Dictionary<int, int> caseTaskMap = new Dictionary<int, int>();
        public int numberOfStars;
        public int numberOfEyes;
        public int birdBucksEarned;
        public string playerName;
        public Color playerColour;
        public Color bgColour;
        public Sprite faceSprite;
    }
}
