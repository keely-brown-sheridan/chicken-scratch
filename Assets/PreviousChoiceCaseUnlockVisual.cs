using ChickenScratch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PreviousChoiceCaseUnlockVisual : MonoBehaviour
{
    [SerializeField]
    private Image bgImage;

    [SerializeField]
    private TMPro.TMP_Text birdBucksText;

    [SerializeField]
    private TMPro.TMP_Text multiplierText;

    [SerializeField]
    private Image tooltipBGImage;

    [SerializeField]
    private TMPro.TMP_Text caseNameText;

    [SerializeField]
    private CertificationSlot certificationSlot1, certificationSlot2;

    public void Initialize(ContractCaseUnlockData caseUnlock)
    {
        CaseChoiceData caseChoice = GameDataManager.Instance.GetCaseChoice(caseUnlock.identifier);
        if(caseChoice != null)
        {
            bgImage.color = caseChoice.importantFontColour;
            birdBucksText.text = (caseUnlock.minBirdbucks).ToString() + "-" + caseUnlock.maxBirdbucks.ToString();
            multiplierText.text = caseChoice.startingScoreModifier.ToString();
            caseNameText.text = caseChoice.identifier;
            tooltipBGImage.color = caseChoice.backgroundFontColour;

            if(caseChoice.maxNumberOfSeals > 0)
            {
                certificationSlot1.Initialize(caseUnlock.certificationIdentifier);
                certificationSlot1.gameObject.SetActive(true);
            }
            if(caseChoice.maxNumberOfSeals > 1)
            {
                certificationSlot2.gameObject.SetActive(true);
            }
        }
        

    }
}
