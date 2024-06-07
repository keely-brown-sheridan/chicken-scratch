using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static ChickenScratch.ColourManager;

namespace ChickenScratch
{
    public class PointsPieChart : MonoBehaviour
    {
        [SerializeField]
        private GameObject pieChartPrefab;
        [SerializeField]
        private Transform pieChartParent;

        public void SetPointValues(Dictionary<BirdName, float> birdPointsMap)
        {
            float percentageProgress = 0.0f;
            float total = birdPointsMap.Sum(bp => bp.Value);

            foreach (KeyValuePair<BirdName, float> birdPoint in birdPointsMap)
            {
                GameObject sliceObject = Instantiate(pieChartPrefab, pieChartParent);
                Image sliceImage = sliceObject.GetComponent<Image>();
                sliceImage.color = ColourManager.Instance.birdMap[birdPoint.Key].colour;
                percentageProgress += birdPoint.Value;
                sliceImage.fillAmount = percentageProgress / total;
                sliceObject.transform.SetAsFirstSibling();
            }
        }
    }
}