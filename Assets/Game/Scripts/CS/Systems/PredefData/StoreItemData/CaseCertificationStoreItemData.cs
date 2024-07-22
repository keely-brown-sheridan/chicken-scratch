using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ChickenScratch
{
    [CreateAssetMenu(fileName = "CaseCertificationStoreItem", menuName = "GameData/Create Case Certification Store Item")]
    public class CaseCertificationStoreItemData : StoreItemData
    {
        public string caseChoiceIdentifier;
        public string certificationIdentifier;

        public CaseCertificationStoreItemData()
        {

        }

        public override void Initialize(StoreItemData existingData)
        {
            base.Initialize(existingData);
            CaseCertificationStoreItemData existingCertificationData = (existingData as CaseCertificationStoreItemData);
            caseChoiceIdentifier = existingCertificationData.caseChoiceIdentifier;
            certificationIdentifier = existingCertificationData.certificationIdentifier;
        }

        public CaseCertificationStoreItemData(CaseCertificationStoreItemNetData netData)
        {
            CaseCertificationStoreItemData gameData = GameDataManager.Instance.GetCertificationStoreItem();
            if (gameData != null)
            {
                cost = gameData.cost;
                itemName = gameData.itemName;

                CertificationData certification = GameDataManager.Instance.GetCertification(netData.certificationIdentifier);

                caseChoiceIdentifier = netData.caseIdentifier;
                //Set up the proper description to be reflective of what the certification will be
                itemDescription = "Adds the " + netData.certificationIdentifier + " certification to " + caseChoiceIdentifier + ":";
                if (certification != null)
                {
                    itemDescription += " \n" + certification.description;
                }
                certificationIdentifier = netData.certificationIdentifier;
                itemImagePrefab = gameData.itemImagePrefab;
                itemType = StoreItem.StoreItemType.case_certification;
                storeBGColour = GameDataManager.Instance.GetCaseChoice(netData.caseIdentifier).colour;
                index = netData.index;
            }
            else
            {
                Debug.LogError("Could not find matching certification data.");
            }

        }
    }
}
