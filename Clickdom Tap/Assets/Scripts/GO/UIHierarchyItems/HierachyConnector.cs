using ANU.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
[ExecuteInEditMode]
public class HierachyConnector : AHierarchyBase
{
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
    
    private void Reset()
    {
        Init();
    }

    private void Update()
    {
        Init();
    }
}
