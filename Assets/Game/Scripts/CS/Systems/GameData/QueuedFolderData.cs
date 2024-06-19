using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChickenScratch
{
    [Serializable]
    public class QueuedFolderData
    {
        public int round = -1;
        public DrawingRound.CaseState queuedState = DrawingRound.CaseState.invalid;
    }
}
