
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class PausePlayerListing : MonoBehaviour
    {
        [SerializeField]
        private BirdImage birdFaceImage;

        [SerializeField]
        private TMPro.TMP_Text playerText;

        [SerializeField]
        private GameObject kickHoverHighlightObject;

        [SerializeField]
        private GameObject coverHoverHighlightObject;

        [SerializeField]
        private TMPro.TMP_Text coverButtonText;

        [SerializeField]
        private GameObject connectedElementsObject;

        [SerializeField]
        private GameObject disconnectedTextObject;

        [SerializeField]
        private GameObject kickButtonObject;

        private enum CoverButtonState
        {
            cover, uncover
        }
        private CoverButtonState coverState = CoverButtonState.cover;

        private CoverConfirmation coverConfirmation;
        private KickConfirmation kickConfirmation;
        private ColourManager.BirdName birdName;

        public void Initialize(ColourManager.BirdName inBirdName, CoverConfirmation inCoverConfirmation, KickConfirmation inKickConfirmation)
        {
            birdName = inBirdName;
            BirdData listingBird = GameDataManager.Instance.GetBird(birdName);
            if(listingBird != null )
            {
                BirdHatData.HatType birdHat = GameManager.Instance.playerFlowManager.GetBirdHatType(birdName);
                birdFaceImage.Initialize(birdName, birdHat);
                playerText.color = listingBird.colour;
            }
            playerText.text = SettingsManager.Instance.GetPlayerName(birdName);

            coverConfirmation = inCoverConfirmation;
            kickConfirmation = inKickConfirmation;

            kickButtonObject.SetActive(SettingsManager.Instance.isHost);
        }

        public void ToggleCover()
        {
            switch(coverState)
            {
                case CoverButtonState.cover:
                    coverConfirmation.Show(birdName, this);
                    break;
                case CoverButtonState.uncover:
                    GameManager.Instance.playerFlowManager.ShowAuthorDrawingLines(birdName);
                    SettingsManager.Instance.UncoverPlayer(birdName);
                    coverButtonText.text = "COVER";
                    coverState = CoverButtonState.cover;
                    break;
            }
        }

        public void OnClickKick()
        {
            kickConfirmation.Show(birdName);
        }

        public void SwitchToUncover()
        {
            coverState = CoverButtonState.uncover;
            coverButtonText.text = "UNCOVER";
        }

        public void OnDisconnect()
        {
            connectedElementsObject.SetActive(false);
            disconnectedTextObject.SetActive(true);
        }
    }

}
