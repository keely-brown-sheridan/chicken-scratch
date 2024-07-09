using UnityEngine.UI;
using UnityEngine;

namespace ChickenScratch
{
    public class WaitingOnPlayerVisual : MonoBehaviour
    {
        [SerializeField]
        private BirdImage birdImage;

        public void Initialize(ColourManager.BirdName birdName)
        {
            BirdData bird = GameDataManager.Instance.GetBird(birdName);
            if(bird == null)
            {
                Debug.LogError("Could not initialize waiting on player visual because bird["+birdName.ToString()+"] is not mapped in the Colour Manager.");
                return;
            }

            BirdHatData.HatType birdHat = GameManager.Instance.playerFlowManager.GetBirdHatType(birdName);
            birdImage.Initialize(birdName, birdHat);
        }
    }

}
