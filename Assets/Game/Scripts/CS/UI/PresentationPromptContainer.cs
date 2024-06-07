using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class PresentationPromptContainer : MonoBehaviour
    {
        public int cabinetID;

        public List<IndexMap> allPrompts;

        public Dictionary<int, Text> promptMap = new Dictionary<int, Text>();

        public List<IndexMap> allFaces;

        public Dictionary<int, SpriteRenderer> faceMap = new Dictionary<int, SpriteRenderer>();

        public Text guess;

        public bool isInitialized = false;

        public void initialize()
        {
            if (!isInitialized)
            {
                foreach (IndexMap prompt in allPrompts)
                {
                    if (promptMap.ContainsKey(prompt.index))
                    {
                        Debug.LogError("Promptmap for cabinet[" + cabinetID.ToString() + "] already has instance of prompt[" + prompt.index + "].");
                    }
                    promptMap.Add(prompt.index, prompt.gameObject.GetComponent<Text>());
                }

                foreach (IndexMap face in allFaces)
                {
                    faceMap.Add(face.index, face.GetComponent<SpriteRenderer>());
                }
                isInitialized = true;
            }

        }
    }
}