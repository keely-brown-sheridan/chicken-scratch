
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace ChickenScratch
{
    [System.Serializable]
    public class DrawingData
    {
        [XmlAttribute]
        public int caseID = -1;

        [XmlAttribute]
        public int round = -1;

        [XmlAttribute]
        public ColourManager.BirdName author = ColourManager.BirdName.none;
        [XmlIgnore]
        public float timeTaken = 0.0f;

        [XmlArray("Visuals"), XmlArrayItem("Visual")]
        public List<DrawingLineData> visuals = new List<DrawingLineData>();

        [XmlAttribute("IsQueued")]
        public bool isQueuedForPlayer = false;

        public DrawingData()
        {

        }
        public DrawingData(int inCaseID, int inRound, ColourManager.BirdName inAuthor)
        {
            caseID = inCaseID;
            round = inRound;
            author = inAuthor;
        }

        public void PrepareForXmlSave(bool inIsQueuedForPlayer)
        {
            isQueuedForPlayer = inIsQueuedForPlayer;
            foreach(DrawingLineData visual in visuals)
            {
                visual.PrepareForXmlSave();
            }
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
