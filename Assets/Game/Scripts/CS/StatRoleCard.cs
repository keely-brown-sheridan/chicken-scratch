using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ChickenScratch.ColourManager;

namespace ChickenScratch
{
    public class StatRoleCard : MonoBehaviour
    {
        [SerializeField]
        private Image faceImage;
        [SerializeField]
        private TMPro.TMP_Text roleText;

        public void SetValues(BirdName bird, string roleName)
        {
            faceImage.sprite = ColourManager.Instance.birdMap[bird].faceSprite;
            roleText.color = ColourManager.Instance.birdMap[bird].colour;
            roleText.text = roleName;
        }
    }
}