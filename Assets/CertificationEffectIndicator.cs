using ChickenScratch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CertificationEffectIndicator : MonoBehaviour
{
    [SerializeField]
    private Image sealImage;

    [SerializeField]
    private Image iconImage;

    [SerializeField]
    private TMPro.TMP_Text effectDescriptionText;

    [SerializeField]
    private float timeToShowEffect;

    private float timeShowingEffect;

    public void Show(CertificationData certification, string effectDescription, string effectSFX = "")
    {
        timeShowingEffect = 0f;
        effectDescriptionText.text = effectDescription;
        sealImage.sprite = certification.sealSprite;
        sealImage.color = certification.sealColour;
        iconImage.sprite = certification.iconSprite;
        if(effectSFX != "")
        {
            AudioManager.Instance.PlaySound(effectSFX);
        }
        gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        timeShowingEffect += Time.deltaTime;
        float timeRatio = timeShowingEffect / timeToShowEffect;
        if(timeRatio > 1)
        {
            gameObject.SetActive(false);
        }
        sealImage.color = new Color(sealImage.color.r, sealImage.color.g, sealImage.color.b, 1-timeRatio);
        iconImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, 1-timeRatio);
        effectDescriptionText.color = new Color(effectDescriptionText.color.r, effectDescriptionText.color.g, effectDescriptionText.color.b, 1 - timeRatio);
    }
}
