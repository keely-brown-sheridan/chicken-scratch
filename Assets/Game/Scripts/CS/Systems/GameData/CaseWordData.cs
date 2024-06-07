using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChickenScratch.GameModeData;

namespace ChickenScratch
{
    public class CaseWordData
    {
        public string value = "";
        public CaseWordTemplateData template = new CaseWordTemplateData();
        public enum DifficultyDescriptor
        {
            Easy, Mild, Average, Tricky, Distressing
        }
        public int difficulty = -1;
        public string category = "";
        public CaseWordData()
        {

        }

        public CaseWordData(string inValue, CaseWordTemplateData inTemplate, int inDifficulty)
        {
            value = inValue;
            template = inTemplate;
            difficulty = inDifficulty;
        }
    }
}
