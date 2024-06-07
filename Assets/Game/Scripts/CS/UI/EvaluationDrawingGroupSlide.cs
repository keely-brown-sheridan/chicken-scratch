
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class EvaluationDrawingGroupSlide : MonoBehaviour
    {
        public int evaluationRound;
        public GameObject drawingPositionObject1, drawingPositionObject2;
        public GameObject containerObject1, containerObject2;
        public Image playerBirdImage1, playerBirdImage2;
        public Text playerNameText1, playerNameText2;
        public int playerIndex1, playerIndex2;
        public bool active = false;
    }
}