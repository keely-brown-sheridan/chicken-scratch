
using UnityEngine;

namespace ChickenScratch
{
    public class CaseEmailSection : MonoBehaviour
    {
        public CaseEmail.CaseEmailTaskType taskType;

        [SerializeField]
        private TMPro.TMP_Text likeCountText;

        public void SetRating(int likes)
        {
            if (likes > 0)
            {
                likeCountText.gameObject.SetActive(true);
                likeCountText.text = "x" + likes.ToString();
            }
        }

    }
}
