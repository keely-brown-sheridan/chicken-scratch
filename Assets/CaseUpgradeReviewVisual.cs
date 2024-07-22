using ChickenScratch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CaseUpgradeReviewVisual : MonoBehaviour
{
    [SerializeField]
    private Image bgImage;

    [SerializeField]
    private TMPro.TMP_Text caseTypeText;

    [SerializeField]
    private TMPro.TMP_Text birdbucksUpgradeText, modifierUpgradeText, frequencyUpgradeText;

    [SerializeField]
    private CertificationSlot certificationSlot1, certificationSlot2;

    private int birdbucksUpgrades = 0;
    private int modifiersUpgrades = 0;
    private int frequencyUpgrades = 0;

    private int numberOfCertifications = 0;

    public void Initialize(CaseChoiceData caseChoice)
    {
        if (caseChoice != null)
        {
            bgImage.color = caseChoice.colour;
            caseTypeText.text = caseChoice.identifier;
            caseTypeText.color = caseChoice.importantFontColour;
        }

    }

    public void Initialize(ContractCaseUnlockData caseUnlock)
    {
        CaseChoiceData caseChoice = GameDataManager.Instance.GetCaseChoice(caseUnlock.identifier);
        if(caseChoice != null)
        {
            if(caseChoice.maxNumberOfSeals > 0)
            {
                if(caseUnlock.certificationIdentifier != "")
                {
                    numberOfCertifications++;
                }
                certificationSlot1.Initialize(caseUnlock.certificationIdentifier);
                certificationSlot1.gameObject.SetActive(true);
            }
            if(caseChoice.maxNumberOfSeals > 1)
            {
                certificationSlot2.gameObject.SetActive(true);
            }
            bgImage.color = caseChoice.colour;
            caseTypeText.text = caseChoice.identifier;
            caseTypeText.color = caseChoice.importantFontColour;
        }
        
    }

    public void IncreaseBirdbucksUpgrade()
    {
        birdbucksUpgrades++;
        string birdbucksUpgradeTextValue = "";
        for(int i = 0; i < birdbucksUpgrades; i++)
        {
            birdbucksUpgradeTextValue += "$";
        }
        birdbucksUpgradeText.text = birdbucksUpgradeTextValue;
    }

    public void IncreaseModifierUpgrade()
    {
        modifiersUpgrades++;
        string modifiersUpgradeTextValue = "";
        for (int i = 0; i < modifiersUpgrades; i++)
        {
            modifiersUpgradeTextValue += "*";
        }
        modifierUpgradeText.text = modifiersUpgradeTextValue;
    }

    public void IncreaseFrequencyUpgrade()
    {
        frequencyUpgrades++;
        string frequencyUpgradeTextValue = "";
        for (int i = 0; i < frequencyUpgrades; i++)
        {
            frequencyUpgradeTextValue += "+";
        }
        frequencyUpgradeText.text = frequencyUpgradeTextValue;
    }

    public void AddCertificate(string certificateIdentifier)
    {
        if(numberOfCertifications == 0)
        {
            certificationSlot1.Initialize(certificateIdentifier);
        }
        else if(numberOfCertifications == 1)
        {
            certificationSlot2.Initialize(certificateIdentifier);
        }
        numberOfCertifications++;
    }
}
