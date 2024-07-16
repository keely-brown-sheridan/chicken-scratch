using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChickenScratch
{
    [System.Serializable]
    public class PlayerRatingNetData
    {
        public PlayerRatingData data;
        public int caseID = -1;
        public int round = -1;

        public PlayerRatingNetData()
        {

        }

        public PlayerRatingNetData(PlayerRatingData inData, int inCaseID, int inRound)
        {
            data = inData;
            caseID = inCaseID;
            round = inRound;
        }
    }
}
