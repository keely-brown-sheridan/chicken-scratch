using ChickenScratch;
using UnityEngine;
using UnityEngine.UI;

public class BirdVoteVisualization : MonoBehaviour
{
    [SerializeField]
    private Image birdImage;

    public void Initialize(BirdData birdData)
    {
        birdImage.sprite = birdData.faceSprite;
    }
}
