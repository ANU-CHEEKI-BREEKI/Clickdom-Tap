using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(WholeProgress))]
public class WholeProgressHandle : MonoBehaviour
{
    [Serializable]
    public class FloatEvent : UnityEvent<float> { }

    [SerializeField] Rect triggerZone;
    [SerializeField] Material caslteMaerial;
    [SerializeField] float disolveMin = 0.26f;
    [SerializeField] float disolveMax = 1;

    [SerializeField] private FloatEvent wholeProgressChanged = new FloatEvent();
    [SerializeField] private FloatEvent moneyChanged = new FloatEvent();

    private WholeProgress progress;
    private Coroutine batchRoutine;

    private Wrap<float> progressButch = null;

    private void Start()
    {
        progress = GetComponent<WholeProgress>();
        var system = World.Active.GetOrCreateSystem<WholeProgressIncreacesByParticlesSystem>();
        system.Init(triggerZone, this);

        var meleeSystem = World.Active.GetOrCreateSystem<WholeProgressIncreacerByMeleeAttack>();
        meleeSystem.Init(this);

        progress.Progress.ValueChanged += Progress_ValueChanged;
        progress.OnMoneyChanged += Progress_OnMoneyChanged;

        Progress_ValueChanged(0, 0);
    }

    private void OnDestroy()
    {
        progress.Progress.ValueChanged -= Progress_ValueChanged;
        progress.OnMoneyChanged -= Progress_OnMoneyChanged;

        if (batchRoutine != null)
            StopCoroutine(batchRoutine);
    }

    private void Progress_ValueChanged(float newValue, float oldValue)
    {
        var val = progress.Progress.PercentValue;
        var disolve = Mathf.Lerp(disolveMax, disolveMin, val);

        caslteMaerial.SetFloat("_Disolve", disolve);

        wholeProgressChanged.Invoke(val);
    }

    private void Progress_OnMoneyChanged(float obj)
    {
        moneyChanged.Invoke(obj);
    }

    public void IncreaceProgressInPlace(float damage, Vector3 position, float batchDelay = 1f)
    {
        PopUpNumbers.Instance.WriteLine((int)damage, position);

        if(progressButch == null)
        {
            progressButch = new Wrap<float>() { value = damage };
            batchRoutine = StartCoroutine(IncreaceProgressButched(batchDelay, progressButch, progress.Progress));
        }
        else
            progressButch.value += damage;
    }

    private class Wrap<T>
    {
        public T value;
    }

    private IEnumerator IncreaceProgressButched(float batchDelay, Wrap<float> progressButch, Progress actualProgress)
    {
        yield return new WaitForSeconds(batchDelay);
        actualProgress.Value += progressButch.value;
        batchRoutine = null;
        progressButch = null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(triggerZone.center, triggerZone.size);
    }
}
