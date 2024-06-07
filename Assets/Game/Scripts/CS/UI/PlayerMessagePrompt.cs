using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class PlayerMessagePrompt : MonoBehaviour
    {
        public Text promptText;

        public void Activate(string inText)
        {
            promptText.text = inText;
            gameObject.SetActive(true);
        }

        public void ClosePrompt()
        {
            gameObject.SetActive(false);
        }
    }
}