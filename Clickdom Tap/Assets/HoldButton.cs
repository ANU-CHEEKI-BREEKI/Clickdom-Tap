using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HoldButton : MonoBehaviour
{
    [System.Serializable]
    public class FloatEvent : UnityEvent<float> { }

    [SerializeField] private float holdDuration = 1f;
    [Space]
    [SerializeField] private UnityEvent onExecute;
    [Space]
    [SerializeField] private UnityEvent onBeginHold;
    [SerializeField] private UnityEvent onEntHold;
    [Space]
    [SerializeField] private FloatEvent onHoldProgress;

    private Coroutine holdRoutine = null;

    public void StartHold()
    {
        onBeginHold.Invoke();

        holdRoutine = StartCoroutine(HoldRoutone());
    }

    public void EndHold()
    {
        if (holdRoutine != null)
        {
            StopCoroutine(holdRoutine);
            holdRoutine = null;
        }

        onEntHold.Invoke();

        onHoldProgress.Invoke(0);
    }

    private void ExecuteEvent()
    {
        onExecute.Invoke();
    }

    private IEnumerator HoldRoutone()
    {
        var timer = 0f;
        while(timer < holdDuration)
        {
            yield return null;
            timer += Time.deltaTime;
            onHoldProgress.Invoke(timer / holdDuration);
        }

        onHoldProgress.Invoke(1f);

        ExecuteEvent();

        holdRoutine = null;
    }
}
