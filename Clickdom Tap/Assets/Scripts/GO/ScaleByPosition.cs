using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ScaleByPosition : MonoBehaviour
{
    private new Camera camera;
    private Transform camTransform;
    private new Transform transform;

    [SerializeField] private float minScale = 0;
    [SerializeField] private float maxScale = 1;

    private float minY;
    private float maxY;
    
    private void OnEnable()
    {
        camera = Camera.main;
        camTransform = camera.transform;
        transform = base.transform;
    }

    void Update()
    {
        var camPos = camTransform.position;
        var minY = camPos.y - camera.orthographicSize * 0.7f;
        var maxY = camPos.y + camera.orthographicSize * 0.7f;
        
        if (maxY == minY) return;

        float posY = Mathf.Clamp(transform.position.y, minY, maxY);
        float t = (posY - minY) / (maxY - minY);

        transform.localScale = Mathf.Lerp(maxScale, minScale, t) * Vector3.one;
    }
}
