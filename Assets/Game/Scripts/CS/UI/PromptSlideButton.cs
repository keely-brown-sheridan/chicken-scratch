using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ChickenScratch.ColourManager;

namespace ChickenScratch
{
    public class PromptSlideButton : MonoBehaviour
    {
        public List<PresentationWord> allWords = new List<PresentationWord>();
        public Dictionary<BirdName, Dictionary<int, PresentationWord>> wordObjectMap = new Dictionary<BirdName, Dictionary<int, PresentationWord>>();
        public Dictionary<int, BirdName> birdNameIndexMap = new Dictionary<int, BirdName>();

        public class PresentationWord
        {
            public Text wordText;
            public int wordIndex = -1;
            public BirdTag author;
        }
    }
}