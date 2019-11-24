using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SetAnchorsByRectEditorExtention : MonoBehaviour
{
    [MenuItem("Extensions/UI/SetAnchorsByRect", priority = 1000)]
    private static void SetAnchorsByRect()
    {
        var rectTransforms = Selection.objects
            .Where(o => o is GameObject)
            .Select(o => (o as GameObject).GetComponent<RectTransform>())
            .Where(rt => rt != null);

        var _camera = Camera.main;
        var screenSize = new Vector2()
        {
            x = _camera.pixelWidth,
            y = _camera.pixelHeight
        };

        foreach (var rt in rectTransforms)
            SetAnchorsByRect(_camera, screenSize, rt);
    }

    private static void SetAnchorsByRect(Camera _camera, Vector2 screenSize, RectTransform rt)
    {
        var rect = rt.rect;
        var aPos = rt.anchoredPosition;
        var pos = rt.position;
        var screenPos = _camera.WorldToScreenPoint(pos);
        rect.center = screenPos;

        rt.anchorMax = rect.max / screenSize;
        rt.anchorMin = rect.min / screenSize;

        rt.anchoredPosition = Vector2.zero;

        rt.sizeDelta = Vector2.zero;
        rt.position = pos;

#if UNITY_EDITOR
        EditorUtility.SetDirty(rt);
#endif
    }
}
