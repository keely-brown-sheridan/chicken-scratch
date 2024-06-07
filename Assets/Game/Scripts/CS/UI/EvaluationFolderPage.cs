using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class EvaluationFolderPage : MonoBehaviour
    {
        public List<EvaluationFolderDrawing> allEvaluationFolderDrawings;
        public List<IndexMap> allEvaluationFolderButtons;
        public List<IndexMap> allExpandedDrawings;
        public List<FolderRating> allEvaluationRatings;
        public Text promptText;

        public GameObject closeExpandedDrawingButton;
        public List<GameObject> objectsToHideOnDrawingExpand = new List<GameObject>();
        public int round;
        public bool active = false;
        public Canvas evaluationDrawingCanvas;

        private bool isInitialized = false;
        private Dictionary<int, EvaluationFolderDrawing> evaluationFolderDrawingMap = new Dictionary<int, EvaluationFolderDrawing>();
        private Dictionary<int, GameObject> evaluationFolderButtonMap = new Dictionary<int, GameObject>();
        private Dictionary<int, GameObject> expandedDrawingMap = new Dictionary<int, GameObject>();
        private Dictionary<int, FolderRating> evaluationRatingMap = new Dictionary<int, FolderRating>();

        public void initialize()
        {
            if (!isInitialized)
            {
                evaluationFolderDrawingMap.Clear();
                foreach (EvaluationFolderDrawing evaluationDrawing in allEvaluationFolderDrawings)
                {
                    evaluationFolderDrawingMap.Add(evaluationDrawing.index, evaluationDrawing);
                }

                evaluationFolderButtonMap.Clear();
                foreach (IndexMap evaluationDrawingButton in allEvaluationFolderButtons)
                {
                    evaluationFolderButtonMap.Add(evaluationDrawingButton.index, evaluationDrawingButton.gameObject);
                }

                expandedDrawingMap.Clear();
                foreach (IndexMap expandedDrawing in allExpandedDrawings)
                {
                    expandedDrawingMap.Add(expandedDrawing.index, expandedDrawing.gameObject);
                }

                evaluationRatingMap.Clear();
                foreach (FolderRating evaluationRating in allEvaluationRatings)
                {
                    evaluationRatingMap.Add(evaluationRating.index, evaluationRating);
                }
                isInitialized = true;
            }

        }

        public void setPrompt(string adjective, string noun)
        {
            promptText.text = "EVALUATION:\n" + adjective + "\n" + noun;
        }

        public void expandDrawing(int index)
        {
            foreach (GameObject objectToHideOnDrawingExpand in objectsToHideOnDrawingExpand)
            {
                objectToHideOnDrawingExpand.SetActive(false);
            }

            foreach (KeyValuePair<int, GameObject> expandedDrawing in expandedDrawingMap)
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

            foreach (KeyValuePair<int, GameObject> expandedDrawing in expandedDrawingMap)
            {
                expandedDrawing.Value.gameObject.SetActive(false);
            }

            closeExpandedDrawingButton.SetActive(false);
        }
    }
}