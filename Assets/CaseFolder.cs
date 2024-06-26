using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static ChickenScratch.PlayerBirdArm;

namespace ChickenScratch
{
    public abstract class CaseFolder : MonoBehaviour
    {
        [SerializeField]
        private List<SpriteRenderer> inFolderRenderers, outFolderRenderers;

        [SerializeField]
        private Animator formAnimator;

        [SerializeField]
        private FileStamp folderStamp;

        [SerializeField]
        private CaseModifierVisual caseModifierVisual;

        private bool isActive = false;
        private bool isStampActive = false;
        protected UnityAction timeCompleteAction;

        void Update()
        {
            if(isActive && !isStampActive)
            {
                folderStamp.SetAsActive();
                isStampActive = true;
            }
            
        }

        public virtual void Show(Color inFolderColour, float timeForTask, float currentModifierValue, float maxModifierValue, float modifierDecrement)
        {
            isActive = true;
            if (SettingsManager.Instance.GetSetting("stickies") &&
                !GameManager.Instance.playerFlowManager.instructionRound.hasClickedFirstCabinet)
            {
                GameManager.Instance.playerFlowManager.instructionRound.hideCabinetStickies();
            }

            foreach (SpriteRenderer drawingFolderRenderer in inFolderRenderers)
            {
                drawingFolderRenderer.color = inFolderColour;
            }
            Bird playerBird = ColourManager.Instance.GetBird(SettingsManager.Instance.birdName);
            if(playerBird == null)
            {
                Debug.LogError("Could not map colours for the folder renderers because bird["+SettingsManager.Instance.birdName+"] does not exist in the ColourManager.");
            }
            else
            {
                Color folderColour = playerBird.folderColour;
                foreach (SpriteRenderer drawingFolderRenderer in outFolderRenderers)
                {
                    drawingFolderRenderer.color = folderColour;
                }
            }


            formAnimator.SetBool("Slide", true);
            caseModifierVisual.Initialize(timeForTask, currentModifierValue, maxModifierValue, modifierDecrement);
            
        }

        public virtual void Hide()
        {
            folderStamp.SetAsResting();
            formAnimator.SetBool("Slide", false);
            isActive = false;
            isStampActive = false;
            if(timeCompleteAction != null)
            {
                DeregisterFromTimer(timeCompleteAction);
            }
        }

        public abstract bool HasStarted();

        public void RegisterToTimer(UnityAction action)
        {
            caseModifierVisual.onTimeComplete.AddListener(action);
        }

        public void DeregisterFromTimer(UnityAction action)
        {
            caseModifierVisual.onTimeComplete.RemoveListener(action);
        }

        public float GetScoreModifier()
        {
            return caseModifierVisual.GetFinalModifierValue();
        }
    }
}

