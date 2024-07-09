using UnityEngine;

namespace ChickenScratch
{
    [CreateAssetMenu(fileName = "Hat", menuName = "GameData/Create Hat")]
    public class HatData : ScriptableObject
    {
        public Sprite sprite;
        public float width;
        public float height;
        public BirdHatData.HatType hatType;
    }
}
