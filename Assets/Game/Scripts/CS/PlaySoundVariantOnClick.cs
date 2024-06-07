using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ChickenScratch
{
    public class PlaySoundVariantOnClick : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField]
        private string soundVariantName;

        public void OnPointerDown(PointerEventData eventData)
        {
            AudioManager.Instance.PlaySoundVariant(soundVariantName);
        }
    }
}