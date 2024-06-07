using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class HonkManager : MonoBehaviour
    {
        public List<HonkObject> honkObjects;
        public List<HonkObject> nonHonkObjects;

        private bool portugeeseIsOn = false;
        private bool isFirstPress = true;

        public void InvertPortugeeseStatus()
        {
            if (isFirstPress)
            {
                //Find all HonkObject in the scene and sort them
                Object[] allHonkObjects = FindObjectsOfTypeAll(typeof(HonkObject));
                foreach (HonkObject honkObject in allHonkObjects)
                {
                    if (honkObject.isHonk)
                    {
                        honkObjects.Add(honkObject);
                    }
                    else
                    {
                        nonHonkObjects.Add(honkObject);
                    }
                }
                isFirstPress = false;
            }

            portugeeseIsOn = !portugeeseIsOn;

            if (portugeeseIsOn)
            {
                foreach (HonkObject honkObject in honkObjects)
                {
                    //If the object exists and either doesn't have a counterpart or the counterpart is active then show it
                    if (honkObject.gameObject &&
                        (!honkObject.counterpart || honkObject.counterpart.activeSelf))
                    {
                        honkObject.gameObject.SetActive(true);
                    }

                }
                foreach (HonkObject nonHonkObject in nonHonkObjects)
                {
                    //If the object exists turn it off
                    if (nonHonkObject.gameObject)
                    {
                        nonHonkObject.gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                foreach (HonkObject nonHonkObject in nonHonkObjects)
                {
                    //If the object exists and either doesn't have a counterpart or the counterpart is active then show it
                    if (nonHonkObject.gameObject &&
                        (!nonHonkObject.counterpart || nonHonkObject.counterpart.activeSelf))
                    {
                        nonHonkObject.gameObject.SetActive(true);
                    }
                }

                foreach (HonkObject honkObject in honkObjects)
                {
                    //If the object exists turn it off
                    honkObject.gameObject.SetActive(false);
                }
            }
        }
    }
}