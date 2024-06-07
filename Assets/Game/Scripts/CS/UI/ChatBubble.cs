using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class ChatBubble : MonoBehaviour
    {
        public float timeToStayOpen = 5.0f;
        public Image bubbleImage;
        public Text bubbleText;

        private float currentTimeOpen = 5.0f;
        private Color startingImageColor, startingTextColor;

        private void Start()
        {
            startingImageColor = bubbleImage.color;
            startingTextColor = bubbleText.color;
        }


        // Update is called once per frame
        void Update()
        {
            float alphaFactor;

            if (currentTimeOpen < timeToStayOpen)
            {
                currentTimeOpen += Time.deltaTime;

                alphaFactor = ((timeToStayOpen - currentTimeOpen) / timeToStayOpen);

                bubbleImage.color = new Color(startingImageColor.r, startingImageColor.g, startingImageColor.b, startingImageColor.a * alphaFactor);
                bubbleText.color = new Color(startingTextColor.r, startingTextColor.g, startingTextColor.b, startingTextColor.a * alphaFactor);
            }
        }

        public void resetTime()
        {
            gameObject.SetActive(true);
            currentTimeOpen = 0.0f;
        }
    }
}