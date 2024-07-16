using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;

namespace ChickenScratch
{
    [CreateAssetMenu(fileName = "CaseUnlockStoreItem", menuName = "GameData/Create Case Unlock Store Item")]
    public class CaseUnlockStoreItemData : StoreItemData
    {
        public string caseChoiceIdentifier;
        public List<CaseUnlockStoreItemData> unlocks = new List<CaseUnlockStoreItemData>();

        public CaseUnlockStoreItemData()
        {

        }

        public override void Initialize(StoreItemData existingData)
        {
            base.Initialize(existingData);
            CaseUnlockStoreItemData existingUnlockData = (existingData as CaseUnlockStoreItemData);
            caseChoiceIdentifier = existingUnlockData.caseChoiceIdentifier;
            unlocks = existingUnlockData.unlocks;
        }

        public CaseUnlockStoreItemData(CaseUnlockStoreItemNetData netData)
        {
            CaseUnlockStoreItemData gameData = (CaseUnlockStoreItemData)GameDataManager.Instance.GetMatchingCaseUnlockStoreItem(netData.caseChoiceIdentifier);
            if(gameData != null)
            {
                cost = gameData.cost;
                itemName = gameData.itemName;
                itemDescription = gameData.itemDescription;
                caseChoiceIdentifier = gameData.caseChoiceIdentifier;
                unlocks = gameData.unlocks;
                itemImagePrefab = gameData.itemImagePrefab;
                itemType = StoreItem.StoreItemType.case_unlock;
                storeBGColour = GameDataManager.Instance.GetCaseChoice(gameData.caseChoiceIdentifier).colour;
                index = netData.index;
            }
            else
            {
                Debug.LogError("Could not find matching unlock data.");
            }

        }
    }
}
