using ChickenScratch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreReviewPanel : MonoBehaviour
{
    [SerializeField]
    private Transform caseUpgradeVisualHolder;

    [SerializeField]
    private Transform previousChoiceVisualHolder;

    [SerializeField]
    private GameObject caseUpgradeVisualPrefab;

    [SerializeField]
    private GameObject previousChoicePrefab;

    private Dictionary<string, CaseUpgradeReviewVisual> caseUpgrades = new Dictionary<string, CaseUpgradeReviewVisual>();

    public void Initialize(List<CaseChoiceData> initialCaseChoices)
    {
        foreach (CaseChoiceData caseChoice in initialCaseChoices)
        {
            GameObject caseUpgradeVisualObject = Instantiate(caseUpgradeVisualPrefab, caseUpgradeVisualHolder);
            CaseUpgradeReviewVisual caseUpgradeVisual = caseUpgradeVisualObject.GetComponent<CaseUpgradeReviewVisual>();
            caseUpgradeVisual.Initialize(caseChoice);
            caseUpgrades.Add(caseChoice.identifier, caseUpgradeVisual);
        }
    }

    public void AddPreviousChoice(string dayName, List<ContractCaseUnlockData> caseChoicesUnlocked, int birdbuckGoal, float timeInRound)
    {
        GameObject previousChoiceObject = Instantiate(previousChoicePrefab, previousChoiceVisualHolder);
        PreviousChoiceRow previousChoice = previousChoiceObject.GetComponent<PreviousChoiceRow>();
        previousChoice.Initialize(dayName, caseChoicesUnlocked, birdbuckGoal, timeInRound);

        foreach(ContractCaseUnlockData caseUnlock in caseChoicesUnlocked)
        {
            GameObject caseUpgradeVisualObject = Instantiate(caseUpgradeVisualPrefab, caseUpgradeVisualHolder);
            CaseUpgradeReviewVisual caseUpgradeVisual = caseUpgradeVisualObject.GetComponent<CaseUpgradeReviewVisual>();
            caseUpgradeVisual.Initialize(caseUnlock);
            caseUpgrades.Add(caseUnlock.identifier, caseUpgradeVisual);
        }
    }

    public void UpgradeBirdbucksForCase(string caseIdentifier)
    {
        if(caseUpgrades.ContainsKey(caseIdentifier))
        {
            caseUpgrades[caseIdentifier].IncreaseBirdbucksUpgrade();
        }
    }

    public void UpgradeMultiplierForCase(string caseIdentifier)
    {
        if (caseUpgrades.ContainsKey(caseIdentifier))
        {
            caseUpgrades[caseIdentifier].IncreaseModifierUpgrade();
        }
    }

    public void UpgradeFrequencyForCase(string caseIdentifier)
    {
        if (caseUpgrades.ContainsKey(caseIdentifier))
        {
            caseUpgrades[caseIdentifier].IncreaseFrequencyUpgrade();
        }
    }

    public void TogglePanel()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    public void UpdateCertificationForCase(string identifier, string certificationIdentifier)
    {
        if(caseUpgrades.ContainsKey(identifier))
        {
            caseUpgrades[identifier].AddCertificate(certificationIdentifier);
        }
    }
}
