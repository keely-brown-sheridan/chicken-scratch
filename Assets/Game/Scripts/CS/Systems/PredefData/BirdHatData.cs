using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace ChickenScratch
{
    [System.Serializable]
    public class BirdHatData
    {
        public enum HatType
        {
            crown, baseball_cap, jester_hat, top_hat, fez, chefs_hat, wizards, beret, straw_hat, santa, sherlock, flower, propeller, none
        }
        public HatType hatType;
        public string hatName;

        public Sprite hatSprite;
        public Vector3 position;
        public Vector3 rotation;
        public float width;
        public float height;
        public Vector3 scale = Vector3.one;
    }
}
