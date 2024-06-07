using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChickenScratch
{
    public class FollowTransform : MonoBehaviour
    {
        public Transform transformToFollow;

        // Update is called once per frame
        void Update()
        {
            transform.position = transformToFollow.position;
        }
    }
}