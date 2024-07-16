using ChickenScratch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChickenScratch
{
    [Serializable]
    public class EndgameTaskNetData
    {
        public bool expectingDrawing = false;

        public int round;
        public TaskData.TaskType taskType;
        public PlayerRatingData ratingData;
        public PlayerTextInputData promptData;
        public ColourManager.BirdName assignedPlayer;
        public float timeModifierDecrement;
        public bool isComplete = false;

        public EndgameTaskNetData() 
        { 
        }

        public EndgameTaskNetData(EndgameTaskData taskData)
        {
            round = taskData.round;
            expectingDrawing = taskData.expectingDrawing;
            taskType = taskData.taskType;
            ratingData = taskData.ratingData;
            promptData = taskData.promptData;
            assignedPlayer = taskData.assignedPlayer;
            timeModifierDecrement = taskData.timeModifierDecrement;
            isComplete = taskData.isComplete;
        }
    }
}
