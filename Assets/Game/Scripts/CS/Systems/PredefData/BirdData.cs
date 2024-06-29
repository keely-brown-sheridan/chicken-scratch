using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ChickenScratch
{
    [CreateAssetMenu(fileName = "Bird", menuName = "GameData/Create Bird")]
    public class BirdData : ScriptableObject
    {
        public Sprite faceSprite;
        public Sprite borderedFaceSprite;
        public Sprite cabinetFaceSprite;
        public Sprite scannerFaceSprite;
        public Sprite armSprite;
        public Color colour, bgColour, folderColour;
        public ColourManager.BirdName birdName;
        public Material material, bgLineMaterial, ghostMaterial, ghostBGMaterial;
        public Texture2D cursor, featherCursor, handCursor;
        public Sprite armOutlineSprite;
        public Sprite accusationBodySprite, accusationArmSprite, accusationHandSprite;
        public Sprite peanutGallerySprite;
        public GameObject accuseBirdPrefab;
        public GameObject accuseArmPrefab;
        public Vector3 birdHandWidthScaling;
        public float birdHandRotationAdjustment;
        public GameObject slidesBirdPrefab;
        public GameObject storePlayerBirdArmPrefab;
        public GameObject storeBirdArmPrefab;
        public string birdSoundName;
    }
}
