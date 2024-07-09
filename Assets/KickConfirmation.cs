
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class KickConfirmation : MonoBehaviour
    {
        [SerializeField]
        private BirdImage birdImage;

        [SerializeField]
        private TMPro.TMP_Text promptText;

        private ColourManager.BirdName birdName;

        public void Show(ColourManager.BirdName inBirdName)
        {
            birdName = inBirdName;
            BirdData kickingBird = GameDataManager.Instance.GetBird(birdName);
            if(kickingBird != null)
            {
                BirdHatData.HatType birdHat = GameManager.Instance.playerFlowManager.GetBirdHatType(birdName);
                birdImage.Initialize(birdName, birdHat);

            }
            string playerName = SettingsManager.Instance.GetPlayerName(birdName);
            promptText.text = "Are you sure you want to kick "+ playerName + "?";
            gameObject.SetActive(true);
        }

        public void Confirm()
        {
            SettingsManager.Instance.KickConnection(birdName);
            gameObject.SetActive(false);
        }

        public void Cancel()
        {
            gameObject.SetActive(false);
        }
    }
}

