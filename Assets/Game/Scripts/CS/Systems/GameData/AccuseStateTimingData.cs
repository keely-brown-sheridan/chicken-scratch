using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChickenScratch
{
    [System.Serializable]
    public class AccuseStateTimingData
    {
        public AccusationRound.RoundState state;
        public AccusationRound.RoundState nextState;
        public float duration;
    }
}
