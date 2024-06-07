using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class MusicButton : MonoBehaviour
    {
        public Image musicButtonStatusImage;
        public Sprite onSprite, offSprite;
        public bool isOn = true;

        public void Click()
        {
            isOn = !isOn;
            AudioManager.Instance.SetMusic(isOn);
            musicButtonStatusImage.sprite = isOn ? onSprite : offSprite;

            SettingsManager.Instance.UpdateSetting("music", isOn);
        }
    }
}