using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ChickenScratch
{
    [CreateAssetMenu(fileName = "ChargedStoreItem", menuName = "GameData/Create Charged Store Item")]
    public class ChargedStoreItemData : StoreItemData
    {
        public int numberOfUses;
    }
}
