using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class CursorFollow : MonoBehaviour
    {
        [SerializeField]
        private PauseMenu pauseMenu;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (!pauseMenu || !pauseMenu.isOpen)
            {
                Vector3 temp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                transform.position = new Vector3(temp.x, temp.y, transform.position.z);
            }

        }
    }
}