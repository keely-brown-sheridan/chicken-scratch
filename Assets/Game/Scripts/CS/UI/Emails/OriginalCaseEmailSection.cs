using UnityEngine;

namespace ChickenScratch
{
    public class OriginalCaseEmailSection : CaseEmailSection
    {
        [SerializeField]
        private TMPro.TMP_Text originalPrefixText, originalNounText;


        public void Initialize(string correctPrefix, string correctNoun)
        {
            originalPrefixText.text = SettingsManager.Instance.CreatePrefixText(correctPrefix);
            originalNounText.text = SettingsManager.Instance.CreateNounText(correctNoun);
        }
    }
}
