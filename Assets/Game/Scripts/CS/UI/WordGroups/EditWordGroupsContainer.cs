using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class EditWordGroupsContainer : MonoBehaviour
    {
        [SerializeField]
        private GameObject contentObject;

        [SerializeField]
        private GameObject wordItemPrefab;

        [SerializeField]
        private TMPro.TMP_InputField nameInputField;

        [SerializeField]
        private TMPro.TMP_Dropdown typeDropDown;

        [SerializeField]
        private GameObject addWordButtonObject;

        [SerializeField]
        private WordManagerWarningMessage wordManagerWarningMessage;

        [SerializeField]
        private Color warningMessageColour, validMessageColour;

        private WordGroupData.WordType currentWordType = WordGroupData.WordType.prefixes;

        public string editingCategoryName => _editingCategoryName;
        private string _editingCategoryName = "";

        private WordItem selectedItem = null;

        private void Update()
        {
            if (selectedItem != null && Input.GetKeyDown(KeyCode.Tab))
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    ShiftToHigherItem();
                }
                else
                {
                    ShiftToLowerItem();
                }
            }
        }

        public void AddToContent()
        {
            GameObject wordItemObject = Instantiate(wordItemPrefab, contentObject.transform);
            WordItem wordItem = wordItemObject.GetComponent<WordItem>();
            wordItem.Initialize(new WordData("", currentWordType.ToString(), 1), this);
            addWordButtonObject.transform.SetAsLastSibling();
        }

        public void SetWordType()
        {
            string wordTypeName = typeDropDown.captionText.text;
            switch (wordTypeName)
            {
                case "Prefixes":
                    currentWordType = WordGroupData.WordType.prefixes;
                    break;
                case "Nouns":
                    currentWordType = WordGroupData.WordType.nouns;
                    break;
                default:
                    return;
            }
        }

        public void Open(WordGroupData selectedGroupData)
        {
            //Delete word items - if any existed before
            WordItem[] existingWordItems = contentObject.transform.GetComponentsInChildren<WordItem>();

            if (existingWordItems.Length > 0)
            {
                for (int i = existingWordItems.Length - 1; i >= 0; i--)
                {
                    Destroy(existingWordItems[i].gameObject);
                }
            }

            nameInputField.text = selectedGroupData.name;
            _editingCategoryName = selectedGroupData.name;
            //Set the word type
            switch (selectedGroupData.wordType)
            {
                case WordGroupData.WordType.prefixes:
                    typeDropDown.value = 1;
                    break;
                case WordGroupData.WordType.nouns:
                    typeDropDown.value = 2;
                    break;
            }

            //Create word items for every word in the group
            foreach (WordData word in selectedGroupData.words)
            {
                GameObject wordItemObject = Instantiate(wordItemPrefab, contentObject.transform);
                WordItem wordItem = wordItemObject.GetComponent<WordItem>();
                wordItem.Initialize(new WordData(word.text, selectedGroupData.wordType.ToString(), word.difficulty));
            }

            addWordButtonObject.transform.SetAsLastSibling();
        }

        public bool CanSave(List<string> existingGroupNames, ref List<WordData> words)
        {
            foreach (string existingGroupName in existingGroupNames)
            {
                if (existingGroupName == nameInputField.text && existingGroupName != editingCategoryName)
                {
                    wordManagerWarningMessage.ShowMessage("Category already exists.", warningMessageColour);
                    return false;
                }
            }
            //Get all of the new words
            WordItem[] wordItems = contentObject.GetComponentsInChildren<WordItem>();

            //Check for repeats
            List<string> existingWords = new List<string>();
            foreach (WordItem wordItem in wordItems)
            {
                wordItem.Refresh(nameInputField.text);

                if (existingWords.Contains(wordItem.WordData.text))
                {
                    wordManagerWarningMessage.ShowMessage("Category contains word repeats.", warningMessageColour);
                    return false;
                }
                if (wordItem.WordData.text == "")
                {
                    wordManagerWarningMessage.ShowMessage("Category contains empty words.", warningMessageColour);
                    return false;
                }
                existingWords.Add(wordItem.WordData.text);
                words.Add(wordItem.WordData);
            }

            if (words.Count < 3)
            {
                wordManagerWarningMessage.ShowMessage("Insufficient words for a word group.", warningMessageColour);
                return false;
            }

            return true;
        }

        public WordGroupData Save(List<WordData> words, WordGroupData existingWordGroup)
        {
            WordGroupData.WordType wordType = WordGroupData.WordType.invalid;
            //Get the word type
            switch (typeDropDown.captionText.text)
            {
                case "Prefixes":
                    wordType = WordGroupData.WordType.prefixes;
                    break;
                case "Nouns":
                    wordType = WordGroupData.WordType.nouns;
                    break;
            }

            //Get existing group and update the name
            existingWordGroup.SetName(nameInputField.text);
            existingWordGroup.SetType(wordType);
            existingWordGroup.words = words;
            wordManagerWarningMessage.ShowMessage("Word group saved.", validMessageColour);
            return existingWordGroup;
        }

        public void SetSelectedItem(WordItem item)
        {
            selectedItem = item;
        }

        public void DeselectItem(WordItem item)
        {
            if (selectedItem == item)
            {
                selectedItem = null;
            }
        }

        private void ShiftToHigherItem()
        {
            List<Transform> itemTransforms = new List<Transform>();
            foreach (Transform itemTransform in contentObject.transform)
            {
                if (itemTransform != addWordButtonObject)
                {
                    itemTransforms.Add(itemTransform);
                }
            }
            for (int i = 1; i < itemTransforms.Count; i++)
            {
                if (itemTransforms[i] == selectedItem.transform)
                {
                    selectedItem = itemTransforms[i - 1].GetComponent<WordItem>();
                    selectedItem.Select();
                }
            }
        }

        private void ShiftToLowerItem()
        {
            List<Transform> itemTransforms = new List<Transform>();
            foreach (Transform itemTransform in contentObject.transform)
            {
                if (itemTransform != addWordButtonObject)
                {
                    itemTransforms.Add(itemTransform);
                }
            }
            for (int i = 0; i < itemTransforms.Count - 2; i++)
            {
                if (itemTransforms[i] == selectedItem.transform)
                {
                    selectedItem = itemTransforms[i + 1].GetComponent<WordItem>();
                    selectedItem.Select();
                }
            }
        }
    }
}