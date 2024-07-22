using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ChickenScratch
{
    public class CollapsingBoxType : DrawingBoxType
    {
        [SerializeField]
        private List<DrawingBlockerVariantData> variants;

        [SerializeField]
        private GameObject blockerPrefab;

        private void Start()
        {
            //Test();
        }

        private void Test()
        {
            Initialize(10f);
        }


        public void Initialize(float caseTime)
        {
            //Clear previous blockers if there are any
            List<Transform> transforms = new List<Transform>();
            foreach(Transform child in transform)
            {
                transforms.Add(child);
            }
            for(int i = transforms.Count - 1; i >= 0; i--)
            {
                Destroy(transforms[i].gameObject);
            }

            //Choose a random collapse type
            variants = variants.OrderBy(x => System.Guid.NewGuid()).ToList();

            foreach(DrawingBlockerBoxData boxData in variants[0].drawingBoxes)
            {
                GameObject blockerObject = GameObject.Instantiate(blockerPrefab, transform);
                DrawingBlocker blocker = blockerObject.GetComponent<DrawingBlocker>();
                if(blocker != null)
                {
                    float startTime = caseTime * boxData.startingTimeRatio;
                    float endTime = caseTime * boxData.endingTimeRatio;

                    blocker.Initialize(startTime, endTime, boxData.startingDimensions, boxData.targetDimensions, boxData.startingPosition, boxData.targetPosition);
                }
            }

        }
    }

}
