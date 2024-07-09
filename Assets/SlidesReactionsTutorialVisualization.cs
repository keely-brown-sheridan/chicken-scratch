using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidesReactionsTutorialVisualization : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> reactionsObjects;

    [SerializeField]
    private float timeBetweenReveals;

    private float timeSinceLastReveal = 0f;
    private int reactionObjectIndex = 0;

    // Update is called once per frame
    void Update()
    {
        timeSinceLastReveal += Time.deltaTime;

        if(timeSinceLastReveal > timeBetweenReveals)
        {
            timeSinceLastReveal = 0f;
            reactionsObjects[reactionObjectIndex].SetActive(true);
            reactionObjectIndex++;
            if(reactionObjectIndex >= reactionsObjects.Count)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
