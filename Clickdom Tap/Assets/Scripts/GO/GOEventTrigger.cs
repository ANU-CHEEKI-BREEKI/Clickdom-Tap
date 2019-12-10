using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GOEventTrigger : MonoBehaviour
{
    public UnityEvent OnAwaked;
    public UnityEvent OnStarted;
    public UnityEvent OnEnabled;
    public UnityEvent OnDisabled;
    public UnityEvent OnDestroyed;

    private void Awake()
    {
        OnAwaked?.Invoke();
    }

    private void Start()
    {
        OnStarted?.Invoke();
    }

    private void OnEnable()
    {
        OnEnabled?.Invoke();
    }

    private void OnDisable()
    {
        OnDisabled?.Invoke();
    }

    private void OnDestroy()
    {
        OnDestroyed?.Invoke();
    }
}