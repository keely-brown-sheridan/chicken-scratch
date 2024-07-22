using ChickenScratch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContractCaseTypeUnlock : MonoBehaviour
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

    [SerializeField]
    private CertificationSlot certificationSlot1, certificationSlot2;



    public void Initialize(ContractCaseUnlockData caseUnlockData)
    {
        CaseChoiceData choice = GameDataManager.Instance.GetCaseChoice(caseUnlockData.identifier);
        if(choice == null)
        {
            Debug.LogError("Choice["+ caseUnlockData.identifier + "] does not exist in the game data manager, cannot initialize store unlock case type.");
            return;
        }
        backgroundImage.color = choice.colour;
        caseTypeNameText.text = "Unlock\n" + choice.identifier.ToUpper();
        caseTypeDescriptionText.text = choice.description;
        
        baseBirdbucksText.text = (caseUnlockData.minBirdbucks).ToString() + "-" + caseUnlockData.maxBirdbucks.ToString();
        baseMultiplierText.text = choice.startingScoreModifier.ToString();
        certificationSlot1.gameObject.SetActive(choice.maxNumberOfSeals > 0);
        certificationSlot2.gameObject.SetActive(choice.maxNumberOfSeals > 1);

        if(caseUnlockData.certificationIdentifier != "")
        {
            certificationSlot1.Initialize(caseUnlockData.certificationIdentifier);
        }    
    }
}
