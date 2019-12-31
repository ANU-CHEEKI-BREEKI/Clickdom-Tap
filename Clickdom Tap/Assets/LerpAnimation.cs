using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LerpAnimation : MonoBehaviour
{
    [SerializeField] private bool playOnAwake = true;
    [Space]
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Transform targetPositionTransform;
    [Space]
    [SerializeField] private float duration = 2f;
    [SerializeField] private Vector2 slowmotionDirection = new Vector2(0, 2);
    [Space]
    [SerializeField] private bool resetStartPosAtPlay = true;
    [SerializeField] private AnimationCurve spedCurve;
    [SerializeField] private AnimationCurve scaleCurve;
    [SerializeField] private UnityEvent onStartPlay;
    [SerializeField] private UnityEvent onEndPlay;

    private Vector3 startPos;
    private float timer;

    private bool isPlaying = false;

    private void Awake()
    {
        if (playOnAwake)
            Play();
        startPos = targetTransform.position;
    }

    private void Reset()
    {
        targetTransform = transform as RectTransform;
    }

    private void Update()
    {
        if (isPlaying)
        {
            if (timer < duration)
            {
                timer += Time.deltaTime;

                var normalizedTimer = timer / duration;
                var time = spedCurve.Evaluate(normalizedTimer);

                MoveTransitionPhase(startPos, time);
                transform.localScale = scaleCurve.Evaluate(time) * Vector3.one;
            }
            else
            {
                isPlaying = false;

                onEndPlay?.Invoke();
            }
        }
    }

    [ContextMenu("Play")]
    public void Play()
    {
        if(!resetStartPosAtPlay)
            startPos = targetTransform.position;
        timer = 0f;

        isPlaying = true;

        onStartPlay?.Invoke();
    }

    private void MoveTransitionPhase(Vector3 startPos, float normalizedTimer)
    {
        Vector3 targetPos = GetTransitionPosition(startPos);
        targetTransform.position = LerpSqr(startPos, targetPos, targetPositionTransform.position, normalizedTimer);
    }

    private Vector3 GetTransitionPosition(Vector3 startPos)
    {
        Vector3 pos;
        if (targetTransform is RectTransform)
            pos = startPos + (Vector3)(slowmotionDirection * Utils.ScreenToWorld((targetTransform as RectTransform).rect.height * Vector2.one).x);
        else
            pos = startPos + (Vector3)slowmotionDirection;
        return pos;
    }

    private Vector3 LerpSqr(Vector3 startPos, Vector3 middlePos, Vector3 destinationPos, float normalizedTimer)
    {
        var p1 = Vector3.Lerp(startPos, middlePos, normalizedTimer);
        var p2 = Vector3.Lerp(startPos, destinationPos, normalizedTimer);
        return Vector3.Lerp(p1, p2, normalizedTimer);
    }

    private void OnDrawGizmos()
    {
        var middlepos = GetTransitionPosition(targetTransform.position);
        Gizmos.DrawSphere(middlepos, 0.2f);
        Gizmos.DrawLine(targetTransform.position, middlepos);
    }
}
