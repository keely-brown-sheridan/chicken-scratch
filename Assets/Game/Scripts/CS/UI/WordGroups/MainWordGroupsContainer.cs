using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class MainWordGroupsContainer : MonoBehaviour
    {
        [SerializeField]
        private GameObject editButton;
        [SerializeField]
        private GameObject contentObject;
        [SerializeField]
        private GameObject wordGroupDisplayItemPrefab;

        // Start is called before the first frame update
        void Start()
        {

        }

        public void SetEditButtonActiveState(bool isActive)
        {
            editButton.SetActive(isActive);
        }

        public void ClearWordGroupDisplayItems()
        {
            List<GameObject> wordGroupDisplayItems = new List<GameObject>();
            foreach (Transform child in contentObject.transform)
            {
                wordGroupDisplayItems.Add(child.gameObject);
            }
            for (int i = wordGroupDisplayItems.Count - 1; i >= 0; i--)
            {
                Destroy(wordGroupDisplayItems[i]);
            }
        }

        public void CreateWordGroupDisplayItem(string inGroupName, List<WordData> inWords, WordGroupsController inWordGroupsController)
        {
            GameObject wordGroupDisplayObject = Instantiate(wordGroupDisplayItemPrefab, contentObject.transform);
            WordGroupDisplayItem wordGroupDisplayItem = wordGroupDisplayObject.GetComponent<WordGroupDisplayItem>();
            wordGroupDisplayItem.Initialize(inGroupName, inWords, inWordGroupsController);
        }
    }
}