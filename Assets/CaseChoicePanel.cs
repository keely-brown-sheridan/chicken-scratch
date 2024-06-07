using ChickenScratch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaseChoicePanel : MonoBehaviour
{
    [SerializeField]
    private TMPro.TMP_Text choice1TitleText, choice2TitleText;

    [SerializeField]
    private TMPro.TMP_Text choice1DescriptionText, choice2DescriptionText;

    [SerializeField]
    private TMPro.TMP_Text choice1PromptText, choice2PromptText;

    private CaseChoiceData choice1, choice2;

    public void SetChoices(CaseChoiceData inChoice1, CaseChoiceData inChoice2)
    {
        
        choice1 = inChoice1;
        choice2 = inChoice2;

        choice1TitleText.text = choice1.name;
        choice2TitleText.text = choice2.name;
        string choice1Description = "Difficulty: " + choice1.difficulty.ToString();
        choice1Description += "\nReward: " + choice1.reward.ToString();
        choice1Description += "\n" + choice1.numberOfTasks.ToString() + " tasks";
        if(choice1.cost > 0)
        {
            choice1Description += "\nCosts " + choice1.cost.ToString() + " to start";
        }
        if(choice1.penalty > 0)
        {
            choice1Description += "\nLose " + choice1.penalty.ToString() + " if not completed/incorrect";
        }
        choice1DescriptionText.text = choice1Description;
        choice1PromptText.text = "Prompt: " + choice1.correctPrompt;
        
        string choice2Description = "Difficulty: " + choice2.difficulty.ToString();
        choice2Description += "\nReward: " + choice2.reward.ToString();
        choice2Description += "\n" + choice2.numberOfTasks.ToString() + " tasks";
        if (choice2.cost > 0)
        {
            choice2Description += "\nCosts " + choice2.cost.ToString() + " to start";
        }
        if (choice2.penalty > 0)
        {
            choice2Description += "\nLose " + choice2.penalty.ToString() + " if not completed/incorrect";
        }
        choice2DescriptionText.text = choice2Description;
        choice2PromptText.text = "Prompt: " + choice2.correctPrompt;
        gameObject.SetActive(true);
    }

    public void Choose1()
    {
        GameManager.Instance.gameDataHandler.CmdChooseCase(SettingsManager.Instance.birdName, choice1);
        gameObject.SetActive(false);
    }

    public void Choose2()
    {
        GameManager.Instance.gameDataHandler.CmdChooseCase(SettingsManager.Instance.birdName, choice2);
        gameObject.SetActive(false);
    }
}
