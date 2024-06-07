using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public abstract class ColourDrawingTool : DrawingTool
    {
        public Color currentColour => _currentColour;
        public DrawingLineData.LineColour currentColourType => _currentColourType;
        protected Color _currentColour;
        protected DrawingLineData.LineColour _currentColourType;
    }
}