using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class GuessSlideContents : SlideContents
    {
        [SerializeField]
        private Image authorImage;
        [SerializeField]
        private TMP_Text authorNameText;
        [SerializeField]
        private TMP_Text prefixText, nounText;

        [SerializeField]
        private float prefixWaitDuration;
        [SerializeField]
        private float nounWaitDuration;

        [SerializeField]
        private ParticleSystem successDescriptorEffect, successNounEffect;

        [SerializeField]
        private ParticleSystem failureDescriptorEffect, failureNounEffect;

        [SerializeField]
        private Color correctColour, incorrectColour;

        private bool hasShownNoun = false;
        private bool hasShownPrefix = false;
        private float duration;
        private float timeWaiting = 0.0f;

        private bool isPrefixCorrect, isNounCorrect;

        public void Initialize(ColourManager.BirdName guesser, Dictionary<int,string> guessesMap, Dictionary<int,CaseWordData> correctWordsMap, float inDuration)
        {
            duration = inDuration;
            timeWaiting = 0f;
            Bird authorBird = ColourManager.Instance.birdMap[guesser];
            authorImage.sprite = authorBird.faceSprite;
            authorNameText.color = authorBird.colour;
            if(guessesMap.ContainsKey(1))
            {
                prefixText.text = guessesMap[1];
                isPrefixCorrect = guessesMap[1] == correctWordsMap[1].value;
                prefixText.color = isPrefixCorrect ? correctColour : incorrectColour;
            }
            if(guessesMap.ContainsKey(2))
            {
                nounText.text = guessesMap[2];
                isNounCorrect = guessesMap[2] == correctWordsMap[2].value;
                nounText.color = isNounCorrect ? correctColour : incorrectColour;
            }
        }

        private void Update()
        {
            if(active)
            {
                timeWaiting += Time.deltaTime;
                if (timeWaiting > duration)
                {
                    isComplete = true;
                }
                else if (timeWaiting * GameManager.Instance.playerFlowManager.slidesRound.slideSpeed > prefixWaitDuration &&
                    !hasShownPrefix)
                {
                    hasShownPrefix = true;
                    prefixText.gameObject.SetActive(true);
                    if (isPrefixCorrect)
                    {
                        successDescriptorEffect.gameObject.SetActive(true);
                        successDescriptorEffect.Play();
                        AudioManager.Instance.PlaySound("TimeBonus");
                    }
                    else
                    {
                        failureDescriptorEffect.gameObject.SetActive(true);
                        failureDescriptorEffect.Play();
                        AudioManager.Instance.PlaySound("TimePenalty");
                    }
                }
                else if (timeWaiting * GameManager.Instance.playerFlowManager.slidesRound.slideSpeed > nounWaitDuration &&
                        !hasShownNoun)
                {
                    hasShownNoun = true;
                    nounText.gameObject.SetActive(true);
                    if (isNounCorrect)
                    {
                        successNounEffect.gameObject.SetActive(true);
                        successNounEffect.Play();
                        AudioManager.Instance.PlaySound("TimeBonus");
                    }
                    else
                    {
                        failureNounEffect.gameObject.SetActive(true);
                        failureNounEffect.Play();
                        AudioManager.Instance.PlaySound("TimePenalty");
                    }
                }
            }
            
        }
    }
}
