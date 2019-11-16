using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class FloatToText : MonoBehaviour
{
    [SerializeField] private string format = "";

    private TextMeshProUGUI textmesh;


    public float Float
    {
        set
        {
            textmesh.text = value.ToString(format);
        }
    }

    private void Start()
    {
        textmesh = GetComponent<TextMeshProUGUI>();
    }


}
