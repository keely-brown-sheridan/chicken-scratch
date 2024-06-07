using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class WordGroupReviewContainer : MonoBehaviour
    {
        [SerializeField]
        private GameObject contentObject;
        [SerializeField]
        private TMPro.TMP_Text groupNameText;

        [SerializeField]
        private GameObject wordReviewItemPrefab;

        public void Refresh(string groupName, List<WordData> words)
        {
            groupNameText.text = groupName;

            //Delete the existing words
            WordReviewDisplayItem[] existingWordReviewDisplayItems = contentObject.transform.GetComponentsInChildren<WordReviewDisplayItem>();
            for (int i = existingWordReviewDisplayItems.Length - 1; i >= 0; i--)
            {
                Destroy(existingWordReviewDisplayItems[i].gameObject);
            }

            foreach (WordData word in words)
            {
                GameObject wordReviewDisplayObject = Instantiate(wordReviewItemPrefab, contentObject.transform);
                WordReviewDisplayItem wordReviewDisplayItem = wordReviewDisplayObject.GetComponent<WordReviewDisplayItem>();
                wordReviewDisplayItem.Initialize(word.text);
            }
        }
    }
}