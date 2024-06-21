using ChickenScratch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlideBirdBuck : MonoBehaviour
{
    [SerializeField]
    private float risingHeight;

    [SerializeField]
    private Image buckImage;

    [SerializeField]
    private string spawnSFX;

    [SerializeField]
    private float lifetime;

    [SerializeField]
    private float startingSize;

    private float timeAlive = 0f;
    private Vector3 startingPosition;
    private Vector3 targetPosition;

    // Start is called before the first frame update
    void Start()
    {
        AudioManager.Instance.PlaySound(spawnSFX, true);
        startingPosition = transform.position;
        targetPosition = transform.position + Vector3.up * risingHeight;
    }

    // Update is called once per frame
    void Update()
    {
        timeAlive += Time.deltaTime;
        if(timeAlive >= lifetime)
        {
            Destroy(gameObject);
            return;
        }
        float timeRatio = timeAlive / lifetime;
        transform.position = Vector3.Lerp(startingPosition, targetPosition, timeRatio);
        Vector3 startingScale = Vector3.one * startingSize;

        transform.localScale = Vector3.Lerp(startingScale, Vector3.one, timeRatio);
        buckImage.color = new Color(1f, 1f, 1f, 1 - timeRatio);
    }
}
