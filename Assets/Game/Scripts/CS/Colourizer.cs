using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class Colourizer : MonoBehaviour
    {
        public List<Image> imagesToColourize = new List<Image>();

        public void Colourize(Color inColour)
        {
            foreach (Image imageToColourize in imagesToColourize)
            {
                imageToColourize.color = inColour;
            }
        }
    }
}