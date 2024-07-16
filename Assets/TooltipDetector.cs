using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipDetector : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler
{
    [SerializeField]
    private GameObject tooltipParentObject;

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipParentObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltipParentObject.SetActive(true);
    }
}
