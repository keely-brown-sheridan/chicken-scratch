using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class PossiblePrompt : MonoBehaviour
    {
        public int wordIndex;
        public string identifier;
        public Text displayText;
        public Image backgroundImage;
        public bool isCorrect = false;
    }
}