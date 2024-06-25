using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ChickenScratch.DrawingLineData;

namespace ChickenScratch
{
    public class Drawing : MonoBehaviour
    {
        List<GameObject> drawingObjects = new List<GameObject>();
        public List<GameObject> CreateDrawingsFromVisuals(List<DrawingLineData> drawingVisuals, Vector3 position, float thicknessReductionFactor)
        {
            Material drawingMaterial;
            GameObject drawingPrefab = ColourManager.Instance.linePrefab;
            float lineThickness = -1;
            foreach (DrawingLineData visual in drawingVisuals)
            {
                DrawingLineData line = (DrawingLineData)visual;
                drawingMaterial = ColourManager.Instance.baseLineMaterial;

                lineThickness = line.lineSize * thicknessReductionFactor;
                if (lineThickness == -1)
                {
                    Debug.LogError("Invalid line thickness name[" + line.lineSize + "], could not create drawing.");
                    return new List<GameObject>();
                }

                GameObject newLineObject = Instantiate(drawingPrefab, Vector3.zero, Quaternion.identity, transform);
                
                drawingObjects.Add(newLineObject);
                LineRenderer newLineRenderer = newLineObject.GetComponent<LineRenderer>();
                DrawingLine drawingLine = newLineObject.GetComponent<DrawingLine>();
                drawingLine.drawingLineData = visual;

                newLineRenderer.material = drawingMaterial;
                newLineRenderer.positionCount = line.positions.Count;
                newLineRenderer.material.color = line.lineColour;

                int iterator = 0;
                
                foreach(Vector3 linePosition in line.GetPositions())
                {
                    newLineRenderer.SetPosition(iterator, linePosition + position );
                    iterator++;
                }
                //newLineRenderer.SetPositions(line.GetPositions().ToArray());
                newLineObject.transform.position = new Vector3(newLineObject.transform.position.x, newLineObject.transform.position.y, line.zDepth);
                newLineRenderer.startWidth = lineThickness;
                newLineRenderer.endWidth = lineThickness;
            }

            return drawingObjects;
        }

        public List<DrawingLineData> GetVisualsFromDrawing(Vector3 transformPosition)
        {
            List<DrawingLineData> drawingVisuals = new List<DrawingLineData>();
            DrawingLine currentDrawingLine;
            List<Vector3> points;
            Vector3 pointToAdd;

            for (int i = drawingObjects.Count - 1; i >= 0; i--)
            {
                currentDrawingLine = drawingObjects[i].GetComponent<DrawingLine>();
                Vector3 drawingObjectPosition = drawingObjects[i].transform.position;
                currentDrawingLine.drawingLineData.objectPosition = new Vector3(drawingObjectPosition.x, drawingObjectPosition.y, drawingObjectPosition.z);
                points = new List<Vector3>();
                LineRenderer currentLineRenderer = drawingObjects[i].GetComponent<LineRenderer>();
                //currentLineRenderer.Simplify(0.1f);
                for (int j = 0; j < currentLineRenderer.positionCount; j++)
                {
                    pointToAdd = currentLineRenderer.GetPosition(j);
                    //Subtract the world position so that it isn't based on where the drawing is when it's being drawn
                    points.Add(pointToAdd - transformPosition);
                }
                currentDrawingLine.drawingLineData.SetPositions(points);
                drawingVisuals.Add(currentDrawingLine.drawingLineData);

            }

            return drawingVisuals;
        }

        public void AddShape(GameObject shapeObject)
        {
            drawingObjects.Add(shapeObject);
            //shapeObject.transform.parent = transform;
        }

        public void UndoLastVisual()
        {
            if (drawingObjects.Count > 0)
            {
                if (drawingObjects[drawingObjects.Count - 1] != null)
                {
                    Destroy(drawingObjects[drawingObjects.Count - 1]);
                    AudioManager.Instance.PlaySound("EraseLine");
                }

                drawingObjects.RemoveAt(drawingObjects.Count - 1);
            }
        }

        public bool HasVisuals()
        {
            return drawingObjects.Count > 0;
        }

        public void Clear()
        {
            Debug.LogError("Clearing drawing.");
            for (int i = drawingObjects.Count - 1; i >= 0; i--)
            {
                Destroy(drawingObjects[i].gameObject);
            }
            drawingObjects.Clear();
        }
    }
}