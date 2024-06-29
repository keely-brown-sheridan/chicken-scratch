using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ChickenScratch
{
    [CreateAssetMenu(fileName = "CaseUpgradeStoreItem", menuName = "GameData/Create Case Upgrade Store Item")]
    public class CaseUpgradeStoreItemData : StoreItemData
    {
        public string caseChoiceIdentifier;
        public float startingModifierIncrease;
        public float modifierDecrementDecrease;
        public int bonusPointIncrease;
        public float timeIncrease;
        public int correctWordPointIncrease;
        public List<CaseUpgradeStoreItemData> unlocks = new List<CaseUpgradeStoreItemData>();

        public CaseUpgradeStoreItemData()
        {

        }

        public CaseUpgradeStoreItemData(CaseUpgradeStoreItemNetData netData)
        {
            CaseUpgradeStoreItemData gameData = (CaseUpgradeStoreItemData)GameDataManager.Instance.GetMatchingCaseUpgradeStoreItem(netData.itemName);
            if(gameData != null)
            {
                itemName = gameData.itemName;
                itemDescription = gameData.itemDescription;
                itemImagePrefab = gameData.itemImagePrefab;
                cost = gameData.cost;
                itemType = StoreItem.StoreItemType.case_upgrade;
                caseChoiceIdentifier = gameData.caseChoiceIdentifier;
                startingModifierIncrease = gameData.startingModifierIncrease;
                modifierDecrementDecrease = gameData.modifierDecrementDecrease;
                bonusPointIncrease = gameData.bonusPointIncrease;
                timeIncrease = gameData.timeIncrease;
                correctWordPointIncrease = gameData.correctWordPointIncrease;
                unlocks = gameData.unlocks;
                storeBGColour = gameData.storeBGColour;
                index = netData.index;
            }

        }
    }
}
