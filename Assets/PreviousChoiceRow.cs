using ChickenScratch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviousChoiceRow : MonoBehaviour
{
    [SerializeField]
    private Transform choiceHolder;

    [SerializeField]
    private GameObject choicePrefab;

    [SerializeField]
    private TMPro.TMP_Text birdbuckText;

    [SerializeField]
    private TMPro.TMP_Text timeInRoundText;

    [SerializeField]
    private TMPro.TMP_Text dayNameText;

    public void Initialize(string dayName, List<ContractCaseUnlockData> chosenContractUnlocks, int birdbuckGoal, float timeInRound)
    {
        dayNameText.text = dayName;
        birdbuckText.text = birdbuckGoal.ToString();
        timeInRoundText.text = (timeInRound/60f).ToString() + " Min";

        foreach(ContractCaseUnlockData contractUnlock in chosenContractUnlocks)
        {
            CaseChoiceData caseChoice = GameDataManager.Instance.GetCaseChoice(contractUnlock.identifier);
            if(caseChoice != null)
            {
                GameObject choiceObject = Instantiate(choicePrefab, choiceHolder);
                PreviousChoiceCaseUnlockVisual choiceVisual = choiceObject.GetComponent<PreviousChoiceCaseUnlockVisual>();
                choiceVisual.Initialize(contractUnlock);
            }
            
        }
    }
}
