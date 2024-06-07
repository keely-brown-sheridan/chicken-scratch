using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class WordReviewDisplayItem : MonoBehaviour
    {
        [SerializeField]
        private TMPro.TMP_Text wordText;

        public void Initialize(string inWordName)
        {
            wordText.text = inWordName;
        }
    }
}