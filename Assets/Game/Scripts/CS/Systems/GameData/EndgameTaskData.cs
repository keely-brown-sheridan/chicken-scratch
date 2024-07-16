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
        public bool expectingDrawing = false;

        public int round;
        public TaskData.TaskType taskType;
        public PlayerRatingData ratingData;
        public DrawingData drawingData;
        public PlayerTextInputData promptData;
        public ColourManager.BirdName assignedPlayer;
        public float timeModifierDecrement;
        public bool isComplete = false;

        public EndgameTaskData()
        {

        }

        public EndgameTaskData(TaskData taskData, int inRound, ColourManager.BirdName inPlayer, float inTimeModifierDecrement)
        {
            taskType = taskData.taskType;
            assignedPlayer = inPlayer;
            ratingData = new PlayerRatingData();
            round = inRound;
            timeModifierDecrement = inTimeModifierDecrement;
        }

        public EndgameTaskData(EndgameTaskNetData netData)
        {
            expectingDrawing = netData.expectingDrawing;
            taskType = netData.taskType;
            ratingData = netData.ratingData;
            promptData = netData.promptData;
            assignedPlayer = netData.assignedPlayer;
            timeModifierDecrement = netData.timeModifierDecrement;
            isComplete = netData.isComplete;
        }


    }
}
