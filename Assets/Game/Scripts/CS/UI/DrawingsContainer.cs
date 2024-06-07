using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace ChickenScratch
{
    public class DrawingsContainer : MonoBehaviour
    {
        public GameObject drawingHolderPrefab = null;
        public Dictionary<int, Dictionary<int, DrawingHolderObject>> drawingHolderMap = new Dictionary<int, Dictionary<int, DrawingHolderObject>>();

        public bool AddDrawingHolder(int caseID, int round)
        {
            if (!drawingHolderMap.ContainsKey(caseID))
            {
                drawingHolderMap.Add(caseID, new Dictionary<int, DrawingHolderObject>());
            }
            if (!drawingHolderPrefab)
            {
                return false;
            }
            GameObject newDrawingHolderObject;
            DrawingHolderObject drawingHolder;
            if (drawingHolderMap[caseID].ContainsKey(round))
            {
                foreach (Transform child in drawingHolderMap[caseID][round].transform)
                {
                    Destroy(child.gameObject);
                }
            }
            else
            {
                newDrawingHolderObject = Instantiate(drawingHolderPrefab, transform);
                drawingHolder = newDrawingHolderObject.GetComponent<DrawingHolderObject>();
                if (!drawingHolder)
                {
                    return false;
                }
                drawingHolder.CaseID = caseID;
                drawingHolder.Round = round;
                drawingHolderMap[caseID].Add(round, drawingHolder);
            }

            return true;
        }

        public void Show(DrawingData drawingData, float scalingFactor, Vector3 drawingOffset)
        {
            if (AddDrawingHolder(drawingData.caseID, drawingData.round))
            {
                if (!drawingHolderMap.ContainsKey(drawingData.caseID))
                {
                    Debug.LogError("Drawing holder does not contain case[" + drawingData.caseID.ToString() + "].");
                    return;
                }
                if (!drawingHolderMap[drawingData.caseID].ContainsKey(drawingData.round))
                {
                    Debug.LogError("Drawing holder does not contain round[" + drawingData.round.ToString() + "] for case[" + drawingData.caseID.ToString() + "].");
                    return;
                }
                Debug.LogError("Showing drawing for case["+drawingData.caseID.ToString()+"] on round["+drawingData.round.ToString()+"].");
                Vector3 drawingScale = new Vector3(scalingFactor, scalingFactor, 1f);
                GameManager.Instance.playerFlowManager.createDrawingVisuals(drawingData, drawingHolderMap[drawingData.caseID][drawingData.round].gameObject.transform, transform.position, drawingScale, scalingFactor);
                drawingHolderMap[drawingData.caseID][drawingData.round].gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError("Failed to add drawing holder.");
                return;
            }
        }

        public void HidePreviousDrawings()
        {
            foreach (KeyValuePair<int, Dictionary<int, DrawingHolderObject>> caseHolder in drawingHolderMap)
            {
                foreach (KeyValuePair<int, DrawingHolderObject> roundHolder in caseHolder.Value)
                {
                    roundHolder.Value.gameObject.SetActive(false);
                }
            }
        }
    }
}