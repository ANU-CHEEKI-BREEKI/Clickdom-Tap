using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(SpriteRenderer))]
public class ShadowColor : MonoBehaviour
{
    [SerializeField] private ShadowSettings settings;
    [SerializeField] private bool asCustomMaterialColor = false;

    private SpriteRenderer _renderer;

    private void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (settings == null)
            return;

        if (!asCustomMaterialColor)
        {
            _renderer.color = settings.ShadowsData.color;
        }
        else
        {
            var mpb = new MaterialPropertyBlock();
            _renderer.GetPropertyBlock(mpb);
            mpb.SetColor("_Color", settings.ShadowsData.color);
            _renderer.SetPropertyBlock(mpb);
            //_renderer.material.Matero .SetColor("_Color", settings.ShadowsData.color);
        }
    }
}
