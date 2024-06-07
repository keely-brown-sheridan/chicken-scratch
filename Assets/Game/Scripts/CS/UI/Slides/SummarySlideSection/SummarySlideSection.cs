
using UnityEngine;
using UnityEngine.UI;


namespace ChickenScratch
{
    public class SummarySlideSection : MonoBehaviour
    {
        public SlideTypeData.SlideType slideType => _slideType;

        [SerializeField]
        private SlideTypeData.SlideType _slideType;

        [SerializeField]
        protected GoldStarDetectionArea goldStarDetectionArea;

        [SerializeField]
        protected Image authorImage;

        [SerializeField]
        protected TMPro.TMP_Text authorNameText;

        public Vector3 positionWhereItShouldBeIfUnityWasntShit;

        public virtual void Show()
        {
            transform.position = positionWhereItShouldBeIfUnityWasntShit;
        }
    }
}
