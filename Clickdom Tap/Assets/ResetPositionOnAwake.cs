using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetPositionOnAwake : MonoBehaviour
{
    [SerializeField] new private bool enabled = true;
    [Space]
    [SerializeField] private Vector3 resetPosition = Vector3.zero;
    [SerializeField] private bool resetZ = false;

    private void Awake()
    {
        if (!enabled) return;

        var newPos = resetPosition;
        if (!resetZ)
            newPos.z = transform.position.z;
        transform.position = newPos;
    }
}
