using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class EndlessInGameResultForm : MonoBehaviour
    {
        public Animator formAnimator;
        public TMPro.TMP_Text descriptorText, nounText, timeBonusText;
        public Image timeBonusBackgroundImage;
        public GameObject timeBonusObject;

        [SerializeField]
        private float riseDuration = 1.0f;
        private float showDuration = 2.5f;
        private float timeShowingForm = 0.0f;
        private bool hasReacted = false;
        private string reactionName = "";

        void Update()
        {
            if (timeShowingForm > 0.0f)
            {
                timeShowingForm += Time.deltaTime;

                if (timeShowingForm > showDuration)
                {
                    timeShowingForm = 0.0f;
                    AudioManager.Instance.PlaySound("sfx_game_env_boss_lower");
                    formAnimator.SetBool("Slide", false);
                }
                else if (!hasReacted && reactionName != "" && timeShowingForm > riseDuration)
                {
                    hasReacted = true;
                    formAnimator.SetBool(reactionName, true);
                }
            }
        }

        public void show(string inDescriptorValue, Color inDescriptorColour, string inNounValue, Color inNounColour, float inShowDuration, float timeBonus, string inReactionName)
        {
            if (reactionName != "")
            {
                formAnimator.SetBool(reactionName, false);
            }
            reactionName = inReactionName;
            hasReacted = false;
            descriptorText.text = inDescriptorValue;
            descriptorText.color = inDescriptorColour;
            nounText.text = inNounValue;
            nounText.color = inNounColour;
            showDuration = inShowDuration;
            timeShowingForm = Time.deltaTime;
            timeBonusObject.SetActive(timeBonus != 0);
            if (timeBonus != 0)
            {
                Color timeBonusColour = timeBonus > 0 ? Color.green : Color.red;
                timeBonusBackgroundImage.color = timeBonusColour;
                timeBonusText.text = timeBonus.ToString() + "s";
            }
            AudioManager.Instance.PlaySound("sfx_game_env_boss_rise");
            formAnimator.SetBool("Slide", true);
        }
    }
}