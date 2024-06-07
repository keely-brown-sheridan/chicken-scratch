using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class AccusationFolder : MonoBehaviour
    {
        public List<IndexMap> expandedDrawings = new List<IndexMap>();
        public List<IndexMap> expandedDrawingButtons = new List<IndexMap>();

        public Canvas drawingCanvas;

        public Dictionary<int, IndexMap> expandedDrawingMap = new Dictionary<int, IndexMap>();
        public Dictionary<int, IndexMap> expandedDrawingButtonMap = new Dictionary<int, IndexMap>();
        public GameObject closeExpandedDrawingButton;
        public List<GameObject> objectsToHideOnDrawingExpand = new List<GameObject>();

        private bool isInitialized = false;

        public void initialize()
        {
            if (!isInitialized)
            {
                expandedDrawingMap.Clear();
                foreach (IndexMap expandedDrawing in expandedDrawings)
                {
                    expandedDrawingMap.Add(expandedDrawing.index, expandedDrawing);
                }

                expandedDrawingButtonMap.Clear();
                foreach (IndexMap expandedDrawingButton in expandedDrawingButtons)
                {
                    expandedDrawingButtonMap.Add(expandedDrawingButton.index, expandedDrawingButton);
                }
                isInitialized = true;
            }

        }

        public void expandDrawing(int index)
        {
            foreach (GameObject objectToHideOnDrawingExpand in objectsToHideOnDrawingExpand)
            {
                objectToHideOnDrawingExpand.SetActive(false);
            }

            foreach (KeyValuePair<int, IndexMap> expandedDrawing in expandedDrawingMap)
            {
                if (expandedDrawing.Key == index)
                {
                    expandedDrawing.Value.gameObject.SetActive(true);
                }
                else
                {
                    expandedDrawing.Value.gameObject.SetActive(false);
                }
            }

            closeExpandedDrawingButton.SetActive(true);
        }

        public void closeExpandedDrawing()
        {
            foreach (GameObject objectToHideOnDrawingExpand in objectsToHideOnDrawingExpand)
            {
                objectToHideOnDrawingExpand.gameObject.SetActive(true);
            }

            foreach (KeyValuePair<int, IndexMap> expandedDrawing in expandedDrawingMap)
            {
                expandedDrawing.Value.gameObject.SetActive(false);
            }

            closeExpandedDrawingButton.SetActive(false);
        }
    }
}