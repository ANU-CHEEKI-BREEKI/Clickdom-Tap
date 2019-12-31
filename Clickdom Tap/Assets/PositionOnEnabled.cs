using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionOnEnabled : MonoBehaviour
{
    public enum TriggerType { Awake, Enable, Start, LateUpdate, NextFrame }

    [SerializeField] private Transform targetTransform;
    [SerializeField] private Transform targetPosition;
    [Space]
    [SerializeField] private TriggerType type;

    private void Reset()
    {
        targetTransform = transform;
    }

    private void Awake()
    {
        if(type == TriggerType.Awake)
            SetPosition();
    }

    private void OnEnable()
    {
        if (type == TriggerType.Enable)
            SetPosition();
    }
   
    private void Start()
    {
        if (type == TriggerType.Start)
            SetPosition();
        else if (type == TriggerType.NextFrame)
            StartCoroutine(NextFrame());
    }

    IEnumerator NextFrame()
    {
        yield return null;
        SetPosition();
    }

    private void SetPosition()
    {
        targetTransform.position = targetPosition.position;
    }
}


