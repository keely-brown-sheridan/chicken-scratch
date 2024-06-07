using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class TutorialButton : MonoBehaviour
    {
        public GameObject containerObject;
        public string settingName;

        [SerializeField]
        private Toggle buttonToggle;

        public void Initialize(bool startingValue)
        {
            buttonToggle.isOn = startingValue;
        }

        public void Click(bool isOn)
        {
            AudioManager.Instance.PlaySoundVariant("sfx_scan_int_click_gen");
            SettingsManager.Instance.UpdateSetting(settingName, buttonToggle.isOn);
        }
    }
}