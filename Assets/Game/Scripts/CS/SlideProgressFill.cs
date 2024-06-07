using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class SlideProgressFill : MonoBehaviour
    {
        public float requiredPoints = 0;

        [SerializeField]
        private Image bgImage;

        [SerializeField]
        private Image fillImage;

        public void SetVerticalSize(float bottom, float top)
        {
            bgImage.rectTransform.offsetMin = new Vector2(bottom, bgImage.rectTransform.offsetMin.y);
            bgImage.rectTransform.offsetMax = new Vector2(top, bgImage.rectTransform.offsetMax.y);
            fillImage.rectTransform.offsetMin = new Vector2(bottom, fillImage.rectTransform.offsetMin.y);
            fillImage.rectTransform.offsetMax = new Vector2(top, fillImage.rectTransform.offsetMax.y);
        }

        public void SetColours(Color bgColour, Color fillColour)
        {
            bgImage.color = bgColour;
            fillImage.color = fillColour;
        }

        public float GetBottom()
        {
            Vector3[] corners = new Vector3[4];
            bgImage.rectTransform.GetWorldCorners(corners);

            return corners[0].x;
        }

        public float GetTop()
        {
            Vector3[] corners = new Vector3[4];
            bgImage.rectTransform.GetWorldCorners(corners);

            return corners[2].x;
        }

        public void UpdateFillPercentage(float percentage)
        {
            fillImage.fillAmount = percentage;
        }
    }
}