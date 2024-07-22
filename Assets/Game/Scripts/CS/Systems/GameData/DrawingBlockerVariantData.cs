using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    [CreateAssetMenu(fileName = "DrawingBlocker", menuName = "GameData/Create Drawing Blocker")]
    public class DrawingBlockerVariantData : ScriptableObject
    {
        public List<DrawingBlockerBoxData> drawingBoxes;
    }
}
