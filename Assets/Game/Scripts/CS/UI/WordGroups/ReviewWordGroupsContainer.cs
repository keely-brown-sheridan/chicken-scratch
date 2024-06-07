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

        public WordGroupItem CreateWordGroup(string wordGroupName, WordGroupData.WordType wordType, List<WordData> words, WordGroupsController wordGroupsController, bool isHost)
        {
            GameObject wordGroupObject = Instantiate(wordGroupItemPrefab, contentObject.transform);
            WordGroupItem wordGroupItem = wordGroupObject.GetComponent<WordGroupItem>();
            WordGroupData wordGroupData = new WordGroupData(wordGroupName, wordType, words, true, false);
            wordGroupItem.Initialize(wordGroupData, wordGroupsController, isHost);
            return wordGroupItem;
        }

        public void CreateLockedWordGroup(string wordGroupName, WordGroupData.WordType wordType, List<WordData> words, WordGroupsController wordGroupsController, bool isHost)
        {
            WordGroupItem wordGroupItem = CreateWordGroup(wordGroupName, wordType, words, wordGroupsController, isHost);
            wordGroupItem.SetAsLocked();
        }
    }
}