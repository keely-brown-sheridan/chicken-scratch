using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChickenScratch
{
    [Serializable]
    public class TaskData
    {
        public enum TaskType
        {
            base_drawing, prompting, copy_drawing, add_drawing, prompt_drawing, base_guessing
            //evaluation_guessing
            //collage_guessing
        }
        public enum TaskModifier
        {
            standard
            //shrunk
            //rush
        }

        public TaskType taskType;
        public List<TaskModifier> modifiers;
        public float duration;
    }
}
