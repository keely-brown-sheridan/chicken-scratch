using ChickenScratch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaseUpgradeStoreImageItem : StoreImageItem
{
    [SerializeField]
    private GameObject caseUpgradeBenefitPrefab;

    [SerializeField]
    private Transform caseUpgradeBenefitHolder;

    [SerializeField]
    private GameObject birdBucksVisualObject, modifierVisualObject;

    [SerializeField]
    private TMPro.TMP_Text birdBucksText, modifierText;

    public override void Initialize(StoreItemData storeItemData)
    {
        base.Initialize(storeItemData);
        CaseUpgradeStoreItemData upgradeData = storeItemData as CaseUpgradeStoreItemData;
        if (upgradeData != null)
        {
            if(upgradeData.upgradeRampData.pointsPerCorrectWordIncrease != 0 ||
                upgradeData.upgradeRampData.bonusPointsIncrease != 0)
            {
                birdBucksVisualObject.SetActive(true);
                birdBucksText.text = (upgradeData.upgradeRampData.pointsPerCorrectWordIncrease * 2 + upgradeData.upgradeRampData.bonusPointsIncrease).ToString();
            }
            if(upgradeData.upgradeRampData.modifierIncrease != 0)
            {
                modifierVisualObject.SetActive(true);
                modifierText.text = upgradeData.upgradeRampData.modifierIncrease.ToString();
            }
        }
    }
}
