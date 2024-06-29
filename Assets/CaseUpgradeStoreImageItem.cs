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

    public override void Initialize(StoreItemData storeItemData)
    {
        CaseUpgradeStoreItemData upgradeData = ((CaseUpgradeStoreItemData)storeItemData);

        GameObject caseUpgradeBenefitObject;
        CaseUpgradeBenefitVisual caseUpgradeBenefit;
        if (upgradeData.correctWordPointIncrease != 0)
        {
            caseUpgradeBenefitObject = Instantiate(caseUpgradeBenefitPrefab, caseUpgradeBenefitHolder);
            caseUpgradeBenefit = caseUpgradeBenefitObject.GetComponent<CaseUpgradeBenefitVisual>();
            caseUpgradeBenefit.SetText("Correct Word: +" + upgradeData.correctWordPointIncrease.ToString());
        }
        if (upgradeData.bonusPointIncrease != 0)
        {
            caseUpgradeBenefitObject = Instantiate(caseUpgradeBenefitPrefab, caseUpgradeBenefitHolder);
            caseUpgradeBenefit = caseUpgradeBenefitObject.GetComponent<CaseUpgradeBenefitVisual>();
            caseUpgradeBenefit.SetText("Perfect Bonus: +" + upgradeData.bonusPointIncrease.ToString());
        }
        if(upgradeData.startingModifierIncrease != 0)
        {
            caseUpgradeBenefitObject = Instantiate(caseUpgradeBenefitPrefab, caseUpgradeBenefitHolder);
            caseUpgradeBenefit = caseUpgradeBenefitObject.GetComponent<CaseUpgradeBenefitVisual>();
            caseUpgradeBenefit.SetText("Base Modifier: +" + upgradeData.startingModifierIncrease.ToString());
        }
        if (upgradeData.modifierDecrementDecrease != 0)
        {
            caseUpgradeBenefitObject = Instantiate(caseUpgradeBenefitPrefab, caseUpgradeBenefitHolder);
            caseUpgradeBenefit = caseUpgradeBenefitObject.GetComponent<CaseUpgradeBenefitVisual>();
            caseUpgradeBenefit.SetText("Modifier Penalty: -" + upgradeData.modifierDecrementDecrease.ToString());
        }
        
        

        base.Initialize(storeItemData);
    }
}
