using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaseUpgradeBenefitVisual : MonoBehaviour
{
    [SerializeField]
    private TMPro.TMP_Text benefitText;
    
    public void SetText(string text)
    {
        benefitText.text = text;
    }
}
