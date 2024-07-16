using ChickenScratch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BirdImage : MonoBehaviour
{
    [SerializeField]
    private Image faceSpriteImage;

    [SerializeField]
    private Image hatImage;

    public ColourManager.BirdName currentBird => _currentBird;
    private ColourManager.BirdName _currentBird = ColourManager.BirdName.none;

   // private List<ColourManager.BirdName> testBirds = new List<ColourManager.BirdName>() { ColourManager.BirdName.red, ColourManager.BirdName.blue, ColourManager.BirdName.green,
    //                                                                                        ColourManager.BirdName.orange, ColourManager.BirdName.purple, ColourManager.BirdName.grey,
     //                                                                                        ColourManager.BirdName.maroon, ColourManager.BirdName.black, ColourManager.BirdName.brown,
    //                                                                                             ColourManager.BirdName.pink, ColourManager.BirdName.yellow, ColourManager.BirdName.teal };
    //private List<BirdHatData.HatType> testHats = new List<BirdHatData.HatType>() { BirdHatData.HatType.flower, BirdHatData.HatType.sherlock, BirdHatData.HatType.santa, BirdHatData.HatType.propeller };
    //private int currentTestBirdIndex = 0;
    //private int currentTestHatIndex = 0;

    private void Awake()
    {
        //_currentBird = testBirds[currentTestBirdIndex];
        //GameManager.Instance.playerFlowManager.SetBirdHatType(_currentBird, testHats[currentTestHatIndex]);
        if(!GameManager.Instance.playerFlowManager.activeBirdImages.Contains(this))
        {
            GameManager.Instance.playerFlowManager.activeBirdImages.Add(this);
            if(_currentBird != ColourManager.BirdName.none)
            {
                Initialize(_currentBird, GameManager.Instance.playerFlowManager.GetBirdHatType(_currentBird));
            }
            
        }

    }
    private void OnDestroy()
    {
        if(GameManager.Instance.playerFlowManager.activeBirdImages.Contains(this))
        {
            GameManager.Instance.playerFlowManager.activeBirdImages.Remove(this);
        }
    }

    private void Update()
    {
        //if(Input.GetKeyDown(KeyCode.E))
        //{
        //    //Increase the hat index
        //    currentTestBirdIndex++;
        //    if(currentTestBirdIndex >= testBirds.Count)
        //    {
        //        currentTestBirdIndex = 0;
        //    }

        //    _currentBird = testBirds[currentTestBirdIndex];
        //    GameManager.Instance.playerFlowManager.SetBirdHatType(_currentBird, testHats[currentTestHatIndex]);
        //}
        //if(Input.GetKeyDown(KeyCode.Q))
        //{
        //    //Increase the bird index
        //    currentTestHatIndex++;
        //    if(currentTestHatIndex >= testHats.Count)
        //    {
        //        currentTestHatIndex = 0;
        //    }

        //    _currentBird = testBirds[currentTestBirdIndex];
        //    GameManager.Instance.playerFlowManager.SetBirdHatType(_currentBird, testHats[currentTestHatIndex]);
        //}
    }

    public void Initialize(ColourManager.BirdName birdName, BirdHatData.HatType hatType = BirdHatData.HatType.none)
    {
        _currentBird = birdName;
        BirdData birdData = GameDataManager.Instance.GetBird(birdName);
        if (birdData != null)
        {
            faceSpriteImage.sprite = birdData.borderedFaceSprite;
            BirdHatData hatData = birdData.GetHat(hatType);
            if(hatData == null)
            {
                hatImage.gameObject.SetActive(false);
            }
            else
            {
                hatImage.gameObject.SetActive(true);
                hatImage.sprite = hatData.hatSprite;

                float widthRatio = faceSpriteImage.rectTransform.rect.width / 100f;
                float heightRatio = faceSpriteImage.rectTransform.rect.height / 100f;

                hatImage.rectTransform.sizeDelta = new Vector2(hatData.width * widthRatio, hatData.height * heightRatio);
                hatImage.transform.eulerAngles = hatData.rotation;
                hatImage.transform.localPosition = new Vector3(hatData.position.x * widthRatio, hatData.position.y * heightRatio, hatData.position.z);
                hatImage.transform.localScale = hatData.scale;
            }
        }
    }

    public void UpdateImage(ColourManager.BirdName birdName, BirdHatData.HatType hatType = BirdHatData.HatType.none)
    {
        if(_currentBird == birdName)
        {
            Initialize(birdName, hatType);
        }
    }
}
