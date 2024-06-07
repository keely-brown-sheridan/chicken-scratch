using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class LoadingCircle : MonoBehaviour
    {
        [SerializeField]
        private float fillDuration;

        [SerializeField]
        private float holdDuration;

        [SerializeField]
        private Image fillCircleImage;

        private float timeHolding = 0.0f;
        private float timeFilling = 0.0f;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (timeFilling < fillDuration)
            {
                timeFilling += Time.deltaTime;
                fillCircleImage.fillAmount = timeFilling / fillDuration;
            }
            else
            {
                timeHolding += Time.deltaTime;
                fillCircleImage.color = new Color(fillCircleImage.color.r, fillCircleImage.color.g, fillCircleImage.color.b, 1 - timeHolding / holdDuration);
                if (timeHolding > holdDuration)
                {
                    timeFilling = 0.0f;
                    timeHolding = 0.0f;
                    fillCircleImage.fillAmount = 0.0f;
                    fillCircleImage.color = new Color(fillCircleImage.color.r, fillCircleImage.color.g, fillCircleImage.color.b, 1.0f);
                }
            }
        }
    }
}