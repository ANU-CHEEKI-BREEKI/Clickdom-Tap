using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class AnimationPauser : MonoBehaviour, ISpeedSettable
{
    [Serializable]
    private class FloatEvent : UnityEvent<float> { }

    [SerializeField] private float pauseDuration;
    [SerializeField] private FloatEvent onPauseProgress;
    [SerializeField] private UnityEvent onPauseStarts;
    [SerializeField] private UnityEvent onPauseEnds;

    private Animator animator;
    private Coroutine routine;
    private bool isPaused;

    private void OnValidate()
    {
        if (pauseDuration < 0)
            pauseDuration = 0;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();

        onPauseEnds?.Invoke();
    }

    private void OnDestroy()
    {
        if (routine != null)
            StopCoroutine(routine);
    }

    public void PauseAnimation()
    {
        routine = StartCoroutine(PauseAnimatorRoutine(pauseDuration));
    }

    public void ResumeAnimation()
    {
        isPaused = false;
    }

    private IEnumerator PauseAnimatorRoutine(float pauseDurationSeconds)
    {
        var oldSpeed = animator.speed;
        animator.speed = 0;

        isPaused = true;
        onPauseStarts?.Invoke();

        var timer = 0f;

        onPauseProgress?.Invoke(0);

        while (timer < pauseDurationSeconds && isPaused)
        {
            yield return null;
            timer += Time.deltaTime;

            onPauseProgress?.Invoke(timer / pauseDurationSeconds);
        }

        onPauseProgress?.Invoke(1);

        onPauseEnds?.Invoke();

        animator.speed = oldSpeed;
        isPaused = false;
    }

    void ISpeedSettable.SetSpeed(float speed)
    {
        var pauseDuration = 1f;

        if (speed != 0)
            pauseDuration = 60f / speed;

        this.pauseDuration = pauseDuration;
    }
}
