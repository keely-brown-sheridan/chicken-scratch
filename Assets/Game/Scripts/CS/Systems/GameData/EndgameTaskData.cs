using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChickenScratch
{
    [Serializable]
    public class EndgameTaskData
    {
        public int round;
        public TaskData.TaskType taskType;
        public PlayerRatingData ratingData;
        public DrawingData drawingData;
        public PlayerTextInputData promptData;
        public ColourManager.BirdName assignedPlayer;

        public EndgameTaskData()
        {

        }

        public EndgameTaskData(TaskData taskData, int inRound, ColourManager.BirdName inPlayer)
        {
            taskType = taskData.taskType;
            assignedPlayer = inPlayer;
            ratingData = new PlayerRatingData();
            round = inRound;
        }


    }
}
