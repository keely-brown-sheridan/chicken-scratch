using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ChickenScratch.ColourManager;
using System;
using System.Globalization;
using System.Linq;

namespace ChickenScratch
{
    [Serializable]
    public class DrawingLineData : DrawingVisualData
    {
        public enum LineType
        {
            Freeform, Straight, Invalid
        }
        public Color lineColour = Color.black;
        public LineType lineType = LineType.Invalid;
        public float lineSize = 0.15f;
        public BirdName author = BirdName.none;
        public List<Vector3> positions = new List<Vector3>();

        public List<Vector3> GetPositions()
        {
            List<Vector3> allPositions = new List<Vector3>();
            foreach (Vector3 position in positions)
            {
                allPositions.Add(position);
            }
            return allPositions;
        }

        public List<Vector3> GetTransformedPositions(Vector3 parentPosition, Vector3 parentScale, int numberOfPositionsToReturn)
        {
            List<Vector3> allTransformedPositions = new List<Vector3>();
            int iterator = 0;
            foreach (Vector3 position in positions)
            {
                Vector3 transformedPosition = position;
                transformedPosition = new Vector3(transformedPosition.x * parentScale.x, transformedPosition.y * parentScale.y, transformedPosition.z * parentScale.z);
                transformedPosition += parentPosition;
                allTransformedPositions.Add(transformedPosition);
                iterator++;
                if(iterator >= numberOfPositionsToReturn)
                {
                    break;
                }
            }
            return allTransformedPositions;
        }

        public void SetPositions(List<Vector3> inPositions)
        {
            positions = new List<Vector3>();
            foreach (Vector3 position in inPositions)
            {
                positions.Add(new Vector3(position.x, position.y, position.z));
            }
        }

        public bool isInitialized => _isInitialized;
        private bool _isInitialized = false;

        public int index = -1;

        public int subIndex = -1;

        public DrawingLineData()
        {

        }
        public DrawingLineData(BirdName inAuthor, string[] networkingMessageSegments)
        {
            List<Vector3> allPoints = new List<Vector3>();
            string[] points;
            string[] pointSegments;
            float x = 0, y = 0, z = 0;
            Vector3 currentPoint;

            author = inAuthor;
            index = int.TryParse(networkingMessageSegments[1], out index) ? index : -1;
            subIndex = int.TryParse(networkingMessageSegments[2], out subIndex) ? subIndex : -1;
            lineColour = Enum.TryParse(networkingMessageSegments[3], out lineColour) ? lineColour : Color.black;

            lineSize = float.Parse(networkingMessageSegments[4]);
            zDepth = int.Parse(networkingMessageSegments[5]);
            sortingOrder = int.Parse(networkingMessageSegments[6]);
            points = networkingMessageSegments[7].Split(new string[] { GameDelim.POINT }, StringSplitOptions.None);

            for (int j = 0; j < points.Length; j++)
            {
                pointSegments = points[j].Split(new string[] { GameDelim.SUB }, StringSplitOptions.None);
                if (pointSegments.Length != 3 ||
                    !float.TryParse(pointSegments[0], out x) ||
                    !float.TryParse(pointSegments[1], out y) ||
                    !float.TryParse(pointSegments[2], out z))
                {
                    //Debug.LogError("Point["+ points.ToList().IndexOf(points[j]) +"] was in invalid format.");
                    continue;
                }
                else
                {
                    currentPoint = new Vector3(float.Parse(pointSegments[0], CultureInfo.InvariantCulture), float.Parse(pointSegments[1], CultureInfo.InvariantCulture), float.Parse(pointSegments[2], CultureInfo.InvariantCulture));
                    allPoints.Add(currentPoint);
                }
            }
            positions = allPoints;

            if (author == BirdName.none)
            {
                Debug.LogError("Author was in invalid format.");
                return;
            }
            if (sortingOrder == -1)
            {
                Debug.LogError("Sorting order was in invalid format.");
                return;
            }
            _isInitialized = true;
        }

        public string GetIdentifier(int inIndex, int inSubIndex)
        {

            string identifier = "line" + GameDelim.SUBVISUAL;
            identifier += inIndex.ToString() + GameDelim.SUBVISUAL;
            identifier += inSubIndex.ToString() + GameDelim.SUBVISUAL;
            identifier += lineColour.ToString() + GameDelim.SUBVISUAL;
            identifier += lineSize.ToString() + GameDelim.SUBVISUAL;
            identifier += zDepth.ToString() + GameDelim.SUBVISUAL;
            identifier += sortingOrder.ToString() + GameDelim.SUBVISUAL;

            return identifier;
        }

        public string GetPointMessageSegment(int pointIndex)
        {
            Vector3 currentPoint = positions[pointIndex];
            string pointMessageSegment = Math.Round(currentPoint.x, 3).ToString(CultureInfo.InvariantCulture) + GameDelim.SUB;
            pointMessageSegment += Math.Round(currentPoint.y, 3).ToString(CultureInfo.InvariantCulture) + GameDelim.SUB;
            pointMessageSegment += Math.Round(currentPoint.z, 3).ToString(CultureInfo.InvariantCulture);

            return pointMessageSegment;
        }
    }
}