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
        private Transform taskHolder;

        [SerializeField]
        private GameObject taskPrefab;

        [SerializeField]
        private TMPro.TMP_Text rewardText;

        [SerializeField]
        private TMPro.TMP_Text modifierText;

        [SerializeField]
        private GameObject caseTabObject;

        [SerializeField]
        private Image caseTabFillImage;

        private CaseChoiceData caseChoiceData;

        public void Initialize(CaseChoiceNetData inCaseChoiceData)
        {
            caseChoiceData = GameDataManager.Instance.GetCaseChoice(inCaseChoiceData.caseChoiceIdentifier);
            titleText.text = caseChoiceData.identifier;
            descriptionText.text = caseChoiceData.description;
            int totalReward = 0;
            if (inCaseChoiceData.correctWordIdentifiersMap.Count == 2)
            {
                CaseWordData prefixWord = GameDataManager.Instance.GetWord(inCaseChoiceData.correctWordIdentifiersMap[0]);
                CaseWordData nounWord = GameDataManager.Instance.GetWord(inCaseChoiceData.correctWordIdentifiersMap[1]);
                prefixText.text = SettingsManager.Instance.CreatePromptText(prefixWord.value, nounWord.value);
                totalReward += prefixWord.difficulty;
                totalReward += nounWord.difficulty;
            }
            else
            {
                Debug.LogError("Invalid number of correct words in the identifiers map.");
            }
            
            nounText.text = "";
            bgImage.color = caseChoiceData.colour;

            List<Transform> existingTaskTransforms = new List<Transform>();
            //delete existing task objects
            foreach (Transform child in taskHolder)
            {
                existingTaskTransforms.Add(child);
            }
            for(int i = existingTaskTransforms.Count - 1; i >= 0; i--) 
            {
                Destroy(existingTaskTransforms[i].gameObject);
            }

            foreach(CaseChoiceData.TaskSprite queuedTaskSprite in caseChoiceData.queuedTaskSprites)
            {
                GameObject spawnedTaskImageObject = Instantiate(taskPrefab, taskHolder);
                spawnedTaskImageObject.GetComponent<Image>().sprite = SettingsManager.Instance.GetTaskSprite(queuedTaskSprite);
            }
            totalReward += caseChoiceData.pointsPerCorrectWord * 2 + caseChoiceData.bonusPoints;

            rewardText.text = totalReward.ToString();
            modifierText.text = caseChoiceData.startingScoreModifier.ToString();
        }

        public void SetCaseTab(float caseTabModifierValue)
        {
            caseTabObject.SetActive(true);
            Bird playerBird = ColourManager.Instance.GetBird(SettingsManager.Instance.birdName);
            if(playerBird == null)
            {
                Debug.LogError("ERROR[SetCaseTab]: Could not get player bird["+SettingsManager.Instance.birdName.ToString()+"] because it doesn't exist in the ColourManager.");
            }
            caseTabFillImage.color = playerBird.colour;
            modifierText.text = (caseChoiceData.startingScoreModifier + caseTabModifierValue).ToString();
        }
    }
}

