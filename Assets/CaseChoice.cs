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

        public void Initialize(CaseChoiceNetData inCaseChoiceData)
        {
            CaseChoiceData caseChoiceData = GameDataManager.Instance.GetCaseChoice(inCaseChoiceData.caseChoiceIdentifier);
            titleText.text = caseChoiceData.identifier;
            descriptionText.text = caseChoiceData.description;
            if(inCaseChoiceData.correctWordIdentifiersMap.Count == 2)
            {
                CaseWordData prefixWord = GameDataManager.Instance.GetWord(inCaseChoiceData.correctWordIdentifiersMap[0]);
                CaseWordData nounWord = GameDataManager.Instance.GetWord(inCaseChoiceData.correctWordIdentifiersMap[1]);
                prefixText.text = SettingsManager.Instance.CreatePromptText(prefixWord.value, nounWord.value);
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
            int totalReward = caseChoiceData.pointsPerCorrectWord * 2 + caseChoiceData.bonusPoints;
            rewardText.text = totalReward.ToString();
        }
    }
}

