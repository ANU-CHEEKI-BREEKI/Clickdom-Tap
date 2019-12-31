using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class DelayAnimatorStartByWindXCoord : MonoBehaviour
{
    [SerializeField] private float delay = 0.2f;
    [SerializeField] private float range = 1f;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.enabled = false;

        StartCoroutine(DelayAndPlay(delay));
    }

    private IEnumerator DelayAndPlay(float delay)
    {
        var actualDelay = Mathf.Sign(transform.position.x * Mathf.Deg2Rad * range) * delay;
        yield return new WaitForSeconds(actualDelay);
        animator.enabled = true;
    }
}
