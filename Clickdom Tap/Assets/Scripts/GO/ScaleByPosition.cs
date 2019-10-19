using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ScaleByPosition : MonoBehaviour
{
    private new Camera camera;
    private Transform camTransform;
    private new Transform transform;

    [SerializeField] ScaleByPositionSettings scaleByPosSettings;

    private void OnEnable()
    {
        transform = base.transform;
    }

    void Update()
    {
        float posY = Mathf.Clamp(transform.position.y, scaleByPosSettings.MinY, scaleByPosSettings.MaxY);
        float t = (posY - scaleByPosSettings.MinY) / (scaleByPosSettings.MaxY - scaleByPosSettings.MinY);

        transform.localScale = Mathf.Lerp(scaleByPosSettings.MaxScale, scaleByPosSettings.MinScale, t) * Vector3.one;
    }
}
