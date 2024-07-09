using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class ReviewWordGroupsContainer : MonoBehaviour
    {
        [SerializeField]
        private GameObject wordGroupItemPrefab;

        [SerializeField]
        private GameObject contentObject;

        [SerializeField]
        private GameObject addButtonObject;

        public void ClearWordGroups()
        {
            List<GameObject> wordGroupDisplayItems = new List<GameObject>();
            foreach (Transform child in contentObject.transform)
            {
                if(child.gameObject == addButtonObject)
                {
                    continue;
                }
                wordGroupDisplayItems.Add(child.gameObject);
            }
            for (int i = wordGroupDisplayItems.Count - 1; i >= 0; i--)
            {
                Destroy(wordGroupDisplayItems[i]);
            }
        }

        public WordGroupItem CreateWordGroup(string wordGroupName, WordGroupData.WordType wordType, List<WordData> words, WordGroupsController wordGroupsController, bool isHost)
        {
            GameObject wordGroupObject = Instantiate(wordGroupItemPrefab, contentObject.transform);
            WordGroupItem wordGroupItem = wordGroupObject.GetComponent<WordGroupItem>();
            WordGroupData wordGroupData = new WordGroupData(wordGroupName, wordType, words, true, false);
            wordGroupItem.Initialize(wordGroupData, wordGroupsController, isHost);

            //Shift add button to the bottom
            addButtonObject.transform.SetAsLastSibling();
            return wordGroupItem;
        }

        public void CreateLockedWordGroup(string wordGroupName, WordGroupData.WordType wordType, List<WordData> words, WordGroupsController wordGroupsController, bool isHost)
        {
            WordGroupItem wordGroupItem = CreateWordGroup(wordGroupName, wordType, words, wordGroupsController, isHost);
            wordGroupItem.SetAsLocked();
        }
    }
}