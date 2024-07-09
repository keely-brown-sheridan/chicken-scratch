using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

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

        [SerializeField]
        private GameObject addWordButtonObject;

        [SerializeField]
        private ScrollRect scrollRect;

        [SerializeField]
        private WordManagerWarningMessage wordManagerWarningMessage;

        [SerializeField]
        private Color warningMessageColour, validMessageColour;

        private WordGroupData.WordType currentWordType = WordGroupData.WordType.prefixes;
        private WordItem selectedItem = null;

        private void Update()
        {
            if(selectedItem != null && Input.GetKeyDown(KeyCode.Tab))
            {
                if(Input.GetKey(KeyCode.LeftShift) ||  Input.GetKey(KeyCode.RightShift))
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

            //Shift the scrollbar to the bottom
            float backup = scrollRect.verticalNormalizedPosition;

            /* Content changed here */

            StartCoroutine(ApplyScrollPosition(scrollRect, backup));
        }

        IEnumerator ApplyScrollPosition(ScrollRect sr, float verticalPos)
        {
            yield return new WaitForEndOfFrame();
            sr.verticalNormalizedPosition = verticalPos;
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)sr.transform);
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
                    wordManagerWarningMessage.ShowMessage("Word group already exists.", warningMessageColour);
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
                    wordManagerWarningMessage.ShowMessage("Word group contains word repeats.", warningMessageColour);
                    return false;
                }
                if (wordItem.WordData.text == "")
                {
                    wordManagerWarningMessage.ShowMessage("Word group contains empty words.", warningMessageColour);
                    return false;
                }
                existingWords.Add(wordItem.WordData.text);
                words.Add(wordItem.WordData);
            }

            if (words.Count < 3)
            {
                wordManagerWarningMessage.ShowMessage("Insufficient words in group (min 8).", warningMessageColour);
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
                    wordManagerWarningMessage.ShowMessage("Invalid word type.", warningMessageColour);
                    return false;
            }

            return true;
        }

        public void ClearWords()
        {
            List<Transform> wordTransforms = new List<Transform>();
            foreach(Transform wordTransform in contentObject.transform)
            {
                if(addWordButtonObject.transform == wordTransform)
                {
                    continue;
                }
                wordTransforms.Add(wordTransform);
            }
            for(int i = wordTransforms.Count - 1; i >= 0; i--)
            {
                Destroy(wordTransforms[i].gameObject);
            }
        }

        public WordGroupData Save(List<WordData> words)
        {
            WordGroupData currentWordGroup = new WordGroupData(nameInputField.text, currentWordType, words, true, false);
            wordManagerWarningMessage.ShowMessage("Word group saved.", validMessageColour);
            return currentWordGroup;
        }

        public void ForceInputToUppercase()
        {
            nameInputField.text = nameInputField.text.ToUpper();
        }

        public void SetSelectedItem(WordItem item)
        {
            selectedItem = item;
        }

        public void DeselectItem(WordItem item)
        {
            if(selectedItem == item)
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
                    selectedItem = itemTransforms[i-1].GetComponent<WordItem>();
                    selectedItem.Select();
                    break;
                }
            }
        }

        private void ShiftToLowerItem()
        {
            List<Transform> itemTransforms = new List<Transform>();
            foreach(Transform itemTransform in contentObject.transform)
            {
                if(itemTransform != addWordButtonObject)
                {
                    itemTransforms.Add(itemTransform);
                }
            }
            for(int i = 0; i < itemTransforms.Count - 2; i++)
            {
                if (itemTransforms[i] == selectedItem.transform)
                {
                    selectedItem = itemTransforms[i+1].GetComponent<WordItem>();
                    selectedItem.Select();
                    break;
                }
            }
        }

    }
}