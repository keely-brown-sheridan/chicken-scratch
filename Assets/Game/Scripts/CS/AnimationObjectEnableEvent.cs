using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class AnimationObjectEnableEvent : MonoBehaviour
    {
        public List<GameObject> objectsToEnableOnFire = new List<GameObject>();
        public List<GameObject> objectsToDisableOnFire = new List<GameObject>();
        public void Fire()
        {
            foreach (GameObject objectToEnable in objectsToEnableOnFire)
            {
                objectToEnable.SetActive(true);
            }
            foreach (GameObject objectToDisable in objectsToDisableOnFire)
            {
                objectToDisable.SetActive(false);
            }
        }
    }
}