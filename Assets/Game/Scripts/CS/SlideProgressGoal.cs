using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class SlideProgressGoal : MonoBehaviour
    {
        [SerializeField]
        private Image bgImage;

        [SerializeField]
        private TMPro.TMP_Text nameText;

        private Color reachedColour;

        public void Initialize(string identifier, Color inNotReachedColour, Color inReachedColour)
        {
            nameText.text = identifier;
            bgImage.color = inNotReachedColour;
            reachedColour = inReachedColour;
        }

        public void SetAsReached()
        {
            bgImage.color = reachedColour;
            nameText.color = Color.black;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}