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
        public CaseUpgradeRampValues upgradeRampData;
        public string caseChoiceIdentifier;
        public float modifierDecrementDecrease;
        public float timeIncrease;

        public List<StoreItemData> unlocks = new List<StoreItemData>();

        public CaseUpgradeStoreItemData()
        {

        }

        public override void Initialize(StoreItemData existingItemData)
        {
            base.Initialize(existingItemData);
            CaseUpgradeStoreItemData upgradeData = existingItemData as CaseUpgradeStoreItemData;
            upgradeRampData = upgradeData.upgradeRampData;
            caseChoiceIdentifier = upgradeData.caseChoiceIdentifier;
            modifierDecrementDecrease = upgradeData.modifierDecrementDecrease;
            timeIncrease = upgradeData.timeIncrease;
            unlocks = upgradeData.unlocks;
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
                upgradeRampData = gameData.upgradeRampData;
                modifierDecrementDecrease = gameData.modifierDecrementDecrease;
                timeIncrease = gameData.timeIncrease;
                unlocks = gameData.unlocks;
                storeBGColour = GameDataManager.Instance.GetCaseChoice(gameData.caseChoiceIdentifier).colour;
                index = netData.index;
            }

        }
    }
}
