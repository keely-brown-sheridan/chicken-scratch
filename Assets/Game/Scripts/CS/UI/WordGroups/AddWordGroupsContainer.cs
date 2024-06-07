using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ChickenScratch
{
    public class AddWordGroupsContainer : MonoBehaviour
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

        public bool CanSave(List<string> existingWordGroupNames, ref List<WordData> words)
        {
            foreach (string wordGroupName in existingWordGroupNames)
            {
                if (wordGroupName == nameInputField.text)
                {
                    Debug.LogError("Error: Trying to save word group with name of existing word group.");
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

            //Get the word type
            switch (typeDropDown.captionText.text)
            {
                case "Prefixes":
                    currentWordType = WordGroupData.WordType.prefixes;
                    break;
                case "Nouns":
                    currentWordType = WordGroupData.WordType.nouns;
                    break;
                default:
                    Debug.LogError("Invalid word type selected.");
                    return false;
            }

            return true;
        }

        public WordGroupData Save(List<WordData> words)
        {
            WordGroupData currentWordGroup = new WordGroupData(nameInputField.text, currentWordType, words, true, false);
            return currentWordGroup;
        }
    }
}