using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class PromptTabHandler : MonoBehaviour
    {
        public InputField prefixInputField;
        public InputField nounInputField;

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab) &&
                prefixInputField.isFocused)
            {
                nounInputField.Select();
            }
        }
    }
}
