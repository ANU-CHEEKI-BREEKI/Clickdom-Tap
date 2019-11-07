using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HoldButton : MonoBehaviour
{
    [System.Serializable]
    public class FloatEvent : UnityEvent<float> { }

    [Header("Timing")]
    [SerializeField] private float holdDuration = 1f;
    [SerializeField] private float clickMaxDuration = 0.5f;

    [Header("On execute by hold button")]
    [SerializeField] private UnityEvent onExecute;

    [Header("On execute by click button")]
    [SerializeField] private UnityEvent onClickExecute;

    [Header("Callbacks")]
    [SerializeField] private UnityEvent onBeginHold;
    [SerializeField] private UnityEvent onEntHold;

    [Header("Progress")]
    [SerializeField] private FloatEvent onHoldProgress;

    private Coroutine holdRoutine = null;
    private float holdTimer;

    private void OnValidate()
    {
        if (holdDuration <= 0)
            holdDuration = 1;
        if (clickMaxDuration <= 0)
            clickMaxDuration = 0.5f;
        if (holdDuration <= clickMaxDuration)
            holdDuration = clickMaxDuration + 1;
    }

    public void StartHold()
    {
        onBeginHold.Invoke();

        holdRoutine = StartCoroutine(HoldRoutine());
    }

    public void EndHold()
    {
        if (holdRoutine != null)
        {
            StopCoroutine(holdRoutine);
            holdRoutine = null;
        }

        if (holdTimer <= clickMaxDuration)
            ExecuteClickEvent();

        onEntHold.Invoke();

        onHoldProgress.Invoke(0);
    }

    private void ExecuteEvent()
    {
        onExecute.Invoke();
    }

    private void ExecuteClickEvent()
    {
        onClickExecute.Invoke();
    }

    private IEnumerator HoldRoutine()
    {
        holdTimer = 0f;
        while (holdTimer < holdDuration)
        {
            yield return null;
            holdTimer += Time.deltaTime;
            onHoldProgress.Invoke(holdTimer / holdDuration);
        }

        onHoldProgress.Invoke(1f);

        ExecuteEvent();

        holdRoutine = null;
    }
}