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
        private BirdImage faceImage;
        [SerializeField]
        private TMPro.TMP_Text roleText;

        public void SetValues(BirdName bird, string roleName)
        {
            BirdData roleBird = GameDataManager.Instance.GetBird(bird);
            if(roleBird == null)
            {
                Debug.LogError("Could not set values for stat role card because role bird["+bird.ToString()+"] has not been mapped in the Colour Manager.");
            }
            else
            {
                BirdHatData.HatType birdHat = GameManager.Instance.playerFlowManager.GetBirdHatType(bird);
                faceImage.Initialize(bird, birdHat);
                roleText.color = roleBird.colour;
            }
            
            roleText.text = roleName;
        }
    }
}