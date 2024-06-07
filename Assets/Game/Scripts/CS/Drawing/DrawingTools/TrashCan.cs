using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class TrashCan : MonoBehaviour
    {
        public DrawingController controller;
        public GameObject clearDrawingPrompt;

        [SerializeField]
        private string useSoundName = "";

        [SerializeField]
        private string confirmSoundName = "";
        [SerializeField]
        private HideCursorOnHover drawingControllerCursorHider;

        public void showPromptToClearDrawing()
        {
            drawingControllerCursorHider.activated = false;
            AudioManager.Instance.PlaySoundVariant("sfx_game_int_garbage_select");
            controller.canDraw = false;
            clearDrawingPrompt.SetActive(true);
        }

        public void cancelClearDrawing()
        {
            AudioManager.Instance.PlaySoundVariant("sfx_game_int_garbage_cancel");
            drawingControllerCursorHider.activated = true;
            controller.canDraw = true;
            clearDrawingPrompt.SetActive(false);
        }

        public void clearDrawing()
        {
            drawingControllerCursorHider.activated = true;
            StatTracker.Instance.drawingsTrashed++;
            AudioManager.Instance.PlaySoundVariant("sfx_game_int_garbage_confirm");
            controller.canDraw = true;
            clearDrawingPrompt.SetActive(false);
            controller.clearVisuals();
        }
    }
}