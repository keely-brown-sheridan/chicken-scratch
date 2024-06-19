using System.Collections;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static ChickenScratch.ColourManager;

namespace ChickenScratch
{
    public class GoldStarDetectionArea : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private Image starDetectionHoverImage;
        [SerializeField]
        private float hoverAlpha;

        public bool hasBeenStarred = false;

        public BirdName birdName => _birdName;
        public int round => _round;
        public int caseID => _caseID;

        private BirdName _birdName;
        private int _caseID;
        private int _round;

        public void Initialize(BirdName inBirdName, int inRound, int inCaseID)
        {
            hasBeenStarred = false;
            Unhover();
            _birdName = inBirdName;
            _round = inRound;
            _caseID = inCaseID;
        }


        public void Hover()
        {
            if (!hasBeenStarred)
            {
                starDetectionHoverImage.color = new Color(starDetectionHoverImage.color.r, starDetectionHoverImage.color.g, starDetectionHoverImage.color.b, hoverAlpha);
            }

        }

        public void Unhover()
        {
            starDetectionHoverImage.color = new Color(starDetectionHoverImage.color.r, starDetectionHoverImage.color.g, starDetectionHoverImage.color.b, 0.0f);
        }

        public bool GiveStar()
        {
            SlidesRound slidesRound = GameManager.Instance.playerFlowManager.slidesRound;
            if (birdName == BirdName.none || SettingsManager.Instance.birdName == birdName || slidesRound.hasAlreadyGivenLikeToRound(caseID, round))
            {
                Debug.LogError("Cannot give star. BirdName["+birdName.ToString()+"], ThisPlayer["+SettingsManager.Instance.birdName.ToString()+"]");
                return false;
            }

            if (SettingsManager.Instance.isHost)
            {
                EndgameCaseData selectedCase = GameManager.Instance.playerFlowManager.slidesRound.caseDataMap[caseID];

                if (round == -1)
                {
                    _round = GameManager.Instance.playerFlowManager.playerNameMap.Count;
                }

                PlayerRatingData ratingData = selectedCase.taskDataMap[round].ratingData;
                ratingData.likeCount++;
                ratingData.target = birdName;

                StatTracker.Instance.LikePlayer(birdName);
                GameManager.Instance.gameDataHandler.RpcShowSlideRatingVisual(SettingsManager.Instance.birdName, birdName);
            }
            else
            {
                StatTracker.Instance.LikePlayer(birdName);
                GameManager.Instance.gameDataHandler.CmdRateSlide(caseID, round, SettingsManager.Instance.birdName, birdName);
            }

            hasBeenStarred = true;
            starDetectionHoverImage.color = new Color(starDetectionHoverImage.color.r, starDetectionHoverImage.color.g, starDetectionHoverImage.color.b, 0.0f);


            return true;
        }

        public void OnEnable()
        {

        }

        public void OnDisable()
        {
            SlidesRound slidesRound = GameManager.Instance.playerFlowManager.slidesRound;
            if (slidesRound.currentHoveredGoldStarDetectionArea == this) slidesRound.currentHoveredGoldStarDetectionArea = null;
            Unhover();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            GameManager.Instance.playerFlowManager.slidesRound.currentHoveredGoldStarDetectionArea = this;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Unhover();
            GameManager.Instance.playerFlowManager.slidesRound.currentHoveredGoldStarDetectionArea = null;
        }
    }
}