using ChickenScratch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreUnlockChoice : MonoBehaviour
{
    [SerializeField]
    private Transform unlockBenefitsHolder;

    [SerializeField]
    private GameObject caseTypeUnlockPrefab;

    [SerializeField]
    private GameObject choiceButtonObject;

    public Vector3 buttonPosition => choiceButtonObject.transform.position;
    public List<string> caseTypes => _caseTypes;
    private List<string> _caseTypes = new List<string>();

    public void Initialize(List<string> inCaseTypes, ColourManager.BirdName unionRep)
    {
        _caseTypes = inCaseTypes;

        //Clear previous instances
        List<Transform> children = new List<Transform>();
        foreach(Transform child in unlockBenefitsHolder)
        {
            children.Add(child);
        }
        for(int i = children.Count - 1; i >= 0; i--)
        {
            Destroy(children[i].gameObject);
        }

        //Create instances for each case type 
        foreach(string caseType in caseTypes)
        {
            GameObject caseTypeUnlockObject = Instantiate(caseTypeUnlockPrefab, unlockBenefitsHolder);
            StoreUnlockCaseType caseTypeUnlock = caseTypeUnlockObject.GetComponent<StoreUnlockCaseType>();
            caseTypeUnlock.Initialize(caseType);
        }

        choiceButtonObject.SetActive(unionRep == SettingsManager.Instance.birdName);
    }

    public void OnChoosePressed()
    {
        Choose(true);
    }

    public void Choose(bool endRound)
    {
        GameManager.Instance.gameDataHandler.CmdChooseStoreUnlockOption(_caseTypes, endRound);
        GameManager.Instance.playerFlowManager.storeRound.HideChoiceOptionButtons();
    }

    public void HideChoiceButton()
    {
        choiceButtonObject.SetActive(false);
    }

}
