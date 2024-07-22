using ChickenScratch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContractChoice : MonoBehaviour
{
    [SerializeField]
    private Transform unlockBenefitsHolder;

    [SerializeField]
    private GameObject caseTypeUnlockPrefab;

    [SerializeField]
    private GameObject choiceButtonObject;

    [SerializeField]
    private TMPro.TMP_Text goalText;

    [SerializeField]
    private TMPro.TMP_Text deadlineText;

    public Vector3 buttonPosition => choiceButtonObject.transform.position;

    public StoreChoiceOptionData storeChoiceOption => _storeChoiceOption;
    private StoreChoiceOptionData _storeChoiceOption;

    public bool hasChosen => _hasChosen;
    private bool _hasChosen = false;

    public void Initialize(ColourManager.BirdName unionRep, StoreChoiceOptionData inStoreChoiceOption)
    {
        _hasChosen = false;
        _storeChoiceOption = inStoreChoiceOption;

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

        deadlineText.text = "+" + _storeChoiceOption.timeRamp.ToString() + "s";
        goalText.text = ((int)(_storeChoiceOption.birdbucksPerPlayer * SettingsManager.Instance.GetPlayerNameCount())).ToString();

        //Create instances for each case type 
        foreach(ContractCaseUnlockData caseUnlockData in inStoreChoiceOption.unlocks)
        {
            GameObject caseTypeUnlockObject = Instantiate(caseTypeUnlockPrefab, unlockBenefitsHolder);
            ContractCaseTypeUnlock caseTypeUnlock = caseTypeUnlockObject.GetComponent<ContractCaseTypeUnlock>();
            caseTypeUnlock.Initialize(caseUnlockData);
        }

        choiceButtonObject.SetActive(unionRep == SettingsManager.Instance.birdName);
    }

    public void OnChoosePressed()
    {
        Choose(true);
    }

    public void Choose(bool endRound)
    {
        _hasChosen = true;
        GameManager.Instance.gameDataHandler.CmdChooseContract(_storeChoiceOption, endRound);
        GameManager.Instance.playerFlowManager.storeRound.HideChoiceOptionButtons();
    }

    public void HideChoiceButton()
    {
        choiceButtonObject.SetActive(false);
    }

}
