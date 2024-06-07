using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class WordGroupDisplayItem : MonoBehaviour
    {
        [SerializeField]
        private TMPro.TMP_Text wordGroupText;
        private string wordGroupName = "";
        private WordGroupsController wordGroupsController;
        private List<WordData> words = new List<WordData>();


        public void Initialize(string inWordGroupName, List<WordData> inWords, WordGroupsController inWordGroupsController)
        {
            wordGroupName = inWordGroupName;
            wordGroupText.text = wordGroupName;
            wordGroupsController = inWordGroupsController;
            words = inWords;
        }

        public void Review()
        {
            wordGroupsController.ReviewWordGroup(wordGroupName, words);
        }
    }
}