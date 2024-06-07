using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ChickenScratch.ColourManager;

namespace ChickenScratch
{
    public class ViewerDrawing : MonoBehaviour
    {
        public Dictionary<string, LineRenderer> linesFromPresentor = new Dictionary<string, LineRenderer>();
        public GameObject linePrefab;

        public void reset()
        {
            foreach (LineRenderer line in linesFromPresentor.Values)
            {
                if (line.gameObject)
                {
                    Destroy(line.gameObject);
                }
            }
            linesFromPresentor.Clear();
        }

        public void addNewLine(BirdName author, string identifier, Dictionary<int, Vector3> positionMap)
        {
            GameObject newLine = Instantiate(linePrefab, transform);
            LineRenderer lineDetails = newLine.GetComponent<LineRenderer>();
            List<Vector3> positions = new List<Vector3>();
            foreach (Vector3 position in positionMap.Values)
            {
                positions.Add(position);
            }

            lineDetails.positionCount = positions.Count;
            for (int i = 0; i < positions.Count; i++)
            {
                lineDetails.SetPosition(i, positions[i]);
            }
            lineDetails.material = ColourManager.Instance.birdMap[author].material;
            lineDetails.startColor = ColourManager.Instance.birdMap[author].colour;
            lineDetails.endColor = ColourManager.Instance.birdMap[author].colour;
            lineDetails.sortingOrder = 100;
            linesFromPresentor.Add(identifier, lineDetails);
        }

        public void updateLinePositions(BirdName author, string identifier, Dictionary<int, Vector3> positionMap)
        {
            if (linesFromPresentor.ContainsKey(identifier))
            {
                List<Vector3> positions = new List<Vector3>();
                foreach (KeyValuePair<int, Vector3> position in positionMap)
                {
                    positions.Add(position.Value);
                }
                linesFromPresentor[identifier].positionCount = positions.Count;
                for (int i = 0; i < positions.Count; i++)
                {
                    linesFromPresentor[identifier].SetPosition(i, positions[i]);
                }


            }
            else
            {
                addNewLine(author, identifier, positionMap);
            }
        }
    }
}