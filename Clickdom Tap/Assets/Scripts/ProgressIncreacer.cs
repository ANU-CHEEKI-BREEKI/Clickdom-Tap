using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressIncreacer : MonoBehaviour, IDamageSettable
{
    [SerializeField] private WholeProgressHandle progress;
    [SerializeField] private float damage;
    [SerializeField] private Transform applyPosition;

    private void Start()
    {
        if (applyPosition == null)
            applyPosition = transform;
    }

    public void IncreaceProgress()
    {
        progress?.IncreaceProgressInPlace(damage, applyPosition.position);
    }

    void IDamageSettable.SetDamage(float damage)
    {
        this.damage = damage;
    }
}
