using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class CaseCertificationStoreImageItem : StoreImageItem
    {
        [SerializeField]
        private Image sealImage;

        [SerializeField]
        private Image iconImage;

        public override void Initialize(StoreItemData storeItemData)
        {
            base.Initialize(storeItemData);
            CaseCertificationStoreItemData certificationData = storeItemData as CaseCertificationStoreItemData;
            if (certificationData != null)
            {
                CertificationData certification = GameDataManager.Instance.GetCertification(certificationData.certificationIdentifier);
                if(certification != null)
                {
                    sealImage.color = certification.sealColour;
                    sealImage.sprite = certification.sealSprite;
                    iconImage.sprite = certification.iconSprite;
                }
            }
        }
    }
}

