
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
        [SerializeField]
        private Image wordGroupBackgroundImage;

        private WordGroupData wordGroup;

        private WordGroupsController wordGroupsController = null;

        public void Initialize(WordGroupData inWordGroup, WordGroupsController inWordGroupsController, bool isHost)
        {
            wordGroup = inWordGroup;

            Color wordGroupBackgroundColour = Color.white;
            Color wordGroupFontColour = Color.black;
            switch(inWordGroup.wordType)
            {
                case WordGroupData.WordType.prefixes:
                    wordGroupBackgroundColour = SettingsManager.Instance.prefixBGColour;
                    wordGroupFontColour = SettingsManager.Instance.prefixFontColour;
                    break;
                case WordGroupData.WordType.nouns:
                    wordGroupBackgroundColour = SettingsManager.Instance.nounBGColour;
                    wordGroupFontColour = SettingsManager.Instance.nounFontColour;
                    break;
            }
            wordGroupBackgroundImage.color = wordGroupBackgroundColour;
            wordGroupText.text = wordGroup.name + " - " + wordGroup.words.Count.ToString() + " Words";
            wordGroupText.color = wordGroupFontColour;
            wordGroupToggle.interactable = isHost;

            wordGroupsController = inWordGroupsController;
        }

        public void SetAsLocked()
        {
            editWordGroupButton.gameObject.SetActive(false);
            deleteWordGroupButton.gameObject.SetActive(false);
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