using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static ChickenScratch.PlayerBirdArm;

namespace ChickenScratch
{
    public abstract class CaseFolder : MonoBehaviour
    {
        [SerializeField]
        protected SpriteRenderer folderRenderer;

        [SerializeField]
        protected List<Image> caseTypeColouredImages;

        [SerializeField]
        protected List<TMPro.TMP_Text> caseTypeBackgroundTexts;

        [SerializeField]
        protected List<TMPro.TMP_Text> caseTypeImportantTexts;

        [SerializeField]
        protected TMPro.TMP_Text caseTypeText;

        [SerializeField]
        private List<SpriteRenderer> inFolderRenderers, outFolderRenderers;

        [SerializeField]
        private Animator formAnimator;

        [SerializeField]
        private FileStamp folderStamp;

        [SerializeField]
        private CaseModifierVisual caseModifierVisual;

        [SerializeField]
        protected CasePlayerTabs casePlayerTabs;

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
            BirdData playerBird = GameDataManager.Instance.GetBird(SettingsManager.Instance.birdName);
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

        public void SetCaseTypeVisuals(int caseID)
        {
            if(!GameManager.Instance.playerFlowManager.drawingRound.caseMap.ContainsKey(caseID))
            {
                caseTypeText.gameObject.SetActive(false);
                return;
            }
            ChainData currentCase = GameManager.Instance.playerFlowManager.drawingRound.caseMap[caseID];
            CaseChoiceData choice = GameDataManager.Instance.GetCaseChoice(currentCase.caseTypeName);

            if(choice != null)
            {
                caseTypeText.gameObject.SetActive(true);
                folderRenderer.color = choice.colour;
                caseTypeText.text = currentCase.caseTypeName.ToUpper();
                foreach(Image caseTypeImage in caseTypeColouredImages)
                {
                    caseTypeImage.color = choice.colour;
                }
                foreach (TMPro.TMP_Text backgroundText in caseTypeBackgroundTexts)
                {
                    backgroundText.color = choice.backgroundFontColour;
                }
                foreach(TMPro.TMP_Text importantText in caseTypeImportantTexts)
                {
                    importantText.color = choice.importantFontColour;
                }
            }
        }

        public void Submit()
        {
            folderStamp.onStampComplete.AddListener(Hide);
            folderStamp.StampFile();
        }

        public virtual void Hide()
        {
            folderStamp.onStampComplete.RemoveListener(Hide);
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

        public void RegisterToStampComplete(UnityAction action)
        {
            folderStamp.onStampComplete.AddListener(action);
        }

        public void DeregisterToStampComplete(UnityAction action)
        {
            folderStamp.onStampComplete.RemoveListener(action);
        }

        public void PullDownStamp()
        {
            folderStamp.PullDown();
        }
    }
}

