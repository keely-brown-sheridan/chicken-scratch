using ChickenScratch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CaseChoicePanel : MonoBehaviour
{
    [SerializeField]
    private CaseChoice choice1, choice2, choice3;

    private CaseChoiceNetData choiceData1, choiceData2, choiceData3;

    public void SetChoices(CaseChoiceNetData inChoice1, CaseChoiceNetData inChoice2, CaseChoiceNetData inChoice3)
    {

        choiceData1 = inChoice1;
        choiceData2 = inChoice2;
        choiceData3 = inChoice3;
        choice1.Initialize(choiceData1);
        choice2.Initialize(choiceData2);
        choice3.Initialize(choiceData3);
        
        gameObject.SetActive(true);
    }

    public void Choose1()
    {
        GameManager.Instance.gameDataHandler.CmdChooseCase(SettingsManager.Instance.birdName, choiceData1);
        gameObject.SetActive(false);
    }

    public void Choose2()
    {
        GameManager.Instance.gameDataHandler.CmdChooseCase(SettingsManager.Instance.birdName, choiceData2);
        gameObject.SetActive(false);
    }

    public void Choose3()
    {
        GameManager.Instance.gameDataHandler.CmdChooseCase(SettingsManager.Instance.birdName, choiceData3);
        gameObject.SetActive(false);
    }
}
