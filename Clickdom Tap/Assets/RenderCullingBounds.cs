using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class RenderCullingBounds : MonoBehaviour
{
    [SerializeField] private Rect additionaBounds = Rect.zero;
    [Space]
    [SerializeField] private bool drawGismosAlways = true;

    private Camera _camera = null;

    public Rect CullingBounds
    {
        get
        {
            if(_camera == null)
                _camera = GetComponent<Camera>();

            var camHeight = _camera.orthographicSize * 2;
            var camWidth = camHeight * _camera.aspect;

            var bounds = new Rect();
            bounds.size = new Vector2(camWidth, camHeight) + additionaBounds.size;
            bounds.center = (Vector2)_camera.transform.position + additionaBounds.position;

            return bounds;
        }
    }

    private void Reset()
    {
        ResetCullingBounds();
    }

    [ContextMenu(nameof(ResetCullingBounds))]
    private void ResetCullingBounds()
    {
        additionaBounds = Rect.zero;
    }

    private void OnDrawGizmos()
    {
        if(drawGismosAlways)
            DrawGizmos();
    }

    private void OnDrawGizmosSelected()
    {
        DrawGizmos();
    }

    private void DrawGizmos()
    {
        var bounds = CullingBounds;

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}
