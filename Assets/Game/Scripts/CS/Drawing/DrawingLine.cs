using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class DrawingLine : DrawingVisual
    {
        public DrawingLineData drawingLineData = new DrawingLineData();
        public bool isFading = false;

        [SerializeField]
        private float fadingTime;

        [SerializeField]
        private float fadeUpdateTime;

        private LineRenderer lineRenderer;
        private Color initialColour;
        private float timeActive = 0f;
        private float timeSinceLastFadeUpdate = 0f;

        private void Start()
        {
            lineRenderer = GetComponent<LineRenderer>();
            initialColour = lineRenderer.material.color;
        }

        private void Update()
        {
            if(isFading)
            {
                if(timeActive > fadingTime)
                {
                    return;
                }
                timeActive += Time.deltaTime;
                timeSinceLastFadeUpdate += Time.deltaTime;
                if(timeSinceLastFadeUpdate > fadeUpdateTime)
                {
                    timeSinceLastFadeUpdate = 0f;
                    Material newLineMaterial = lineRenderer.material;
                    float timeRatio = timeActive / fadingTime;
                    newLineMaterial.color = new Color(initialColour.r, initialColour.g, initialColour.b, initialColour.a * (1-timeRatio));
                    lineRenderer.material = newLineMaterial;
                }
                if (timeActive > fadingTime)
                {
                    Material newLineMaterial = lineRenderer.material;
                    float timeRatio = timeActive / fadingTime;
                    newLineMaterial.color = new Color(initialColour.r, initialColour.g, initialColour.b, 0f);
                    lineRenderer.material = newLineMaterial;
                }
            }
        }
    }
}