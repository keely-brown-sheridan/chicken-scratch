using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class FadeObject : MonoBehaviour
    {
        public enum FadeState
        {
            inactive, waiting, fading
        }
        public FadeState state = FadeState.inactive;

        public List<Image> imagesToFade;
        public List<Text> textsToFade;

        public float timeBeforeFading;
        public float timeToFade;

        private float timeBeforeTransition = 0.0f;

        public void showObject()
        {
            state = FadeState.waiting;
            timeBeforeTransition = timeBeforeFading;

            foreach (Image imageToFade in imagesToFade)
            {
                imageToFade.color = new Color(imageToFade.color.r, imageToFade.color.g, imageToFade.color.b, 1);
                imageToFade.gameObject.SetActive(true);
            }

            foreach (Text textToFade in textsToFade)
            {
                textToFade.color = new Color(textToFade.color.r, textToFade.color.g, textToFade.color.b, 1);
                textToFade.gameObject.SetActive(true);
            }
        }

        void Update()
        {
            if (timeBeforeTransition > 0)
            {
                timeBeforeTransition -= Time.deltaTime;

                switch (state)
                {
                    case FadeState.fading:
                        foreach (Image imageToFade in imagesToFade)
                        {
                            imageToFade.color = new Color(imageToFade.color.r, imageToFade.color.g, imageToFade.color.b, timeToFade / timeBeforeTransition);
                        }

                        foreach (Text textToFade in textsToFade)
                        {
                            textToFade.color = new Color(textToFade.color.r, textToFade.color.g, textToFade.color.b, timeToFade / timeBeforeTransition);
                        }

                        break;
                }
            }
            else
            {
                switch (state)
                {
                    case FadeState.waiting:
                        state = FadeState.fading;
                        timeBeforeTransition = timeToFade;
                        break;
                    case FadeState.fading:
                        foreach (Image imageToFade in imagesToFade)
                        {
                            imageToFade.gameObject.SetActive(false);
                        }

                        foreach (Text textToFade in textsToFade)
                        {
                            textToFade.gameObject.SetActive(false);
                        }
                        state = FadeState.inactive;
                        break;
                }
            }
        }
    }
}