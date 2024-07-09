using UnityEngine.Audio;
using System;
using UnityEngine;
using System.Collections.Generic;

namespace ChickenScratch
{
    /// <summary>
    /// Singleton used to play sounds anywhere that they're required.
    /// </summary>
    public class AudioManager : Singleton<AudioManager>
    {
        public int defaultMasterVolume => _defaultMasterVolume;
        [SerializeField]
        private int _defaultMasterVolume;

        public int defaultMusicVolume => _defaultMusicVolume;
        [SerializeField]
        private int _defaultMusicVolume;

        public int defaultEffectsVolume => _defaultEffectsVolume;
        [SerializeField]
        private int _defaultEffectsVolume;

        public List<Sound> Sounds = new List<Sound>();
        [SerializeField]
        private List<SoundVariant> SoundVariants = new List<SoundVariant>();

        private bool musicCanPlay = true;
        private float fadeDuration = 1.0f;
        private List<string> fadeSounds = new List<string>();
        private Dictionary<string, SoundFadeInfo> fadeMap = new Dictionary<string, SoundFadeInfo>();

        private void Awake()
        {
            //DontDestroyOnLoad(gameObject);

            //Loads in the sounds.
            foreach (Sound sound in Sounds)
            {
                sound.source = gameObject.AddComponent<AudioSource>();
                sound.source.clip = sound.clip;

                sound.source.volume = sound.volume;
                sound.source.pitch = sound.pitch;
                sound.source.loop = sound.looping;
            }

            foreach (SoundVariant soundVariant in SoundVariants)
            {
                foreach (Sound sound in soundVariant.variants)
                {
                    sound.source = gameObject.AddComponent<AudioSource>();
                    sound.source.clip = sound.clip;

                    sound.source.volume = sound.volume;
                    sound.source.pitch = sound.pitch;
                    sound.source.loop = sound.looping;
                }
            }

        }

        void Update()
        {
            FadeUpdate();
        }
        private void FadeUpdate()
        {
            List<string> soundsToStopFading = new List<string>();
            foreach (string fadeSound in fadeSounds)
            {
                fadeMap[fadeSound].timeFading += Time.deltaTime;
                float newVolume = fadeMap[fadeSound].originalVolume * (1 - fadeMap[fadeSound].timeFading / fadeDuration);
                fadeMap[fadeSound].sound.source.volume = newVolume;
                if (fadeMap[fadeSound].timeFading > fadeMap[fadeSound].duration)
                {
                    AudioManager.Instance.StopSound(fadeSound);
                    fadeMap[fadeSound].sound.source.volume = fadeMap[fadeSound].originalVolume;
                    soundsToStopFading.Add(fadeSound);
                }
            }
            foreach (string soundToStopFading in soundsToStopFading)
            {


                fadeMap.Remove(soundToStopFading);
                fadeSounds.Remove(soundToStopFading);
            }
        }

        //Plays a sound of corresponding name.
        //canPlayMultiple indicates whether the same sound can be played multiple times.
        public void PlaySound(string name, bool canPlayMultiple = false)
        {
            Sound selectedSound = Sounds.Find(s => s.name == name);

            if (selectedSound == null)
            {
                Debug.LogWarning("Sound: " + name + "not found.");
                return;
            }

            if (!selectedSound.source.isPlaying || canPlayMultiple)
            {
                selectedSound.source.Play();
            }
        }

        public void FadeOutSound(string name, float duration)
        {
            if (fadeMap.ContainsKey(name))
            {
                return;
            }
            fadeSounds.Add(name);
            SoundFadeInfo soundFadeInfo = new SoundFadeInfo();
            soundFadeInfo.timeFading = 0.0f;
            soundFadeInfo.sound = Sounds.Find(s => s.name == name);
            soundFadeInfo.originalVolume = soundFadeInfo.sound.source.volume;
            soundFadeInfo.duration = duration;
            fadeMap.Add(name, soundFadeInfo);
        }

        //Stops the corresponding sound from playing.
        public void StopSound(string name)
        {
            Sound selectedSound = Sounds.Find(s => s.name == name);

            if (selectedSound == null)
            {
                Debug.LogWarning("Sound: " + name + "not found.");
                return;
            }

            if (selectedSound.source.isPlaying)
            {
                selectedSound.source.Stop();
            }
        }

        public void PlaySoundVariant(string name)
        {
            SoundVariant selectedSoundVariant = SoundVariants.Find(s => s.name == name);

            if (selectedSoundVariant == null)
            {
                Debug.LogWarning("Sound Variant: " + name + "not found.");
                return;
            }

            selectedSoundVariant.PlayVariant();
        }

        public void StopSoundVariants(string name)
        {
            SoundVariant selectedSoundVariant = SoundVariants.Find(s => s.name == name);

            if (selectedSoundVariant == null)
            {
                Debug.LogWarning("Sound Variant: " + name + "not found.");
                return;
            }

            selectedSoundVariant.StopAllVariants();
        }

        //Changes the volume of all sounds.
        public void SetGameVolume(float newMasterVolume, float newMusicVolume, float newEffectVolume)
        {
            foreach (Sound sound in Sounds)
            {
                switch (sound.type)
                {
                    case Sound.Type.music:
                        sound.source.volume = newMasterVolume * newMusicVolume * sound.volume;
                        break;
                    case Sound.Type.sfx:
                        sound.source.volume = newMasterVolume * newEffectVolume * sound.volume;
                        break;
                }

            }

            foreach (SoundVariant soundVariant in SoundVariants)
            {
                foreach (Sound sound in soundVariant.variants)
                {
                    sound.source.volume = newMasterVolume * newEffectVolume * sound.volume;
                }
            }
        }

        public void SetMusicVolume(float newVolume)
        {
            foreach (Sound sound in Sounds)
            {
                if (sound.type == Sound.Type.music)
                {
                    sound.source.volume = newVolume * sound.volume;
                }

            }
        }

        public void SetEffectVolume(float newVolume)
        {
            foreach (Sound sound in Sounds)
            {
                if (sound.type == Sound.Type.sfx)
                {
                    sound.source.volume = newVolume * sound.volume;
                }
            }
            foreach (SoundVariant soundVariant in SoundVariants)
            {
                foreach (Sound sound in soundVariant.variants)
                {
                    sound.source.volume = newVolume * sound.volume;
                }
            }
        }

        //Stops all sounds from playing.
        public void StopAll()
        {
            foreach (Sound sound in Sounds)
            {
                sound.source.Stop();
            }
            foreach (SoundVariant soundVariant in SoundVariants)
            {
                foreach (Sound sound in soundVariant.variants)
                {
                    sound.source.Stop();
                }
            }
        }

        public void SetMusic(bool isOn)
        {
            musicCanPlay = isOn;
            if (isOn)
            {
                foreach (Sound sound in Sounds)
                {
                    if (sound.type == Sound.Type.music)
                    {
                        sound.source.volume = sound.volume;
                    }
                }
            }
            else
            {
                foreach (Sound sound in Sounds)
                {
                    if (sound.type == Sound.Type.music)
                    {
                        sound.source.volume = 0.0f;
                    }
                }
            }
        }

        private class SoundFadeInfo
        {
            public float timeFading = 0.0f;
            public float originalVolume = 0.0f;
            public Sound sound;
            public float duration = 1.0f;
        }
    }
}