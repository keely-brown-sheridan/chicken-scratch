using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ChickenScratch
{
    [CreateAssetMenu(fileName = "DrawingTool", menuName = "GameData/Create Drawing Tool")]
    public class DrawingToolData : ScriptableObject
    {
        public GameObject toolPrefab;
        public GameObject heldPrefab;
        public DrawingController.DrawingToolType toolType;

    }
}
