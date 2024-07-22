using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static ChickenScratch.ColourManager;

namespace ChickenScratch
{
    public class SummaryEmailContents : MonoBehaviour
    {
        [SerializeField]
        private GameObject birdbucksRowPrefab, casesRowPrefab, starsRowPrefab;

        [SerializeField]
        private Transform birdbucksRowHolder, casesRowHolder, starsRowHolder;

        [SerializeField]
        private Image birdbucksButtonImage, casesButtonImage, starsButtonImage;

        [SerializeField]
        private GameObject birdbucksTabHolder, casesTabHolder, starsTabHolder;

        [SerializeField]
        private Color selectedTabColour, deselectedTabColour;




        public Text outcomeText;

        public void setSummaryContents()
        {
            Dictionary<BirdName, int> birdbucksMap = new Dictionary<BirdName, int>();
            Dictionary<BirdName, int> starsMap = new Dictionary<BirdName, int>();
            Dictionary<BirdName, List<Color>> folderColoursMap = new Dictionary<BirdName, List<Color>>();

            //Iterate over all cases to determine how many birdbucks each player got, what cases they were involved in and how many stars they received
            foreach (EndgameCaseData caseData in GameManager.Instance.playerFlowManager.slidesRound.caseDataMap.Values)
            {
                Color folderColour = GameDataManager.Instance.GetCaseChoice(caseData.caseTypeName).colour;
                int birdbucksEarned = caseData.taskDataMap.Count != 0 ? caseData.scoringData.GetTotalPoints() : 0;
                foreach (EndgameTaskData taskData in caseData.taskDataMap.Values)
                {
                    if (!birdbucksMap.ContainsKey(taskData.assignedPlayer))
                    {
                        birdbucksMap.Add(taskData.assignedPlayer, 0);
                        starsMap.Add(taskData.assignedPlayer, 0);
                        folderColoursMap.Add(taskData.assignedPlayer, new List<Color>());
                    }
                    birdbucksMap[taskData.assignedPlayer] += birdbucksEarned / caseData.taskDataMap.Count;
                    starsMap[taskData.assignedPlayer] += taskData.ratingData.likeCount;
                    folderColoursMap[taskData.assignedPlayer].Add(folderColour);
                }
            }

            //Determine what the highest amount of birdbucks was
            int highestBirdbucksTotal = 0;
            foreach (int value in birdbucksMap.Values)
            {
                if (value > highestBirdbucksTotal)
                {
                    highestBirdbucksTotal = value;
                }
            }
            //Order the birds by what's highest then create corresponding rows for each type
            List<BirdName> allBirds = birdbucksMap.OrderBy(bb => bb.Value).Select(bb => bb.Key).ToList();
            foreach(BirdName bird in allBirds)
            {
                GameObject instantiatedBirdbucksRow = Instantiate(birdbucksRowPrefab, birdbucksRowHolder);
                ResultsBirdbuckRow resultsBirdbuckRow = instantiatedBirdbucksRow.GetComponent<ResultsBirdbuckRow>();
                resultsBirdbuckRow.Initialize(bird, ((float)(birdbucksMap[bird])) / ((float)highestBirdbucksTotal));
            }

            allBirds = folderColoursMap.OrderBy(fcs => fcs.Value.Count).Select(fcs => fcs.Key).ToList();
            foreach(BirdName bird in allBirds)
            {
                GameObject instantiatedFolderColoursRow = Instantiate(casesRowPrefab, casesRowHolder);
                ResultsCasesRow resultsCasesRow = instantiatedFolderColoursRow.GetComponent<ResultsCasesRow>();
                resultsCasesRow.Initialize(bird, folderColoursMap[bird]);
            }

            allBirds = starsMap.OrderBy(s => s.Value).Select(s => s.Key).ToList();
            foreach(BirdName bird in allBirds)
            {
                GameObject instantiatedStarsRow = Instantiate(starsRowPrefab, starsRowHolder);
                ResultsStarRow starsRow = instantiatedStarsRow.GetComponent<ResultsStarRow>();
                starsRow.Initialize(bird, starsMap[bird]);
            }

            OnBirdbucksTabSelected();
        }

        public void OnBirdbucksTabSelected()
        {
            AudioManager.Instance.PlaySoundVariant("sfx_ui_int_gen_sel");
            birdbucksButtonImage.color = selectedTabColour;
            casesButtonImage.color = deselectedTabColour;
            starsButtonImage.color = deselectedTabColour;
            birdbucksTabHolder.SetActive(true);
            casesTabHolder.SetActive(false);
            starsTabHolder.SetActive(false);
        }

        public void OnCasesTabSelected()
        {
            AudioManager.Instance.PlaySoundVariant("sfx_ui_int_gen_sel");
            birdbucksButtonImage.color = deselectedTabColour;
            casesButtonImage.color = selectedTabColour;
            starsButtonImage.color = deselectedTabColour;
            birdbucksTabHolder.SetActive(false);
            casesTabHolder.SetActive(true);
            starsTabHolder.SetActive(false);
        }

        public void OnStarsTabSelected()
        {
            AudioManager.Instance.PlaySoundVariant("sfx_ui_int_gen_sel");
            birdbucksButtonImage.color = deselectedTabColour;
            casesButtonImage.color = deselectedTabColour;
            starsButtonImage.color = selectedTabColour;
            birdbucksTabHolder.SetActive(false);
            casesTabHolder.SetActive(false);
            starsTabHolder.SetActive(true);
        }
    }
}