using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class SelectOnActivate : MonoBehaviour
    {
        public Selectable objectToSelectOnActivate;

        // Start is called before the first frame update
        private void OnEnable()
        {
            StartCoroutine(WaitForInputActivation());
        }
        public IEnumerator WaitForInputActivation()
        {
            yield return new WaitForSeconds(0.5f);

            if (objectToSelectOnActivate != null && objectToSelectOnActivate != EventSystem.current.currentSelectedGameObject)
            {
                objectToSelectOnActivate.Select();
            }

        }

    }
}