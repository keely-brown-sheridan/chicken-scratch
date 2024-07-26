using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class WordGroupDisplayItem : MonoBehaviour
    {
        [SerializeField]
        private TMPro.TMP_Text wordGroupText;

        [SerializeField]
        private Image wordGroupBackgroundImage;
        private string wordGroupName = "";
        private WordGroupsController wordGroupsController;
        private List<WordData> words = new List<WordData>();


        public void Initialize(string inWordGroupName, WordGroupData.WordType wordType, List<WordData> inWords, WordGroupsController inWordGroupsController)
        {
            Color wordGroupTextColour = Color.black;
            Color wordGroupBGColour = Color.white;
            switch(wordType)
            {
                case WordGroupData.WordType.prefixes:
                    wordGroupTextColour = SettingsManager.Instance.prefixFontColour;
                    wordGroupBGColour = SettingsManager.Instance.prefixBGColour;
                    break;
                case WordGroupData.WordType.nouns:
                    wordGroupTextColour = SettingsManager.Instance.nounFontColour;
                    wordGroupBGColour = SettingsManager.Instance.nounBGColour;
                    break;
                case WordGroupData.WordType.variant:
                    wordGroupTextColour = SettingsManager.Instance.variantFontColour;
                    wordGroupBGColour = SettingsManager.Instance.variantBGColour;
                    break;
                case WordGroupData.WordType.location:
                    wordGroupTextColour = SettingsManager.Instance.locationFontColour;
                    wordGroupBGColour = SettingsManager.Instance.locationBGColour;
                    break;
            }
            wordGroupName = inWordGroupName;
            wordGroupText.text = wordGroupName;
            wordGroupText.color = wordGroupTextColour;
            wordGroupBackgroundImage.color = wordGroupBGColour;
            wordGroupsController = inWordGroupsController;
            words = inWords;
        }

        public void Review()
        {
            wordGroupsController.ReviewWordGroup(wordGroupName, words);
        }
    }
}