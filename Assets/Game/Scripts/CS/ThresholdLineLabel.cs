using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class ThresholdLineLabel : MonoBehaviour
    {
        [SerializeField]
        private TMPro.TMP_Text labelText;

        [SerializeField]
        private LineRenderer lineRenderer;

        [SerializeField]
        private Vector3 lineOffset;

        // Update is called once per frame
        void Update()
        {
            labelText.transform.position = Camera.main.WorldToScreenPoint(lineOffset + lineRenderer.GetPosition(0) + lineRenderer.transform.position);
        }

        public void SetLabelText(string value, Color color)
        {
            labelText.text = value;
            labelText.color = color;
        }

        public void SetLineRenderer(LineRenderer inLine)
        {
            lineRenderer = inLine;
        }
    }
}