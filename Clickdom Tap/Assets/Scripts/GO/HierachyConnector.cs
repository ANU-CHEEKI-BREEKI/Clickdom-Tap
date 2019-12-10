using ANU.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
[ExecuteInEditMode]
public class HierachyConnector : MonoBehaviour
{
    private RectTransform _transform;
    private CanvasGroup _canvasGroup;

    public void Connect(Vector2 from, Vector2 to)
    {
        var dist = Vector3.Distance(from, to);
        _transform.sizeDelta = new Vector2(_transform.sizeDelta.x, dist);

        _transform.pivot = new Vector2(0.5f, 0);
        _transform.anchoredPosition = from;

        var dir = from.GetDirectionTo(to);
        var angle = Math.AngleDegrees(dir);
        var rot = _transform.rotation;
        rot.eulerAngles = rot.eulerAngles.RewriteZ(angle);

        _transform.rotation = rot;
    }

    public void SetActive(bool active)
    {
        _canvasGroup.alpha = active ? 1 : 0.6f;
        _canvasGroup.blocksRaycasts = active;
    }

    private void Reset()
    {
        Init();
    }

    private void Update()
    {
        Init();
    }

    private void Init()
    {
        if (_transform == null)
            _transform = transform as RectTransform;

        if (_canvasGroup == null)
            _canvasGroup = GetComponent<CanvasGroup>();
    }
}
