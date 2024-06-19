using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChickenScratch.ColourManager;

namespace ChickenScratch
{
    [Serializable]
    public class GuessData
    {
        public string prefix = "";
        public string noun = "";
        public BirdName author = BirdName.none;
        public int round = -1;
        public float timeTaken = 0f;
    }
}
