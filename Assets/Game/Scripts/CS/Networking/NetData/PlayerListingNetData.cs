using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChickenScratch
{
    [Serializable]
    public class PlayerListingNetData
    {
        public ColourManager.BirdName selectedBird = ColourManager.BirdName.none;
        public string playerID;
        public string playerName;
    }
}
