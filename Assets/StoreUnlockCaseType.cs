using ChickenScratch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreUnlockCaseType : MonoBehaviour
{
    [SerializeField]
    private Image backgroundImage;

    [SerializeField]
    private TMPro.TMP_Text caseTypeNameText;

    [SerializeField]
    private TMPro.TMP_Text caseTypeDescriptionText;

    [SerializeField]
    private TMPro.TMP_Text baseBirdbucksText;

    [SerializeField]
    private TMPro.TMP_Text baseMultiplierText;

    public void Initialize(string identifier)
    {
        CaseChoiceData choice = GameDataManager.Instance.GetCaseChoice(identifier);
        if(choice == null)
        {
            Debug.LogError("Choice["+identifier+"] does not exist in the game data manager, cannot initialize store unlock case type.");
            return;
        }
        backgroundImage.color = choice.colour;
        caseTypeNameText.text = choice.identifier;
        caseTypeDescriptionText.text = choice.description;
        int minValue = choice.bonusPoints + choice.pointsPerCorrectWord * 2 + 2;
        int maxValue = choice.bonusPoints + choice.pointsPerCorrectWord * 2 + 10;
        baseBirdbucksText.text = (minValue).ToString() + "-" + maxValue.ToString();
        baseMultiplierText.text = choice.startingScoreModifier.ToString();
    }
}
