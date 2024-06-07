using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ChickenScratch
{
    public class FileSummaryEmailContents : MonoBehaviour
    {
        public List<BirdTag> allFileSummaryEmails;

        public Dictionary<ColourManager.BirdName, CaseEmail> fileSummaryEmailMap;

        public bool isInitialized = false;

        public GameObject fileSavedTextObject;
        public TMPro.TMP_Text fileSavedPathText;

        public float totalTimeToShowFileSavedText = 5f;
        private float totalTimeShowingFileSavedText = 0.0f;

        public Dictionary<int, GameObject> fileSummaryIndexMap = new Dictionary<int, GameObject>();
        public List<int> orderedIndexMap = new List<int>();
        private int currentPageIndex = 1;

        [SerializeField]
        private GameObject saveGifButtonObject;
        [SerializeField]
        private GameObject openGifFolderButtonObject;

        // Start is called before the first frame update
        void Start()
        {
            if (!isInitialized)
            {
                initialize();
            }
        }

        public void initialize()
        {
            fileSummaryEmailMap = new Dictionary<ColourManager.BirdName, CaseEmail>();
            fileSummaryIndexMap = new Dictionary<int, GameObject>();
            isInitialized = true;
        }

        void Update()
        {
            if (totalTimeShowingFileSavedText > 0.0f)
            {
                totalTimeShowingFileSavedText += Time.deltaTime;
                if (totalTimeShowingFileSavedText > totalTimeToShowFileSavedText)
                {
                    totalTimeShowingFileSavedText = 0.0f;
                    fileSavedTextObject.SetActive(false);
                    fileSavedPathText.gameObject.SetActive(false);
                }
            }
        }

        public void shiftPageLeft()
        {
            AudioManager.Instance.PlaySoundVariant("sfx_vote_int_gen_click_owl");
            int caseID = orderedIndexMap[currentPageIndex];
            fileSummaryIndexMap[caseID].SetActive(false);
            currentPageIndex--;
            if (currentPageIndex < 0)
            {
                currentPageIndex = fileSummaryIndexMap.Count - 1;
            }
            caseID = orderedIndexMap[currentPageIndex];
            fileSummaryIndexMap[caseID].SetActive(true);

        }

        public void shiftPageRight()
        {
            AudioManager.Instance.PlaySoundVariant("sfx_vote_int_gen_click_owl");
            int caseID = orderedIndexMap[currentPageIndex];
            fileSummaryIndexMap[caseID].SetActive(false);
            currentPageIndex++;
            if (currentPageIndex >= fileSummaryIndexMap.Count)
            {
                currentPageIndex = 0;
            }
            caseID = orderedIndexMap[currentPageIndex];
            fileSummaryIndexMap[caseID].SetActive(true);
        }

        public void addCase(GameObject contentsObject, int index)
        {
            fileSummaryIndexMap.Add(index, contentsObject);
            orderedIndexMap = fileSummaryIndexMap.Keys.ToList();
            orderedIndexMap.Sort();
        }

        public void enableFirstCase()
        {
            if (orderedIndexMap.Count == 0) return;
            int caseID = orderedIndexMap[currentPageIndex];
            fileSummaryIndexMap[caseID].SetActive(true);
        }

        public void saveGifOfChain()
        {
            AudioManager.Instance.PlaySoundVariant("sfx_vote_int_gen_click_owl");
            int caseID = orderedIndexMap[currentPageIndex];

            string fileSavePath = GameManager.Instance.playerFlowManager.MergeSlideImages(caseID);
            totalTimeShowingFileSavedText = Time.deltaTime;
            fileSavedTextObject.SetActive(true);
            fileSavedPathText.gameObject.SetActive(true);
            GUIUtility.systemCopyBuffer = fileSavePath;

            //


        }

        public void openGifFolder()
        {
            AudioManager.Instance.PlaySoundVariant("sfx_vote_int_gen_click_owl");
            System.Diagnostics.Process.Start(Application.persistentDataPath + "\\Screenshots\\");
        }
    }
}