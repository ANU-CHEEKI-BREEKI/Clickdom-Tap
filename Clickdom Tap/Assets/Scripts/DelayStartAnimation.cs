using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class DelayStartAnimation : MonoBehaviour
{
    [SerializeField] [Range(0, 10)] private float delaySeconds;
    [SerializeField] private bool randomDelay;
    [SerializeField] [Range(0, 10)] private float randomRangeSeconds = 1;

    private void Awake()
    {
        var anamator = GetComponent<Animator>();
        anamator.enabled = false;

        var del = delaySeconds;
        if (randomDelay)
            del += Random.Range(-randomRangeSeconds, randomRangeSeconds);

        StartCoroutine(StartAnimationWithDelay(del, anamator));
    }

    private IEnumerator StartAnimationWithDelay(float delay, Animator animator)
    {
        yield return new WaitForSeconds(delay);
        animator.enabled = true;
    }
}
