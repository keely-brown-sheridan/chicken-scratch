
using UnityEngine;

namespace ChickenScratch
{
    public class CaseEmailSection : MonoBehaviour
    {
        public CaseEmail.CaseEmailTaskType taskType;

        [SerializeField]
        private TMPro.TMP_Text likeCountText;
        [SerializeField]
        private GameObject likesHolderObject;

        public void SetRating(int likes)
        {
            if (likes > 0)
            {
                likesHolderObject.gameObject.SetActive(true);
                likeCountText.text = "x" + likes.ToString();
            }
        }

    }
}
