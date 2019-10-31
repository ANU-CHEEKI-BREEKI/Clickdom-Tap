using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(UserProgress))]
public class ProgressUpgradeObserver : MonoBehaviour
{
    private IEnumerable<ICountSettable> counts;
    private IEnumerable<IFrequencySettable> frequencys;
    private IEnumerable<IDamageSettable> damages;
    private IEnumerable<ISpeedSettable> speeds;

    private UserProgress progress;

    private void Start()
    {
        progress = GetComponent<UserProgress>();

        counts = GetComponents<ICountSettable>();
        frequencys = GetComponents<IFrequencySettable>();
        damages = GetComponents<IDamageSettable>();
        speeds = GetComponents<ISpeedSettable>();

        Subscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void SetCount(float count, float oldVale)
    {
        foreach (var item in counts)
            item?.SetCount(count);
    }

    private void SetDamage(float damage, float oldVale)
    {
        foreach (var item in damages)
            item?.SetDamage(damage);
    }

    private void SetFrequency(float frequency, float oldVale)
    {
        foreach (var item in frequencys)
            item?.SetFrequency(frequency);
    }

    private void SetSpeed(float speed, float oldVale)
    {
        foreach (var item in speeds)
            item?.SetSpeed(speed);
    }

    private void Subscribe()
    {
        progress.Count.ValueChanged += SetCount;
        progress.Damage.ValueChanged += SetDamage;
        progress.SpawnRate.ValueChanged += SetFrequency;
        progress.AttackSpeed.ValueChanged += SetSpeed;
    }

    private void Unsubscribe()
    {
        progress.Count.ValueChanged -= SetCount;
        progress.Damage.ValueChanged -= SetDamage;
        progress.SpawnRate.ValueChanged -= SetFrequency;
        progress.AttackSpeed.ValueChanged -= SetSpeed;
    }
}

public interface ICountSettable
{
    void SetCount(float count);
}
public interface IFrequencySettable
{
    void SetFrequency(float frequency);
}
public interface IDamageSettable
{
    void SetDamage(float damage);
}
public interface ISpeedSettable
{
    void SetSpeed(float speed);
}