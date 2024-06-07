using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ChickenScratch
{
    public class TimeAdjustmentVisual : MonoBehaviour
    {
        public float lifeTime = 5f;
        public TMPro.TMP_Text timeValueText;
        public Transform effectVisualTransform;

        public float newTimeValue = 0f;

        public string soundEffectName = "";

        private float timeAlive = 0.0f;

        // Update is called once per frame
        void Update()
        {
            timeAlive += Time.deltaTime;
            if (timeAlive > lifeTime)
            {
                Destroy(gameObject);
            }
        }

        public void AnimationComplete()
        {
            GameManager.Instance.playerFlowManager.currentTimeInRound = newTimeValue;
            AudioManager.Instance.PlaySound(soundEffectName);
        }
    }
}