
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class HUDDrawingColourBtn : MonoBehaviour
    {
        public SpriteRenderer fillRenderer;
        public GameObject selectedObject;
        public DrawingLineData.LineColour colour;
        public DrawingController drawingController;

        public void Select()
        {
            //drawingController.setCurrentDrawingToolColour(colour);
            drawingController.deselectColourButtons();
            selectedObject.SetActive(true);
        }
        public void Deselect()
        {
            selectedObject.SetActive(false);
        }

        public void InitializeColour()
        {
            Color currentColourValue = Color.clear;
            switch (colour)
            {
                case DrawingLineData.LineColour.Base:
                    currentColourValue = Color.black;
                    break;
                case DrawingLineData.LineColour.Colour:
                    currentColourValue = ColourManager.Instance.birdMap[SettingsManager.Instance.birdName].colour;
                    break;
                case DrawingLineData.LineColour.Light:
                    currentColourValue = ColourManager.Instance.birdMap[SettingsManager.Instance.birdName].bgColour;
                    break;
                case DrawingLineData.LineColour.Erase:
                    currentColourValue = Color.white;
                    break;
            }
            fillRenderer.color = currentColourValue;
        }
    }
}