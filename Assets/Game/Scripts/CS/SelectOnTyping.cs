using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ChickenScratch
{
    public class SelectOnTyping : MonoBehaviour
    {
        [SerializeField]
        private TMPro.TMP_InputField inputField;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Input.anyKeyDown && inputField != null && !inputField.isFocused)
            {
                inputField.Select();
            }
        }
    }
}