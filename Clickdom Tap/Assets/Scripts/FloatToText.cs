using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

//[RequireComponent(typeof(TextMeshProUGUI))]
public class FloatToText : MonoBehaviour
{
    public enum NumberFormat { CSHARP, SHORT }
    [SerializeField] private NumberFormat formatting;
    [SerializeField] private string csharpFormat = "";
    [Space]
    [SerializeField]  private TextMeshProUGUI textmesh;

    public float Float
    {
        set
        {
            var text = "";
            if (formatting == NumberFormat.CSHARP)
                text = value.ToString(csharpFormat);
            else
                text = value.ToShortFormattedString();
            textmesh.text = text;
        }
    }

    public bool TextVisibility { get => textmesh.enabled; set => textmesh.enabled = value; }

    private void Reset()
    {
        Init();
    }

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        if (textmesh == null)
            textmesh = GetComponent<TextMeshProUGUI>();
    }
}
