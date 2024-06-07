using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class GoldStarManager : MonoBehaviour
    {
        [SerializeField]
        private List<DragDropStar> goldStars = new List<DragDropStar>();

        public void restock()
        {
            foreach (DragDropStar goldStar in goldStars)
            {
                if (goldStar.restock())
                {
                    break;
                }
            }
        }
    }
}