using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static ChickenScratch.ColourManager;

namespace ChickenScratch
{
    public class TimeBarGraph : MonoBehaviour
    {
        [SerializeField]
        private GameObject barPrefab;

        [SerializeField]
        private Transform barParent;

        public void SetTimeValues(Dictionary<BirdName, float> birdTimeMap)
        {
            float total = birdTimeMap.OrderBy(bp => bp.Value).Last().Value;

            foreach (KeyValuePair<BirdName, float> birdTime in birdTimeMap)
            {
                GameObject sliceObject = Instantiate(barPrefab, barParent);
                Image sliceImage = sliceObject.GetComponent<Image>();
                sliceImage.color = ColourManager.Instance.birdMap[birdTime.Key].colour;
                sliceImage.fillAmount = birdTime.Value / total;
                sliceObject.transform.SetAsFirstSibling();
            }
        }
    }
}