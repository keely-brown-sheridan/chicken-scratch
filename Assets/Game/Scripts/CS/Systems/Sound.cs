using UnityEngine.Audio;
using UnityEngine;
using System.Collections.Generic;

namespace ChickenScratch
{
    /// <summary>
    /// Used to contain the information required for playing a sound with the AudioManager.
    /// </summary>
    [System.Serializable]
    public class Sound
    {
        public enum Type
        {
            sfx, music
        }

        public string name;

        public AudioClip clip;

        [Range(0f, 1f)]
        public float volume = 1.0f;

        [Range(0.1f, 3.0f)]
        public float pitch = 1.0f;

        public bool looping;

        [HideInInspector]
        public AudioSource source;

        public Type type;
    }

    [System.Serializable]
    public class SoundVariant
    {

        public string name;
        public List<Sound> variants;

        public void PlayVariant()
        {
            int randomIndex = Random.Range(0, variants.Count);
            variants[randomIndex].source.Play();
        }

        public void StopAllVariants()
        {
            foreach (Sound variant in variants)
            {
                variant.source.Stop();
            }
        }

    }
}