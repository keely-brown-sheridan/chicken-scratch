using UnityEngine.UI;
using UnityEngine;

namespace ChickenScratch
{
    public class CoverConfirmation : MonoBehaviour
    {

        [SerializeField]
        private BirdImage birdImage;

        [SerializeField]
        private TMPro.TMP_Text promptText;

        private ColourManager.BirdName birdName;
        private PausePlayerListing pauseListing;

        public void Show(ColourManager.BirdName inBirdName, PausePlayerListing inPauseListing)
        {
            pauseListing = inPauseListing;
            birdName = inBirdName;
            BirdData coveringBird = GameDataManager.Instance.GetBird(birdName);
            if (coveringBird != null)
            {
                BirdHatData.HatType birdHat = GameManager.Instance.playerFlowManager.GetBirdHatType(birdName);
                birdImage.Initialize(birdName, birdHat);

            }
            string playerName = SettingsManager.Instance.GetPlayerName(birdName);
            promptText.text = "Are you sure you want to cover " + playerName + "'s tasks?";
            gameObject.SetActive(true);
        }

        public void Confirm()
        {
            pauseListing.SwitchToUncover();
            SettingsManager.Instance.CoverPlayer(birdName);
            gameObject.SetActive(false);
            GameManager.Instance.playerFlowManager.HideAuthorDrawingLines(birdName);
        }

        public void Cancel()
        {
            pauseListing = null;
            gameObject.SetActive(false);
        }
    }

}
