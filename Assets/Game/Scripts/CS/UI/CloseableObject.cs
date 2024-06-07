using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class CloseableObject : MonoBehaviour
    {
        public void CloseObject()
        {
            AudioManager.Instance.PlaySound("ButtonPress");
            gameObject.SetActive(false);
        }
    }
}