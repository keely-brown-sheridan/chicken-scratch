using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class WhiteOut : MonoBehaviour
    {
        [SerializeField]
        private DrawingController controller;
        public void undo()
        {
            controller.undoLastDrawingAction();
        }
    }
}