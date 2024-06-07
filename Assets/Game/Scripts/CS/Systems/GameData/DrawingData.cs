using ChickenScratch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChickenScratch
{
    [System.Serializable]
    public class DrawingData
    {

        public int caseID = -1;

        public int round = -1;

        public ColourManager.BirdName author = ColourManager.BirdName.none;
        public float timeTaken = 0.0f;

        public List<DrawingLineData> visuals = new List<DrawingLineData>();

        public DrawingData()
        {

        }
        public DrawingData(int inCaseID, int inRound, ColourManager.BirdName inAuthor)
        {
            caseID = inCaseID;
            round = inRound;
            author = inAuthor;
        }
    }
}
