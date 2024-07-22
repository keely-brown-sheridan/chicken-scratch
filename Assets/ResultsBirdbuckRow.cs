using ChickenScratch;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class ResultsBirdbuckRow : MonoBehaviour
    {
        [SerializeField]
        private BirdImage birdImage;

        [SerializeField]
        private Image birdbucksVisualization;

        public void Initialize(ColourManager.BirdName birdName, float widthRatio)
        {
            birdImage.Initialize(birdName, GameManager.Instance.playerFlowManager.GetBirdHatType(birdName));
            birdbucksVisualization.color = GameDataManager.Instance.GetBird(birdName).colour;
            birdbucksVisualization.rectTransform.sizeDelta = new Vector2(birdbucksVisualization.rectTransform.sizeDelta.x * widthRatio, birdbucksVisualization.rectTransform.sizeDelta.y);
        }
}
}

