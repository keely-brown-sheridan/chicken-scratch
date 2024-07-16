using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChickenScratch
{
    [Serializable]
    public class TemplateTaskData
    {
        public int roundIndex;
        public TaskData.TaskType taskType;
        public int duration;
        public List<TaskData.TaskModifier> taskModifiers;
        public List<int> requiredRoundTasks;
        public List<WordPromptMapData> caseWordIndices;
    }
}
