using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChickenScratch
{
    public class GoalData
    {
        public int requiredPoints = -1;
        public string name = "";

        public GoalData(int inRequiredPoints, string inName = "")
        {
            requiredPoints = inRequiredPoints;
            name = inName;
        }
    }
}
