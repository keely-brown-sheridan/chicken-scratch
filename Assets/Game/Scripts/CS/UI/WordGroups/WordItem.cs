using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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

        [SerializeField]
        private GameObject highlightObject;

        public WordData WordData => wordData;

        private WordData wordData = null;

        private Dictionary<int, string> difficultyNameMap = new Dictionary<int, string>() { { 1, "Trivial" }, { 2, "Easy" }, { 3, "Mild" }, {4, "Tricky" }, { 5, "Distressing" }  };

        private AddWordGroupsContainer addContainer;
        private EditWordGroupsContainer editContainer;

        public void Initialize(WordData inWordData)
        {
            wordData = inWordData;
            wordText.text = wordData.text;
            difficultyText.text = wordData.difficulty.ToString() + " - " + difficultyNameMap[wordData.difficulty];
            plusObject.SetActive(wordData.difficulty < 5);
            minusObject.SetActive(wordData.difficulty > 1);
        }
        public void Initialize(WordData inWordData, AddWordGroupsContainer inAddContainer)
        {
            Initialize(inWordData);
            addContainer = inAddContainer;
        }
        public void Initialize(WordData inWordData, EditWordGroupsContainer inEditContainer)
        {
            Initialize(inWordData);
            editContainer = inEditContainer;
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
            difficultyText.text = wordData.difficulty.ToString() + " - " + difficultyNameMap[wordData.difficulty];

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
            difficultyText.text = wordData.difficulty.ToString() + "-" + difficultyNameMap[wordData.difficulty];
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

        public void ForceInputToUppercase()
        {
            wordText.text = wordText.text.ToUpper();
        }

        public void OnSelect()
        {
            highlightObject.SetActive(true);
            if(addContainer)
            {
                addContainer.SetSelectedItem(this);
            }
            if(editContainer)
            {
                editContainer.SetSelectedItem(this);
            }
        }

        public void OnDeselect()
        {
            highlightObject.SetActive(false);
            if (addContainer)
            {
                addContainer.DeselectItem(this);
            }
            if(editContainer)
            {
                editContainer.DeselectItem(this);
            }
        }

        public void Select()
        {
            wordText.Select();
        }
    }
}