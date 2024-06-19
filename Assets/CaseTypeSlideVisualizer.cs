using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CaseTypeSlideVisualizer : MonoBehaviour
{
    [SerializeField]
    private Image caseTypeBGImage;

    [SerializeField]
    private TMPro.TMP_Text caseTypeNameText;



    public void Initialize(Color caseTypeColour, string caseTypeName)
    {
        caseTypeBGImage.color = caseTypeColour;
        caseTypeNameText.text = caseTypeName;
    }
}
