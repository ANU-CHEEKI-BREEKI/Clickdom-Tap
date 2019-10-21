using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LaunchProjectileToPosition))]
public class LaunchMisileToMousePos : MonoBehaviour
{
    [SerializeField] bool setZByY;
    [SerializeField] ZByYSettings settings;
    [SerializeField] bool scaleByPos;
    [SerializeField] ScaleByPositionSettings scaleSettings;

    private LaunchProjectileToPosition launcher;

    private void Start()
    {
        launcher = GetComponent<LaunchProjectileToPosition>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var pos = Utils.GetMouseWorldPosition().ToV3();

            if (setZByY)
                pos.z = pos.y * settings.Scale;

            var scale = 1f;
            if (scaleByPos)
                scale = scaleSettings.LerpEvaluete(pos);

            launcher.Launch(
                from: pos + new Vector3(-40, 20),
                to:   pos,
                scale
            );
        }
    }
}
