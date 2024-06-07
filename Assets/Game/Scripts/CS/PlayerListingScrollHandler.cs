using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class PlayerListingScrollHandler : MonoBehaviour
    {

        [SerializeField]
        private ScrollRect playerListingsScrollRect;

        [SerializeField]
        private float dragVelocityThreshold;

        private bool isDragging = false;


        // Start is called before the first frame update
        void Start()
        {


        }

        // Update is called once per frame
        void Update()
        {
            bool isDragThresholdReached = playerListingsScrollRect.velocity.magnitude > dragVelocityThreshold;
            if (!isDragging && isDragThresholdReached)
            {
                isDragging = true;
                AudioManager.Instance.PlaySoundVariant("sfx_scan_int_conveyer_select");
            }
            else if (isDragging && !isDragThresholdReached)
            {
                isDragging = false;
                AudioManager.Instance.StopSoundVariants("sfx_scan_int_conveyer_select");
            }
        }
    }
}