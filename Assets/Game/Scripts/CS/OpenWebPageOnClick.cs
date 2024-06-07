using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class OpenWebPageOnClick : MonoBehaviour
    {
        [SerializeField]
        private string webPagePath = "";

        public void OpenWebPage()
        {
            Application.OpenURL(webPagePath);
        }

    }
}