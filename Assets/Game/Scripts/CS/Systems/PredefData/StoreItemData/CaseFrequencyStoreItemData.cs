using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ChickenScratch
{
    [CreateAssetMenu(fileName = "CasefrequencyStoreItem", menuName = "GameData/Create Case Frequency Store Item")]
    public class CaseFrequencyStoreItemData : StoreItemData
    {
        public string caseChoiceIdentifier;
        public List<StoreItemData> unlocks = new List<StoreItemData>();
        public int frequencyIncrease;

        public CaseFrequencyStoreItemData()
        {

        }

        public override void Initialize(StoreItemData existingData)
        {
            base.Initialize(existingData);
            CaseFrequencyStoreItemData existingFrequencyData = (existingData as CaseFrequencyStoreItemData);
            caseChoiceIdentifier = existingFrequencyData.caseChoiceIdentifier;
            unlocks = existingFrequencyData.unlocks;
            frequencyIncrease = existingFrequencyData.frequencyIncrease;
        }

        public CaseFrequencyStoreItemData(CaseFrequencyStoreItemNetData netData)
        {
            CaseFrequencyStoreItemData gameData = (CaseFrequencyStoreItemData)GameDataManager.Instance.GetMatchingCaseFrequencyStoreItem(netData.itemName);
            if (gameData != null)
            {
                cost = gameData.cost;
                itemName = gameData.itemName;
                itemDescription = gameData.itemDescription;
                caseChoiceIdentifier = gameData.caseChoiceIdentifier;
                unlocks = gameData.unlocks;
                frequencyIncrease = gameData.frequencyIncrease;
                itemImagePrefab = gameData.itemImagePrefab;
                itemType = StoreItem.StoreItemType.case_frequency;
                storeBGColour = GameDataManager.Instance.GetCaseChoice(gameData.caseChoiceIdentifier).colour;
                index = netData.index;
            }
            else
            {
                Debug.LogError("Could not find matching frequency data.");
            }

        }
    }
}
