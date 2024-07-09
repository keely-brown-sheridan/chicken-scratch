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
        base.Initialize(storeItemData);
    }
}
