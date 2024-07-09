using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class TutorialSticky : MonoBehaviour, IPointerDownHandler
    {
        public Text optionA, optionB;
        public bool hasBeenPlaced = false;
        public bool hasBeenClicked = false;
        public string identifier = "";

        [SerializeField]
        private float crumplingDuration;
        [SerializeField]
        private int framesPerCrumpleUpdate;

        [SerializeField]
        private List<GameObject> crumples = new List<GameObject>();

        [SerializeField]
        private TutorialManager tutorialManager;
        [SerializeField]
        private Image baseStickyImage;
        [SerializeField]
        private List<GameObject> objectsToDisableOnCrumple = new List<GameObject>();

        private float timeCrumpling = 0.0f;
        private int framesSinceLastCrumpleUpdate = 0;
        private GameObject currentCrumpleObject;
        private int currentCrumpleIndex = 0;
        public void Awake()
        {
            if (!tutorialManager)
            {
                tutorialManager = FindObjectOfType<TutorialManager>();
            }
        }

        public void Queue(bool useOptionA)
        {
            hasBeenPlaced = true;
            optionA.gameObject.SetActive(false);
            optionB.gameObject.SetActive(false);
            Place();
            if (useOptionA)
            {
                optionA.gameObject.SetActive(true);
            }
            else
            {
                optionB.gameObject.SetActive(true);
            }
        }

        public void Click()
        {
            AudioManager.Instance.PlaySoundVariant("sfx_game_int_sticky_select");
            foreach (GameObject objectToHideOnCrumple in objectsToDisableOnCrumple)
            {
                objectToHideOnCrumple.SetActive(false);
            }
            optionA.gameObject.SetActive(false);
            optionB.gameObject.SetActive(false);
            baseStickyImage.enabled = false;
            //int randomCrumpleIndex = Random.Range(0,crumples.Count);
            currentCrumpleIndex = 0;
            currentCrumpleObject = crumples[currentCrumpleIndex];
            currentCrumpleObject.SetActive(true);
            hasBeenClicked = true;
            timeCrumpling += Time.deltaTime;
        }

        public void Place()
        {
            AudioManager.Instance.PlaySound("sfx_game_env_boss_sticky_place");

            gameObject.SetActive(true);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            StatTracker.Instance.stickyClickCount++;
            Click();
        }

        void FixedUpdate()
        {
            if (timeCrumpling > 0.0f)
            {
                timeCrumpling += Time.deltaTime;
                float progressRatio = timeCrumpling / crumplingDuration;
                //transform.localScale = new Vector3(1-progressRatio, 1-progressRatio, 1.0f);
                framesSinceLastCrumpleUpdate++;
                if (framesSinceLastCrumpleUpdate >= framesPerCrumpleUpdate)
                {
                    currentCrumpleIndex++;
                    if (currentCrumpleIndex >= crumples.Count)
                    {
                        gameObject.SetActive(false);
                        return;
                    }
                    framesSinceLastCrumpleUpdate = 0;
                    currentCrumpleObject.SetActive(false);
                    //int randomCrumpleIndex = Random.Range(0,crumples.Count);
                    currentCrumpleObject = crumples[currentCrumpleIndex];
                    currentCrumpleObject.SetActive(true);
                }
                if (timeCrumpling > crumplingDuration)
                {
                    gameObject.SetActive(false);
                }
            }
        }


    }
}