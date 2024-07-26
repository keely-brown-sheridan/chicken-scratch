
using UnityEngine;
using UnityEngine.UI;


namespace ChickenScratch
{
    public class SummarySlideSection : MonoBehaviour
    {
        public SlideTypeData.SlideType slideType => _slideType;

        public ColourManager.BirdName author => _author;

        protected ColourManager.BirdName _author = ColourManager.BirdName.none;
        [SerializeField]
        protected SlideTypeData.SlideType _slideType;


        public Transform birdBuckArrivalTransform => _birdBuckArrivalTransform;
        [SerializeField]
        private Transform _birdBuckArrivalTransform;


        [SerializeField]
        protected BirdImage authorImage;

        [SerializeField]
        protected TMPro.TMP_Text authorNameText;

        [SerializeField]
        private TMPro.TMP_Text birdBucksEarnedText;

        [SerializeField]
        private GameObject competitionSelectionVisual;

        private int birdBucksEarned = 0;
        public Vector3 positionWhereItShouldBeIfUnityWasntShit;

        
        public void IncreaseBirdbucks()
        {
            if(birdBucksEarned == 0)
            {
                birdBuckArrivalTransform.gameObject.SetActive(true);
            }
            
            birdBucksEarned++;
            birdBucksEarnedText.text = birdBucksEarned.ToString();
        }

        public virtual void Show()
        {
            //transform.position = positionWhereItShouldBeIfUnityWasntShit;
        }

        public void ShowCompetitionSelectionVisual()
        {
            competitionSelectionVisual.SetActive(true);
        }
    }
}
