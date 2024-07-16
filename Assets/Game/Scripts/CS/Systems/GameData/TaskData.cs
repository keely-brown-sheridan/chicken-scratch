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
            base_drawing, prompting, copy_drawing, add_drawing, prompt_drawing, base_guessing, compile_drawing, blender_drawing, morph_guessing
            //evaluation_guessing
            //collage_guessing
        }
        public enum TaskModifier
        {
            standard, shrunk, thirds_first, thirds_second, thirds_third, top, bottom, top_left, top_right, bottom_left, bottom_right, hidden_prefix,
            blind, hidden_noun, capped_prompt, flipped, morphed, invalid
        }

        public TaskType taskType;
        public List<TaskModifier> modifiers = new List<TaskModifier>();
        public float duration;
        public float timeModifierDecrement;
        public List<int> requiredRounds;

        public DrawingRound.CaseState GetCaseState()
        {
            switch(taskType)
            {
                case TaskType.base_drawing:
                case TaskType.prompt_drawing:
                    return DrawingRound.CaseState.drawing;
                case TaskType.add_drawing:
                    return DrawingRound.CaseState.add_drawing;
                case TaskType.copy_drawing:
                    return DrawingRound.CaseState.copy_drawing;
                case TaskData.TaskType.blender_drawing:
                    return DrawingRound.CaseState.blender_drawing;
                case TaskType.base_guessing:
                    return DrawingRound.CaseState.guessing;
                case TaskType.morph_guessing:
                    return DrawingRound.CaseState.morph_guessing;
                case TaskType.prompting:
                    return DrawingRound.CaseState.prompting;

            }
            return DrawingRound.CaseState.invalid;
        }
    }
}
