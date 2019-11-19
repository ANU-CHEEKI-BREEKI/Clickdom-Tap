using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class DelayStartAnimation : MonoBehaviour
{
    [SerializeField] [Range(0, 10)] private float delaySeconds;
    [SerializeField] private bool randomDelay;
    [SerializeField] [Range(0, 10)] private float randomRangeSeconds = 1;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.enabled = false;

        var delay = delaySeconds;
        if (randomDelay)
            delay += Random.Range(-randomRangeSeconds, randomRangeSeconds);

        Invoke("Resume", delay);
    }

    private void Resume()
    {
        animator.enabled = true;
    }
}
