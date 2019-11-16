using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ZByY : MonoBehaviour
{
    [SerializeField] private ZByYSettings settings;
    private Transform _transform;

    void Start()
    {
        _transform = this.transform;
    }

# if UNITY_EDITOR
    void Update()
    {
        if (settings == null)
            return;

        var pos = _transform.position;
        pos.z = pos.y * settings.Scale + settings.ZOffset;
        _transform.position = pos;
    }
#endif
}
