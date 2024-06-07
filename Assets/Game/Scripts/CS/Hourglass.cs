using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenScratch
{
    public class Hourglass : MonoBehaviour
    {
        [SerializeField]
        private Image topSandImage;

        [SerializeField]
        private Image bottomSandImage;

        [SerializeField]
        private float fallDuration;

        [SerializeField]
        private float rotationDuration;

        private enum State
        {
            top_falling, top_rotating, bottom_falling, bottom_rotating, idle, invalid
        }

        private float timeFalling = 0.0f;
        private float timeRotating = 0.0f;

        private State state = State.idle;

        // Start is called before the first frame update
        void Start()
        {
            state = State.top_falling;
        }

        // Update is called once per frame
        void Update()
        {
            switch (state)
            {
                case State.top_falling:
                    timeFalling += Time.deltaTime;
                    topSandImage.fillAmount = 1 - timeFalling / fallDuration;
                    bottomSandImage.fillAmount = timeFalling / fallDuration;
                    if (timeFalling > fallDuration)
                    {
                        AudioManager.Instance.PlaySoundVariant("sfx_game_env_loading");
                        timeFalling = 0.0f;
                        bottomSandImage.fillOrigin = (int)Image.OriginVertical.Bottom;
                        topSandImage.fillOrigin = (int)Image.OriginVertical.Top;
                        state = State.top_rotating;
                    }
                    break;
                case State.top_rotating:
                    timeRotating += Time.deltaTime;
                    transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, -180 * timeRotating / rotationDuration);
                    if (timeRotating > rotationDuration)
                    {
                        timeRotating = 0.0f;
                        state = State.bottom_falling;
                    }
                    break;

                case State.bottom_falling:
                    timeFalling += Time.deltaTime;
                    bottomSandImage.fillAmount = 1 - timeFalling / fallDuration;
                    topSandImage.fillAmount = timeFalling / fallDuration;
                    if (timeFalling > fallDuration)
                    {
                        AudioManager.Instance.PlaySoundVariant("sfx_game_env_loading");
                        timeFalling = 0.0f;
                        bottomSandImage.fillOrigin = (int)Image.OriginVertical.Top;
                        topSandImage.fillOrigin = (int)Image.OriginVertical.Bottom;
                        state = State.bottom_rotating;
                    }
                    break;
                case State.bottom_rotating:
                    timeRotating += Time.deltaTime;
                    transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, -180 - 180 * (timeRotating / rotationDuration));
                    if (timeRotating > rotationDuration)
                    {
                        timeRotating = 0.0f;
                        state = State.top_falling;
                    }
                    break;
            }
        }
    }
}