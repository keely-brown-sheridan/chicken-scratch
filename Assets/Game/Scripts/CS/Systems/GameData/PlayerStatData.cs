using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChickenScratch
{
    [System.Serializable]
    public class PlayerStatData
    {
        public bool pencilUsed, colourMarkerUsed, lightMarkerUsed, eraserUsed;
        public ColourManager.BirdName birdName;
        public float timeInCabinetArea;
        public float guessingTime;
        public int stickyCount;
        public int drawingsTrashed;
        public int numberOfPlayersLiked, numberOfLikesGiven;
        public bool usedStickies;
        public float totalDistanceMoved;

        //Not initialized in the stats roles stuff yet
        public int totalSpent = 0;
        public int totalItemsPurchased = 0;
        public int totalCoffeeItemsPurchased = 0;
        public float timeChoosing = 0f;
        public int casesStarted = 0;
        public bool alwaysChoseHighestDifficulty = true;
        public bool alwaysChoseLowestDifficulty = true;
        public int storeRestocks = 0;
        public bool restockedEmptyShop = false;
        public bool hasLostModifier = false;
        public int numberOfUniqueCases;
        public int totalUnspent;

    }
}
