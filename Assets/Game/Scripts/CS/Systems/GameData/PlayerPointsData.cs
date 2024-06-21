using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChickenScratch.ColourManager;

namespace ChickenScratch
{
    public class PlayerPointsData
    {
        public BirdName player = BirdName.none;
        public int points = -1;
        public EndgameCaseData caseData;

        public PlayerPointsData(BirdName inPlayer, int inPoints, EndgameCaseData inCaseData)
        {
            player = inPlayer;
            points = inPoints;
            caseData = inCaseData;
        }
    }
}
