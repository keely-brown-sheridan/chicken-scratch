using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChickenScratch
{
    [Serializable]
    public class RevealStateData
    {
        public AccusationReveal.State currentState;
        public AccusationReveal.State nextState;
        public float duration;
    }
}
