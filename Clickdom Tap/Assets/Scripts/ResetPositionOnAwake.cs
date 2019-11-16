using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetPositionOnAwake : MonoBehaviour
{
    public enum TriggerType { ON_AWAKE, ON_START}
    public enum ResetType { POS, ANCHORED_POS}

    [SerializeField] new private bool enabled = true;
    [SerializeField] private TriggerType type = TriggerType.ON_AWAKE;
    [SerializeField] private ResetType resetType = ResetType.POS;
    [Space]
    [Header("position")]
    [SerializeField] private Vector3 position = Vector3.zero;
    [SerializeField] private bool resetZ = false;
    [Space]
    [Header("anchors")]
    [SerializeField] private Vector2 anchoredPosition = Vector2.zero;

    private void Awake()
    {
        if(type == TriggerType.ON_AWAKE)
            Reset();
    }

    private void Start()
    {
        if (type == TriggerType.ON_START)
            Reset();
    }
    
    private void Reset()
    {
        if (!enabled) return;

        if (resetType == ResetType.POS)
            ResetPosition();
        if(resetType == ResetType.ANCHORED_POS)
            ResetAnchors();
    }

    [ContextMenu("ResetPosition")]
    private void ResetPosition()
    {
        var newPos = position;
        if (!resetZ)
            newPos.z = transform.position.z;
        transform.position = newPos;
    }

    [ContextMenu("ResetAnchors")]
    private void ResetAnchors()
    {
        var rt = transform as RectTransform;
        if (rt == null)
            return;
        rt.anchoredPosition = anchoredPosition;
    }
}
