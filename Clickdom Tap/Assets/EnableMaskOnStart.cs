using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Mask))]
public class EnableMaskOnStart : MonoBehaviour
{
    [SerializeField] private bool enable = true;

    private void Start()
    {
        GetComponent<Mask>().enabled = enable;
    }
}
