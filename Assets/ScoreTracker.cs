using ChickenScratch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreTracker : MonoBehaviour
{
    [SerializeField]
    private TMPro.TMP_Text birdBucksText;

    [SerializeField]
    private string increaseSFX;

    [SerializeField]
    private Animator animator;

    private int totalEarned = 0;

    public void Reset()
    {
        totalEarned = 0;
        birdBucksText.text = totalEarned.ToString();
    }

    public void IncreaseEarnedBonusBucks(int amount)
    {
        totalEarned += amount;
        birdBucksText.text = totalEarned.ToString();
        AudioManager.Instance.PlaySound(increaseSFX);
        animator.SetTrigger("Calculate");
    }
}
