using UnityEngine;
using System.Collections;
using System;

public class UserProgressBinding : MonoBehaviour
{
    public enum BindingPath { COUNT, DAMAGE, FREQUENCY, SPEED }

    [Header("binding source")]
    [SerializeField] protected UserProgress source;
    [Header("binding path")]
    [SerializeField] protected BindingPath path;

    public BindingPath Path => path;
    public Progress BindingSource { get; protected set; }

    virtual protected void Start()
    {
        switch (path)
        {
            case BindingPath.COUNT:
                BindingSource = source.Count;
                break;
            case BindingPath.DAMAGE:
                BindingSource = source.Damage;
                break;
            case BindingPath.FREQUENCY:
                BindingSource = source.SpawnRate;
                break;
            case BindingPath.SPEED:
                BindingSource = source.AttackSpeed;
                break;
            default:
                throw new NotImplementedException(path.ToString());
        }
    }
}
