using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public abstract class ColourDrawingTool : DrawingTool
    {
        public Color currentColour => _currentColour;
        protected Color _currentColour;
    }
}