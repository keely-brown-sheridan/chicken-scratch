using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ChickenScratch
{
    public class ToggleObjectsOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private List<GameObject> toggleOnObjects = new List<GameObject>();

        [SerializeField]
        private List<GameObject> toggleOffObjects = new List<GameObject>();

        void OnEnable()
        {
            foreach (GameObject toggleObject in toggleOnObjects)
            {
                toggleObject.SetActive(false);
            }
            foreach (GameObject toggleObject in toggleOffObjects)
            {
                toggleObject.SetActive(true);
            }
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            foreach (GameObject toggleObject in toggleOnObjects)
            {
                toggleObject.SetActive(true);
            }
            foreach (GameObject toggleObject in toggleOffObjects)
            {
                toggleObject.SetActive(false);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            foreach (GameObject toggleObject in toggleOnObjects)
            {
                toggleObject.SetActive(false);
            }
            foreach (GameObject toggleObject in toggleOffObjects)
            {
                toggleObject.SetActive(true);
            }
        }
    }
}