
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class RoomListing : MonoBehaviour
    {
        public Text RoomNameText;
        public Text AttendeeCountText;
        public string roomName;
        public CSteamID roomID;

        public void Select()
        {
            MenuLobbyButtons.Instance.SelectRoomListing(this);
        }
    }
}