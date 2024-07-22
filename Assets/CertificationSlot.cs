
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class CertificationSlot : MonoBehaviour
    {
        [SerializeField]
        private Image sealImage;

        [SerializeField]
        private Image iconImage;

        [SerializeField]
        private TMPro.TMP_Text tooltipDescriptionText;

        [SerializeField]
        private GameObject tooltipDetectorObject;

        public void Initialize(string identifier)
        {
            CertificationData certificationData = GameDataManager.Instance.GetCertification(identifier);
            if (certificationData != null)
            {
                if(tooltipDescriptionText != null)
                {
                    tooltipDescriptionText.text = "Certification: " + certificationData.identifier + "\n" + certificationData.description;
                }
                
                sealImage.sprite = certificationData.sealSprite;
                sealImage.color = certificationData.sealColour;
                iconImage.sprite = certificationData.iconSprite;
                sealImage.gameObject.SetActive(true);
                iconImage.gameObject.SetActive(true);
                if(tooltipDetectorObject != null)
                {
                    tooltipDetectorObject.SetActive(true);
                }
                
            }
            else
            {
                sealImage.gameObject.SetActive(false);
                iconImage.gameObject.SetActive(false);
                if (tooltipDetectorObject != null)
                {
                    tooltipDetectorObject.SetActive(false);
                }
            }
        }
    }
}


