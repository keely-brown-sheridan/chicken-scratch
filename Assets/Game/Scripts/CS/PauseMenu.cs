using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField]
        private GameObject quitPromptObject;
        [SerializeField]
        private GameObject mainPanelObject;
        [SerializeField]
        private GameObject settingsPanelObject;
        [SerializeField]
        private GameObject pauseMenuContainer;

        [SerializeField]
        private GameObject panelToOpenOnHotkeyPress;


        [SerializeField]
        private Slider masterVolumeSlider;
        [SerializeField]
        private Slider musicVolumeSlider;
        [SerializeField]
        private Slider effectsVolumeSlider;
        [SerializeField]
        private bool shouldOpenWithHotkey = true;

        public bool isOpen => _isOpen;
        private bool _isOpen = false;

        public bool canBeOpened = true;
        void Start()
        {
            LoadSettings();

        }

        void Update()
        {
            if (canBeOpened && shouldOpenWithHotkey && Input.GetKeyDown(KeyCode.Escape))
            {
                if (isOpen)
                {
                    _isOpen = false;
                    AudioManager.Instance.PlaySound("sfx_ui_int_back");
                    pauseMenuContainer.SetActive(false);
                }
                else
                {
                    _isOpen = true;
                    AudioManager.Instance.PlaySound("sfx_ui_int_gen_sel");
                    pauseMenuContainer.SetActive(true);
                }
            }
        }

        private void LoadSettings()
        {
            float masterVolume = ((float)SettingsManager.Instance.GetIntegerSetting("master_volume", 100)) / 100f;
            float musicVolume = ((float)SettingsManager.Instance.GetIntegerSetting("music_volume", 100)) / 100f;
            float effectVolume = ((float)SettingsManager.Instance.GetIntegerSetting("effect_volume", 100)) / 100f;

            //Load in the previous settings
            masterVolumeSlider.value = masterVolume;
            musicVolumeSlider.value = musicVolume;
            effectsVolumeSlider.value = effectVolume;
        }
        public void Open()
        {
            if (!isOpen)
            {
                AudioManager.Instance.PlaySound("sfx_ui_int_gen_sel");
                LoadSettings();
                _isOpen = true;
                pauseMenuContainer.SetActive(true);
            }
        }

        public void Close()
        {
            if (isOpen)
            {
                _isOpen = false;
                AudioManager.Instance.PlaySound("sfx_ui_int_back");
                pauseMenuContainer.SetActive(false);
            }
        }

        public void OpenQuitPrompt()
        {
            AudioManager.Instance.PlaySoundVariant("sfx_lobby_int_click_gen");
            quitPromptObject.SetActive(true);
        }

        public void BackQuitPrompt()
        {
            AudioManager.Instance.PlaySoundVariant("sfx_lobby_int_click_cancel_back");
            quitPromptObject.SetActive(false);
        }

        public void OpenSettingsTab()
        {
            AudioManager.Instance.PlaySoundVariant("sfx_lobby_int_click_gen");
            settingsPanelObject.SetActive(true);
            mainPanelObject.SetActive(false);
        }

        public void CloseSettingsTab()
        {
            AudioManager.Instance.PlaySoundVariant("sfx_lobby_int_click_gen");
            settingsPanelObject.SetActive(false);
            mainPanelObject.SetActive(true);
        }

        private void SetInitialMasterVolume(float newVolume)
        {
            masterVolumeSlider.value = newVolume;
            //ChangeMasterVolume(newVolume);
        }

        private void SetInitialMusicVolume(float newVolume)
        {
            musicVolumeSlider.value = newVolume;
            //ChangeMusicVolume(newVolume);
        }

        private void SetInitialEffectVolume(float newVolume)
        {
            effectsVolumeSlider.value = newVolume;
            //ChangeEffectVolume();

        }

        public void ChangeMasterVolume()
        {
            float newMasterVolume = masterVolumeSlider.value;
            int updatedValue = (int)(newMasterVolume * 100);

            SettingsManager.Instance.UpdateSetting("master_volume", updatedValue);
            AudioManager.Instance.SetGameVolume(newMasterVolume, musicVolumeSlider.value, effectsVolumeSlider.value);
        }

        public void ChangeMusicVolume()
        {
            float newVolume = musicVolumeSlider.value;
            int updatedValue = (int)(newVolume * 100);
            SettingsManager.Instance.UpdateSetting("music_volume", updatedValue);
            AudioManager.Instance.SetMusicVolume(newVolume);
        }

        public void ChangeEffectVolume()
        {
            float newVolume = effectsVolumeSlider.value;
            int updatedValue = (int)(newVolume * 100);
            SettingsManager.Instance.UpdateSetting("effect_volume", updatedValue);
            AudioManager.Instance.SetEffectVolume(newVolume);
        }
    }
}