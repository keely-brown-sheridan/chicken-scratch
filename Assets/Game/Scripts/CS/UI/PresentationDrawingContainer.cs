using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class PresentationDrawingContainer : MonoBehaviour
    {
        public int cabinetID;
        public List<IndexMap> allDrawings;

        public Dictionary<int, GameObject> drawingMap = new Dictionary<int, GameObject>();

        public List<IndexMap> allFaceSprites;

        public Dictionary<int, SpriteRenderer> faceMap = new Dictionary<int, SpriteRenderer>();

        public Canvas drawingCanvas;

        public void initialize()
        {
            drawingMap.Clear();

            foreach (IndexMap drawing in allDrawings)
            {
                drawingMap.Add(drawing.index, drawing.gameObject);
            }

            faceMap.Clear();
            foreach (IndexMap face in allFaceSprites)
            {
                faceMap.Add(face.index, face.GetComponent<SpriteRenderer>());
            }
        }
    }
}