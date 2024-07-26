using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class CaseChoice : MonoBehaviour
    {
        [SerializeField]
        private TMPro.TMP_Text titleText;

        [SerializeField]
        private TMPro.TMP_Text descriptionText;

        [SerializeField]
        private TMPro.TMP_Text prefixText;

        [SerializeField]
        private TMPro.TMP_Text nounText;

        [SerializeField]
        private Image bgImage;

        [SerializeField]
        private TMPro.TMP_Text rewardText;

        [SerializeField]
        private TMPro.TMP_Text modifierText;

        [SerializeField]
        private GameObject caseTabObject;

        [SerializeField]
        private Image caseTabFillImage;

        [SerializeField]
        private CertificationSlot certificationSlot1, certificationSlot2;

        [SerializeField]
        private GameObject costObject;

        [SerializeField]
        private TMPro.TMP_Text costText;

        [SerializeField]
        private Button chooseButton;

        [SerializeField]
        private CertificationEffectIndicator demandVisualIndicator, ballparkVisualIndicator, overheadVisualIndicator;

        private CaseChoiceData caseChoiceData;
        private int difficulty = 0;
        public int cost => _cost;
        private int _cost = 0;

        public int totalDemandReduction => _totalDemandReduction;
        private int _totalDemandReduction;

        public void Initialize(CaseChoiceNetData inCaseChoiceData, List<CaseChoiceNetData> otherChoices)
        {
            _cost = 0;
            caseChoiceData = GameDataManager.Instance.GetCaseChoice(inCaseChoiceData.caseChoiceIdentifier);
            if(caseChoiceData == null)
            {
                Debug.LogError("Could not initialize case choice because identifier["+inCaseChoiceData.caseChoiceIdentifier.ToString()+"] did not exist in the Game Data Manager.");
                return;
            }

            titleText.text = caseChoiceData.identifier;
            descriptionText.text = caseChoiceData.description;
            int totalReward = 0;
            if (inCaseChoiceData.correctWordIdentifiersMap.Count == 2)
            {
                CaseWordData prefixWord = GameDataManager.Instance.GetWord(inCaseChoiceData.correctWordIdentifiersMap[0]);
                CaseWordData nounWord = GameDataManager.Instance.GetWord(inCaseChoiceData.correctWordIdentifiersMap[1]);
                difficulty = prefixWord.difficulty + nounWord.difficulty;
                string promptValue = "";
                if(caseChoiceData.caseFormat == CaseTemplateData.CaseFormat.curveball ||
                    caseChoiceData.caseFormat == CaseTemplateData.CaseFormat.morph ||
                    caseChoiceData.caseFormat == CaseTemplateData.CaseFormat.location)
                {
                    promptValue = SettingsManager.Instance.CreateNounText(nounWord.value);
                }

                else if(caseChoiceData.caseFormat == CaseTemplateData.CaseFormat.blender)
                {
                    promptValue = SettingsManager.Instance.CreateNounText(prefixWord.value);
                }
                else
                {
                    promptValue = SettingsManager.Instance.CreatePromptText(prefixWord.value, nounWord.value, caseChoiceData.promptFormat);
                }
                prefixText.text = promptValue;
                totalReward += prefixWord.difficulty;
                totalReward += nounWord.difficulty;
            }
            else
            {
                Debug.LogError("Invalid number of correct words in the identifiers map["+inCaseChoiceData.caseChoiceIdentifier+"].");
            }
            
            nounText.text = "";
            bgImage.color = caseChoiceData.colour;

            //Check to see if the other choices have Demand
            _totalDemandReduction = 0;
            CertificationData demandCertification = GameDataManager.Instance.GetCertification("Demand");
            foreach (CaseChoiceNetData otherChoice in otherChoices)
            {
                if(demandCertification != null)
                {
                    if (GameManager.Instance.playerFlowManager.CaseHasCertification(otherChoice.caseChoiceIdentifier, "Demand"))
                    {
                        _totalDemandReduction += ((IntCertificationData)demandCertification).value;
                        inCaseChoiceData.birdbucksPerCorrectWord -= ((IntCertificationData)demandCertification).value;
                    }
                }
            }
            if(demandCertification != null && _totalDemandReduction > 0)
            {
                demandVisualIndicator.Show(demandCertification, "Reward reduced by " + _totalDemandReduction.ToString());
            }
            totalReward += (caseChoiceData.pointsPerCorrectWord - _totalDemandReduction) * 2 + caseChoiceData.bonusPoints;

            if (GameManager.Instance.playerFlowManager.CaseHasCertification(inCaseChoiceData.caseChoiceIdentifier, "Ballpark"))
            {
                CertificationData ballparkCertification = GameDataManager.Instance.GetCertification("Ballpark");
                if(ballparkCertification != null)
                {
                    ballparkVisualIndicator.Show(ballparkCertification, "Reward and Modifier are hidden");
                }
                
                rewardText.text = "??";
                modifierText.text = "??";
            }
            else
            {
                rewardText.text = totalReward.ToString();
                modifierText.text = caseChoiceData.startingScoreModifier.ToString("F2");
            }
            
            caseTabObject.SetActive(false);
            List<string> caseCertifications = GameManager.Instance.playerFlowManager.GetCaseCertifications(caseChoiceData.identifier);
            if (caseChoiceData.maxNumberOfSeals > 0)
            {
                certificationSlot1.gameObject.SetActive(true);
                certificationSlot1.Initialize(caseCertifications.Count > 0 ? caseCertifications[0] : "");
            }
            else
            {
                certificationSlot1.gameObject.SetActive(false);
            }
            if(caseChoiceData.maxNumberOfSeals > 1)
            {
                certificationSlot2.gameObject.SetActive(true);
                certificationSlot2.Initialize(caseCertifications.Count > 1 ? caseCertifications[1] : "");
            }
            else
            {
                certificationSlot2.gameObject.SetActive(false);
            }

            if(caseCertifications.Contains("Overhead"))
            {
                CertificationData overheadData = GameDataManager.Instance.GetCertification("Overhead");
                if(overheadData != null)
                {
                    overheadVisualIndicator.Show(overheadData, "Costs birdbucks upfront to choose");
                    _cost += ((OverheadCertificationData)overheadData).initialCost;
                }
            }

            if(cost > 0)
            {
                costText.text = cost.ToString();
                costObject.SetActive(true);
                if(GameManager.Instance.playerFlowManager.storeRound.currentMoney < cost)
                {
                    chooseButton.interactable = false;
                }
                else
                {
                    chooseButton.interactable = true;
                }
            }
            else
            {
                chooseButton.interactable = true;
                costObject.SetActive(false);
            }

            
        }

        public void SetCaseTab(float caseTabModifierValue)
        {
            caseTabObject.SetActive(true);
            BirdData playerBird = GameDataManager.Instance.GetBird(SettingsManager.Instance.birdName);
            if(playerBird == null)
            {
                Debug.LogError("ERROR[SetCaseTab]: Could not get player bird["+SettingsManager.Instance.birdName.ToString()+"] because it doesn't exist in the ColourManager.");
            }
            caseTabFillImage.color = playerBird.colour;
            modifierText.text = (caseChoiceData.startingScoreModifier + caseTabModifierValue).ToString("F2");
        }

        public int GetDifficulty()
        {
            return difficulty;
        }
    }
}

