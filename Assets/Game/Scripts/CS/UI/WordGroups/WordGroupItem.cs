
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class WordGroupItem : MonoBehaviour
    {
        [SerializeField]
        private Text wordGroupText;
        [SerializeField]
        private Toggle wordGroupToggle;
        [SerializeField]
        private Button editWordGroupButton;
        [SerializeField]
        private Button deleteWordGroupButton;

        private WordGroupData wordGroup;

        private WordGroupsController wordGroupsController = null;

        public void Initialize(WordGroupData inWordGroup, WordGroupsController inWordGroupsController, bool isHost)
        {
            wordGroup = inWordGroup;

            wordGroupText.text = wordGroup.name + "[" + wordGroup.wordType.ToString() + "] - " + wordGroup.words.Count.ToString() + " Words";
            wordGroupToggle.interactable = isHost;

            wordGroupsController = inWordGroupsController;
        }

        public void SetAsLocked()
        {
            editWordGroupButton.interactable = false;
            deleteWordGroupButton.interactable = false;
        }

        public void Toggle()
        {
            wordGroupsController.ToggleWordGroup(wordGroup.name, wordGroupToggle.isOn);
        }

        public void Edit()
        {
            wordGroupsController.EditWordGroup(wordGroup.name);
        }

        public void Delete()
        {
            wordGroupsController.DeleteWordGroup(wordGroup.name);
            Destroy(gameObject);
        }

        public bool IsOn()
        {
            return wordGroupToggle.isOn;
        }
    }
}