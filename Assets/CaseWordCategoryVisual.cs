using ChickenScratch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CaseWordCategoryVisual : MonoBehaviour
{
    [SerializeField]
    private TMPro.TMP_Text prefixCategoryNameText, nounCategoryNameText;

    [SerializeField]
    private Image prefixCategoryBGImage, nounCategoryBGImage;

    public void Initialize(WordCategoryData wordCategoryData)
    {
        prefixCategoryNameText.text = wordCategoryData.prefixCategory;
        nounCategoryNameText.text = wordCategoryData.nounCategory;
        prefixCategoryNameText.color = SettingsManager.Instance.prefixFontColour;
        prefixCategoryBGImage.color = SettingsManager.Instance.prefixBGColour;
        nounCategoryNameText.color = SettingsManager.Instance.nounFontColour;
        nounCategoryBGImage.color = SettingsManager.Instance.nounBGColour;

    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
