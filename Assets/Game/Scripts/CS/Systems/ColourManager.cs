using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class ColourManager : Singleton<ColourManager>
    {
        [SerializeField]
        public enum BirdName
        {
            red, blue, green, grey, brown, purple, none, orange, black, pink, yellow, maroon, teal
        }


        private Dictionary<BirdName, Bird> birdMap = new Dictionary<BirdName, Bird>();
        public List<Bird> allBirds = new List<Bird>();

        public List<DrawingSizeData> allDrawingSizeData = new List<DrawingSizeData>();

        public List<DrawingColourData> allDrawingColourData = new List<DrawingColourData>();

        public List<DrawingShapeData> allDrawingShapeData = new List<DrawingShapeData>();

        public GameObject linePrefab;
        public Material eraseLineMaterial;
        public Material clearLineMaterial;
        public Material baseLineMaterial;
        public Material stampBorderGhostMaterial;
        public Material eraseGhostMaterial;

        

        void Awake()
        {
            foreach (Bird bird in allBirds)
            {
                birdMap.Add(bird.name, bird);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

    [System.Serializable]
    public class Bird
    {
        public Sprite faceSprite;
        public Sprite borderedFaceSprite;
        public Sprite cabinetFaceSprite;
        public Sprite scannerFaceSprite;
        public Sprite armSprite;
        public Color colour, bgColour, folderColour;
        public ColourManager.BirdName name;
        public Material material, bgLineMaterial, ghostMaterial, ghostBGMaterial;
        public Texture2D cursor, featherCursor, handCursor;
        public Sprite armOutlineSprite;
        public Sprite accusationBodySprite, accusationArmSprite, accusationHandSprite;
        public Sprite peanutGallerySprite;
        public Vector3 birdHandWidthScaling;
        public float birdHandRotationAdjustment;
        public GameObject slidesBirdPrefab;
        public GameObject storePlayerBirdArmPrefab;
        public GameObject storeBirdArmPrefab;
        public string birdSoundName;
    }

    [System.Serializable]
    public class DrawingSizeData
    {
        public string Identifier => _identifier;
        [SerializeField] private string _identifier;

        public float Size => _size;
        [SerializeField] private float _size;

        public float DotSize => _dotsize;
        [SerializeField] private float _dotsize;
    }

    [System.Serializable]
    public class DrawingColourData
    {
        public string Identifier => _identifier;
        [SerializeField] private string _identifier;

        public Color DrawColour => _drawColour;
        [SerializeField] private Color _drawColour;

        public Material DrawMaterial => _drawMaterial;
        [SerializeField] private Material _drawMaterial;

        public GameObject LinePrefab => _linePrefab;
        [SerializeField] private GameObject _linePrefab;
    }

    [System.Serializable]
    public class DrawingShapeData
    {
        public string Identifier => _identifier;
        [SerializeField] private string _identifier;

        public GameObject ShapePrefab => _shapePrefab;
        [SerializeField] private GameObject _shapePrefab;
    }
}