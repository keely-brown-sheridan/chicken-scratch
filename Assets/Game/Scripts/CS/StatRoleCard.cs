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
            Bird roleBird = ColourManager.Instance.GetBird(bird);
            if(roleBird == null)
            {
                Debug.LogError("Could not set values for stat role card because role bird["+bird.ToString()+"] has not been mapped in the Colour Manager.");
            }
            else
            {
                faceImage.sprite = roleBird.faceSprite;
                roleText.color = roleBird.colour;
            }
            
            roleText.text = roleName;
        }
    }
}