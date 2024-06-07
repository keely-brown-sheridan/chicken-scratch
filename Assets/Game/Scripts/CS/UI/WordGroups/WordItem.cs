using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class WordItem : MonoBehaviour
    {
        [SerializeField]
        private TMPro.TMP_InputField wordText;

        [SerializeField]
        private TMPro.TMP_Text difficultyText;

        [SerializeField]
        private GameObject plusObject;

        [SerializeField]
        private GameObject minusObject;

        public WordData WordData => wordData;

        private WordData wordData = null;

        public void Initialize(WordData inWordData)
        {
            wordData = inWordData;
            wordText.text = wordData.text;
            difficultyText.text = wordData.difficulty.ToString();
            plusObject.SetActive(wordData.difficulty < 5);
            minusObject.SetActive(wordData.difficulty > 1);
        }

        public void OnWordTextChanged()
        {
            wordData.text = wordText.text;
        }
        public void OnDifficultyIncreased()
        {
            wordData.difficulty++;
            if (wordData.difficulty == 5)
            {
                plusObject.SetActive(false);
            }
            else if (wordData.difficulty > 5)
            {
                wordData.difficulty = 5;
            }
            minusObject.SetActive(true);
            difficultyText.text = wordData.difficulty.ToString();

        }
        public void OnDifficultyDecreased()
        {
            wordData.difficulty--;
            if (wordData.difficulty == 1)
            {
                minusObject.SetActive(false);
            }
            else if (wordData.difficulty < 1)
            {
                wordData.difficulty = 1;
            }
            plusObject.SetActive(true);
            difficultyText.text = wordData.difficulty.ToString();
        }

        public void Delete()
        {
            Destroy(gameObject);
        }

        public void Refresh(string category)
        {
            wordData.text = wordText.text;
            wordData.category = category;
        }


    }
}