using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class AccoladesBirdRow : MonoBehaviour
    {
        public TMPro.TMP_Text playerNameText, statRoleText, statDescriptionText;

        public float startingArmScale, endingArmScale;
        public Vector3 placementPosition;
        private Vector3 startingPosition;

        public float placingDuration;
        public float retractingDuration;

        public Transform cardHolder, armTransform;

        public ColourManager.BirdName birdName;

        public Image pinImage, birdHeadImage;

        public bool isInitialized = false;

        private float timePlacing = 0.0f;
        private float timeRetracting = 0.0f;

        public void StartPlacing(float waitDuration = 0f)
        {
            Invoke("StartPlacingAfterDelay", waitDuration);
        }

        private void StartPlacingAfterDelay()
        {
            startingPosition = cardHolder.transform.localPosition;
            timePlacing = Time.deltaTime;
        }
        void Update()
        {
            if (timePlacing > 0.0f)
            {
                timePlacing += Time.deltaTime;
                cardHolder.localPosition = (placementPosition - startingPosition) * (timePlacing / placingDuration) + startingPosition;
                float currentArmXScale = (endingArmScale - startingArmScale) * (timePlacing / placingDuration) + startingArmScale;
                armTransform.localScale = new Vector3(currentArmXScale, armTransform.localScale.y, armTransform.localScale.z);
                if (timePlacing > placingDuration)
                {
                    pinImage.gameObject.SetActive(true);
                    timePlacing = 0.0f;
                    timeRetracting = Time.deltaTime;
                }

            }
            else if (timeRetracting > 0.0f)
            {
                float currentArmXScale = (endingArmScale - startingArmScale) * (1 - timeRetracting / retractingDuration) + startingArmScale;
                armTransform.localScale = new Vector3(currentArmXScale, armTransform.localScale.y, armTransform.localScale.z);
                timeRetracting += Time.deltaTime;
                if (timeRetracting > retractingDuration)
                {
                    timeRetracting = 0.0f;
                    armTransform.gameObject.SetActive(false);
                }
            }
        }
    }
}