using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class AccoladesTest : MonoBehaviour
    {
        public List<AccoladesBirdRow> birdRows = new List<AccoladesBirdRow>();

        // Start is called before the first frame update
        void Start()
        {
            AccoladesBirdRow redRow = birdRows[0];
            redRow.birdHeadImage.sprite = GameDataManager.Instance.GetBird(ColourManager.BirdName.red).faceSprite;
            redRow.playerNameText.text = "Chicken";
            redRow.playerNameText.color = GameDataManager.Instance.GetBird(ColourManager.BirdName.red).colour;
            redRow.statRoleText.text = "Wordsmith";
            redRow.statDescriptionText.text = "Created the longest prompts.";

            AccoladesBirdRow blueRow = birdRows[1];
            blueRow.birdHeadImage.sprite = GameDataManager.Instance.GetBird(ColourManager.BirdName.blue).faceSprite;
            blueRow.playerNameText.text = "Parrot";
            blueRow.playerNameText.color = GameDataManager.Instance.GetBird(ColourManager.BirdName.blue).colour;
            blueRow.statRoleText.text = "Coaster";
            blueRow.statDescriptionText.text = "Guessed on the easiest case.";

            AccoladesBirdRow greenRow = birdRows[2];
            greenRow.birdHeadImage.sprite = GameDataManager.Instance.GetBird(ColourManager.BirdName.green).faceSprite;
            greenRow.playerNameText.text = "Duck";
            greenRow.playerNameText.color = GameDataManager.Instance.GetBird(ColourManager.BirdName.green).colour;
            greenRow.statRoleText.text = "Deliberator";
            greenRow.statDescriptionText.text = "Took longest on their guesses.";

            AccoladesBirdRow purpleRow = birdRows[3];
            purpleRow.birdHeadImage.sprite = GameDataManager.Instance.GetBird(ColourManager.BirdName.purple).faceSprite;
            purpleRow.playerNameText.text = "Penguin";
            purpleRow.playerNameText.color = GameDataManager.Instance.GetBird(ColourManager.BirdName.purple).colour;
            purpleRow.statRoleText.text = "Perfectionist";
            purpleRow.statDescriptionText.text = "Spent the longest on their cases.";

            AccoladesBirdRow grayRow = birdRows[4];
            grayRow.birdHeadImage.sprite = GameDataManager.Instance.GetBird(ColourManager.BirdName.grey).faceSprite;
            grayRow.playerNameText.text = "Dove";
            grayRow.playerNameText.color = GameDataManager.Instance.GetBird(ColourManager.BirdName.grey).colour;
            grayRow.statRoleText.text = "Procrastinator";
            grayRow.statDescriptionText.text = "Cut it closest to the deadlines.";

            AccoladesBirdRow orangeRow = birdRows[5];
            orangeRow.birdHeadImage.sprite = GameDataManager.Instance.GetBird(ColourManager.BirdName.orange).faceSprite;
            orangeRow.playerNameText.text = "Toucan";
            orangeRow.playerNameText.color = GameDataManager.Instance.GetBird(ColourManager.BirdName.orange).colour;
            orangeRow.statRoleText.text = "Decoder";
            orangeRow.statDescriptionText.text = "Correctly guessed the hardest case.";

            AccoladesBirdRow blackRow = birdRows[6];
            blackRow.birdHeadImage.sprite = GameDataManager.Instance.GetBird(ColourManager.BirdName.black).faceSprite;
            blackRow.playerNameText.text = "Crow";
            blackRow.playerNameText.color = GameDataManager.Instance.GetBird(ColourManager.BirdName.black).colour;
            blackRow.statRoleText.text = "Early Bird";
            blackRow.statDescriptionText.text = "Used least amount of time on their cases.";

            AccoladesBirdRow maroonRow = birdRows[7];
            maroonRow.birdHeadImage.sprite = GameDataManager.Instance.GetBird(ColourManager.BirdName.brown).faceSprite;
            maroonRow.playerNameText.text = "Ostrich";
            maroonRow.playerNameText.color = GameDataManager.Instance.GetBird(ColourManager.BirdName.brown).colour;
            maroonRow.statRoleText.text = "Team Player";
            maroonRow.statDescriptionText.text = "Gave every player a gold star.";

            redRow.StartPlacing();
            blueRow.StartPlacing();
            greenRow.StartPlacing();
            orangeRow.StartPlacing();
            blackRow.StartPlacing();
            grayRow.StartPlacing();
            maroonRow.StartPlacing();
            purpleRow.StartPlacing();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}