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

    [SerializeField]
    private GameObject parentObject;

    [SerializeField]
    private CertificationEffectIndicator assemblyIndicator;

    private CaseChoiceNetData choiceData1, choiceData2, choiceData3;
    private bool hasRerolled = false;
    private float timeChoosing = 0f;
    private int mostDifficultChoice = 0;
    private int leastDifficultChoice = 100;

    private void Update()
    {
        if(timeChoosing > 0)
        {
            timeChoosing += Time.deltaTime;
        }
    }

    public void SetChoices(CaseChoiceNetData inChoice1, CaseChoiceNetData inChoice2, CaseChoiceNetData inChoice3)
    {
        timeChoosing = Time.deltaTime;
        choiceData1 = inChoice1;
        choiceData2 = inChoice2;
        choiceData3 = inChoice3;
        choice1.Initialize(choiceData1, new List<CaseChoiceNetData>() { choiceData2, choiceData3 });
        choice2.Initialize(choiceData2, new List<CaseChoiceNetData>() { choiceData1, choiceData3 });
        choice3.Initialize(choiceData3, new List<CaseChoiceNetData>() { choiceData1, choiceData2 });

        mostDifficultChoice = choice1.GetDifficulty();
        leastDifficultChoice = mostDifficultChoice;

        int temp = choice2.GetDifficulty();
        if(temp > mostDifficultChoice)
        {
            mostDifficultChoice = temp;
        }
        else if(temp < leastDifficultChoice)
        {
            leastDifficultChoice = temp;
        }
        temp = choice3.GetDifficulty();
        if (temp > mostDifficultChoice)
        {
            mostDifficultChoice = temp;
        }
        else if (temp < leastDifficultChoice)
        {
            leastDifficultChoice = temp;
        }

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

        parentObject.SetActive(true);
        hasRerolled = false;
    }

    public void Choose1()
    {
        if(GameManager.Instance.playerFlowManager.CaseHasCertification(choiceData1.caseChoiceIdentifier, "Assembly"))
        {
            CertificationData assemblyCertification = GameDataManager.Instance.GetCertification("Assembly");
            if(assemblyCertification != null)
            {
                assemblyIndicator.Show(assemblyCertification, "+1 to frequency for " + choiceData1.caseChoiceIdentifier);
            }
        }
        GameManager.Instance.gameDataHandler.CmdChooseCase(SettingsManager.Instance.birdName, choiceData1);
        parentObject.SetActive(false);
        caseChoiceRerollerObject.SetActive(false);
        StatTracker.Instance.timeChoosing += timeChoosing;
        StatTracker.Instance.casesStarted++;
        int difficulty = choice1.GetDifficulty();
        if(difficulty < mostDifficultChoice)
        {
            StatTracker.Instance.alwaysChoseHighestDifficulty = false;
        }
        else if(difficulty > leastDifficultChoice)
        {
            StatTracker.Instance.alwaysChoseLowestDifficulty = false;
        }
        timeChoosing = 0f;

        if(choice1.cost > 0)
        {
            AudioManager.Instance.PlaySound("sale");
            GameManager.Instance.playerFlowManager.storeRound.DecreaseCurrentMoney(choice1.cost);
        }

        if (SettingsManager.Instance.GetSetting("stickies"))
        {
            UpdateStickies();
        }

    }

    public void Choose2()
    {
        GameManager.Instance.gameDataHandler.CmdChooseCase(SettingsManager.Instance.birdName, choiceData2);
        if (GameManager.Instance.playerFlowManager.CaseHasCertification(choiceData2.caseChoiceIdentifier, "Assembly"))
        {
            CertificationData assemblyCertification = GameDataManager.Instance.GetCertification("Assembly");
            if (assemblyCertification != null)
            {
                assemblyIndicator.Show(assemblyCertification, "+1 to frequency for " + choiceData2.caseChoiceIdentifier);
            }
        }
        parentObject.SetActive(false);
        caseChoiceRerollerObject.SetActive(false);
        StatTracker.Instance.timeChoosing += timeChoosing;
        StatTracker.Instance.casesStarted++;
        int difficulty = choice2.GetDifficulty();
        if (difficulty < mostDifficultChoice)
        {
            StatTracker.Instance.alwaysChoseHighestDifficulty = false;
        }
        else if (difficulty > leastDifficultChoice)
        {
            StatTracker.Instance.alwaysChoseLowestDifficulty = false;
        }
        timeChoosing = 0f;

        if (choice2.cost > 0)
        {
            AudioManager.Instance.PlaySound("sale");
            GameManager.Instance.playerFlowManager.storeRound.DecreaseCurrentMoney(choice2.cost);
        }
        if (SettingsManager.Instance.GetSetting("stickies"))
        {
            UpdateStickies();
        }
    }

    public void Choose3()
    {
        GameManager.Instance.gameDataHandler.CmdChooseCase(SettingsManager.Instance.birdName, choiceData3);
        if (GameManager.Instance.playerFlowManager.CaseHasCertification(choiceData3.caseChoiceIdentifier, "Assembly"))
        {
            CertificationData assemblyCertification = GameDataManager.Instance.GetCertification("Assembly");
            if (assemblyCertification != null)
            {
                assemblyIndicator.Show(assemblyCertification, "+1 to frequency for " + choiceData3.caseChoiceIdentifier);
            }
        }
        parentObject.SetActive(false);
        caseChoiceRerollerObject.SetActive(false);
        StatTracker.Instance.timeChoosing += timeChoosing;
        StatTracker.Instance.casesStarted++;
        int difficulty = choice3.GetDifficulty();
        if (difficulty < mostDifficultChoice)
        {
            StatTracker.Instance.alwaysChoseHighestDifficulty = false;
        }
        else if (difficulty > leastDifficultChoice)
        {
            StatTracker.Instance.alwaysChoseLowestDifficulty = false;
        }
        timeChoosing = 0f;

        if (choice3.cost > 0)
        {
            AudioManager.Instance.PlaySound("sale");
            GameManager.Instance.playerFlowManager.storeRound.DecreaseCurrentMoney(choice3.cost);
        }
        if (SettingsManager.Instance.GetSetting("stickies"))
        {
            UpdateStickies();
        }
    }

    public void SkipChoice()
    {
        GameManager.Instance.playerFlowManager.drawingRound.onPlayerSubmitTask.Invoke();
        GameManager.Instance.gameDataHandler.CmdSkipCaseChoice();
        GameManager.Instance.gameDataHandler.CmdRequestNextCase(SettingsManager.Instance.birdName);
        parentObject.SetActive(false);
        caseChoiceRerollerObject.SetActive(false);
        StatTracker.Instance.timeChoosing += timeChoosing;
        StatTracker.Instance.alwaysChoseHighestDifficulty = false;
        StatTracker.Instance.alwaysChoseLowestDifficulty = false;
        timeChoosing = 0f;
        if (SettingsManager.Instance.GetSetting("stickies"))
        {
            UpdateStickies();
        }
    }

    private void UpdateStickies()
    {
        TutorialSticky choiceSticky = GameManager.Instance.playerFlowManager.instructionRound.choicesSticky2;
        if (!choiceSticky.hasBeenClicked)
        {
            choiceSticky.Click();
        }
        choiceSticky = GameManager.Instance.playerFlowManager.instructionRound.choicesSticky3;
        if (!choiceSticky.hasBeenClicked)
        {
            choiceSticky.Click();
        }
    }

    public void Reroll()
    {
        hasRerolled = true;
        GameManager.Instance.gameDataHandler.CmdRerollCaseChoice(SettingsManager.Instance.birdName);
        caseChoiceRerollerObject.SetActive(false);
        AudioManager.Instance.PlaySound(rerollSFX);
    }
}
