using ChickenScratch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ChickenScratch.ColourManager;

public class CaseChoicePanel : MonoBehaviour
{
    [SerializeField]
    private CaseChoice choice1, choice2, choice3;

    [SerializeField]
    private GameObject caseChoiceRerollerObject;

    [SerializeField]
    private string rerollSFX;

    private CaseChoiceNetData choiceData1, choiceData2, choiceData3;
    private bool hasRerolled = false;

    public void SetChoices(CaseChoiceNetData inChoice1, CaseChoiceNetData inChoice2, CaseChoiceNetData inChoice3)
    {
        choiceData1 = inChoice1;
        choiceData2 = inChoice2;
        choiceData3 = inChoice3;
        choice1.Initialize(choiceData1);
        choice2.Initialize(choiceData2);
        choice3.Initialize(choiceData3);

        if (GameManager.Instance.playerFlowManager.HasStoreItem(StoreItem.StoreItemType.reroll) && !hasRerolled)
        {
            caseChoiceRerollerObject.SetActive(true);
        }
        if(GameManager.Instance.playerFlowManager.HasStoreItem(StoreItem.StoreItemType.case_tab))
        {
            float modifierIncreaseValue = GameManager.Instance.playerFlowManager.GetStoreItemValue(StoreItem.StoreItemType.case_tab);
            //Randomly choose one of the three choices
            int randomChoice = Random.Range(0, 3);
            switch(randomChoice)
            {
                case 0:
                    inChoice1.modifierIncreaseValue = modifierIncreaseValue;
                    choice1.SetCaseTab(inChoice1.modifierIncreaseValue);
                    break;
                case 1:
                    inChoice2.modifierIncreaseValue = modifierIncreaseValue;
                    choice2.SetCaseTab(inChoice2.modifierIncreaseValue);
                    break;
                case 2:
                    inChoice3.modifierIncreaseValue = modifierIncreaseValue;
                    choice3.SetCaseTab(inChoice3.modifierIncreaseValue);
                    break;
            }
        }

        gameObject.SetActive(true);
        hasRerolled = false;
    }

    public void Choose1()
    {
        GameManager.Instance.gameDataHandler.CmdChooseCase(SettingsManager.Instance.birdName, choiceData1);
        gameObject.SetActive(false);
        caseChoiceRerollerObject.SetActive(false);
    }

    public void Choose2()
    {
        GameManager.Instance.gameDataHandler.CmdChooseCase(SettingsManager.Instance.birdName, choiceData2);
        gameObject.SetActive(false);
        caseChoiceRerollerObject.SetActive(false);
    }

    public void Choose3()
    {
        GameManager.Instance.gameDataHandler.CmdChooseCase(SettingsManager.Instance.birdName, choiceData3);
        gameObject.SetActive(false);
        caseChoiceRerollerObject.SetActive(false);
    }

    public void Reroll()
    {
        hasRerolled = true;
        GameManager.Instance.gameDataHandler.CmdRerollCaseChoice(SettingsManager.Instance.birdName);
        caseChoiceRerollerObject.SetActive(false);
        AudioManager.Instance.PlaySound(rerollSFX);
    }
}
