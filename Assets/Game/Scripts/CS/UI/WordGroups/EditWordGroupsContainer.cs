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

        private WordGroupData.WordType currentWordType = WordGroupData.WordType.prefixes;

        public string editingCategoryName => _editingCategoryName;
        private string _editingCategoryName = "";

        public void AddToContent()
        {
            GameObject wordItemObject = Instantiate(wordItemPrefab, contentObject.transform);
            WordItem wordItem = wordItemObject.GetComponent<WordItem>();
            wordItem.Initialize(new WordData("", currentWordType.ToString(), 1));
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
        }

        public bool CanSave(List<string> existingGroupNames, ref List<WordData> words)
        {
            foreach (string existingGroupName in existingGroupNames)
            {
                if (existingGroupName == nameInputField.text && existingGroupName != editingCategoryName)
                {
                    Debug.LogError("Error: Trying to save word group with a name that already exists.");
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
                    Debug.LogError("Error: Trying to save word group that has word repeats in it.");
                    return false;
                }
                if (wordItem.WordData.text == "")
                {
                    Debug.LogError("Error: Trying to save word group that has an empty word.");
                    return false;
                }
                existingWords.Add(wordItem.WordData.text);
                words.Add(wordItem.WordData);
            }

            if (words.Count < 3)
            {
                Debug.LogError("Error: Insufficient words for a word group.");
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
                case "Prefix":
                    wordType = WordGroupData.WordType.prefixes;
                    break;
                case "Noun":
                    wordType = WordGroupData.WordType.nouns;
                    break;
            }

            //Get existing group and update the name
            existingWordGroup.SetName(nameInputField.text);
            existingWordGroup.SetType(wordType);
            existingWordGroup.words = words;
            return existingWordGroup;
        }
    }
}