
using System;
using System.Collections.Generic;
using UnityEngine;
using static ChickenScratch.ColourManager;

namespace ChickenScratch
{
    public class TutorialSequence : MonoBehaviour
    {
        public List<string> audioClipsToStopOnStart;

        [Serializable]
        public class TutorialSlide
        {
            public List<GameObject> objectsToShow;
            public List<GameObject> objectsToHide;
            public float timeToShow;
            public string audioToPlay;

            public void start()
            {
                AudioManager.Instance.PlaySound(audioToPlay);
                foreach (GameObject objectToShow in objectsToShow)
                {
                    objectToShow.SetActive(true);
                }
                foreach (GameObject objectToHide in objectsToHide)
                {
                    objectToHide.SetActive(false);
                }
            }
        }

        public List<GameObject> objectsToHideOnStart;
        public List<TutorialSlide> slides;

        public bool active = false;
        private float timeShowingCurrentSlide = 0.0f;
        private int currentSlideIndex = 0;

        // Start is called before the first frame update
        void Start()
        {
            //startSlides();
        }

        public void startSlides()
        {
            if (SettingsManager.Instance.isHost)
            {
                foreach (BirdName bird in GameManager.Instance.gameFlowManager.gamePlayers.Keys)
                {
                    if (!GameManager.Instance.gameFlowManager.disconnectedPlayers.Contains(bird) && bird != SettingsManager.Instance.birdName)
                    {
                        //Debug.LogError("Adding tutorial_finished condition.");
                        GameManager.Instance.gameFlowManager.addTransitionCondition("tutorial_finished:" + bird);
                    }
                }
            }

            foreach (GameObject objectToHideOnStart in objectsToHideOnStart)
            {
                objectToHideOnStart.SetActive(false);
            }
            foreach (string audioClipToStopOnStart in audioClipsToStopOnStart)
            {
                AudioManager.Instance.StopSound(audioClipToStopOnStart);
            }

            currentSlideIndex = 0;
            timeShowingCurrentSlide = 0.0f;

            slides[currentSlideIndex].start();
            gameObject.SetActive(true);
            active = true;
            AudioManager.Instance.PlaySound("Tutorial");
        }

        // Update is called once per frame
        void Update()
        {
            if (active)
            {
                timeShowingCurrentSlide += Time.deltaTime;
                if (timeShowingCurrentSlide > slides[currentSlideIndex].timeToShow)
                {
                    timeShowingCurrentSlide = 0.0f;
                    currentSlideIndex++;
                    if (slides.Count > currentSlideIndex)
                    {
                        slides[currentSlideIndex].start();
                    }
                    else
                    {
                        active = false;
                        AudioManager.Instance.StopSound("Tutorial");
                        if (!SettingsManager.Instance.isHost)
                        {
                            GameManager.Instance.gameDataHandler.CmdTransitionCondition("tutorial_finished:" + SettingsManager.Instance.birdName);
                        }
                    }
                }
            }
        }
    }
}