using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ChickenScratch
{
    public class DrawingToolHoverGlow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public GameObject hoverGlowObject;
        public void setAsUnhovered()
        {
            hoverGlowObject.SetActive(false);
        }
        public void setAsHovered()
        {
            hoverGlowObject.SetActive(true);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            setAsHovered();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            setAsUnhovered();
        }
    }
}