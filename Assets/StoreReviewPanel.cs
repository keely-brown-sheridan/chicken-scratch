using ChickenScratch;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreReviewPanel : MonoBehaviour
{
    [SerializeField]
    private Transform caseTypeVisualHolder;

    [SerializeField]
    private GameObject caseTypeVisualPrefab;

    private Dictionary<string, CaseTypeReviewVisual> caseTypes = new Dictionary<string, CaseTypeReviewVisual>();

    [SerializeField]
    private TMPro.TMP_Text goalText, deadlineText, queueText;

    public void Initialize(List<CaseChoiceData> initialCaseChoices)
    {
        foreach (CaseChoiceData caseChoice in initialCaseChoices)
        {
            GameObject caseTypeVisualObject = Instantiate(caseTypeVisualPrefab, caseTypeVisualHolder);
            CaseTypeReviewVisual caseTypeVisual = caseTypeVisualObject.GetComponent<CaseTypeReviewVisual>();
            caseTypeVisual.Initialize(caseChoice);
            caseTypes.Add(caseChoice.identifier, caseTypeVisual);
        }
    }

    public void UpdateUnlocks(List<ContractCaseUnlockData> unlocks)
    {
        foreach(ContractCaseUnlockData unlock in unlocks)
        {
            CaseChoiceData caseChoice = GameDataManager.Instance.GetCaseChoice(unlock.identifier);
            if(caseChoice != null)
            {
                GameObject caseTypeVisualObject = Instantiate(caseTypeVisualPrefab, caseTypeVisualHolder);
                CaseTypeReviewVisual caseTypeVisual = caseTypeVisualObject.GetComponent<CaseTypeReviewVisual>();
                caseTypeVisual.Initialize(caseChoice);
                caseTypes.Add(caseChoice.identifier, caseTypeVisual);
            }
        }
    }

    public void UpdateValues()
    {
        int birdbucks = GameManager.Instance.playerFlowManager.GetCurrentGoal();
        float deadline = GameManager.Instance.playerFlowManager.GetTimeInDay();
        int queue = GameManager.Instance.playerFlowManager.GetCasesForDay();
        goalText.text = birdbucks.ToString();
        var ts = TimeSpan.FromSeconds(deadline);
        deadlineText.text = string.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds);
        queueText.text = queue.ToString() + " CASES";
    }


    public void TogglePanel()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    public void UpdateCertificationForCase(string identifier, string certificationIdentifier)
    {
        if (caseTypes.ContainsKey(identifier))
        {
            caseTypes[identifier].AddCertification(certificationIdentifier);
        }
    }
}
