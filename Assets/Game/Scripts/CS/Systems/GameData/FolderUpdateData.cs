using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChickenScratch.ColourManager;
using static ChickenScratch.DrawingRound;

namespace ChickenScratch
{
    [Serializable]
    public class FolderUpdateData
    {
        public int cabinetIndex;
        public CaseState currentState;
        public float taskTime;
        public float currentScoreModifier;
        public float maxScoreModifier;
        public int roundNumber;
        public int caseID;
        public BirdName player;
        public List<TaskData.TaskModifier> taskModifiers;
        public BirdName lastPlayer;
        public WordCategoryData wordCategory;
    }
}
