using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class IdentityRotation : MonoBehaviour
{
    [SerializeField] Quaternion rotation = Quaternion.identity;

    private Transform _transform;

    private void Start()
    {
        _transform = transform;
    }

    private void Update()
    {
        _transform.rotation = rotation;
    }
}
