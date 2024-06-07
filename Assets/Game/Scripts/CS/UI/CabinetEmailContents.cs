using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class CabinetEmailContents : MonoBehaviour
    {
        public List<IndexMap> allRoundObjects;
        public Dictionary<int, GameObject> roundObjectMap;

        public bool isInitialized = false;

        void Start()
        {
            if (!isInitialized)
            {
                initialize();
            }
        }

        public void initialize()
        {
            roundObjectMap = new Dictionary<int, GameObject>();

            foreach (IndexMap roundObject in allRoundObjects)
            {
                roundObjectMap.Add(roundObject.index, roundObject.gameObject);
            }
            isInitialized = true;
        }


    }
}