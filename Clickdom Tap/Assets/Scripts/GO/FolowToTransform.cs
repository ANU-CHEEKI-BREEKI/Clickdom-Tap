using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FolowToTransform : MonoBehaviour
{
    [SerializeField] private Transform toFolow;
    [SerializeField] private bool onStartToo = true;

    private Transform _transfrom;

    private void Start()
    {
        _transfrom = transform;

        if (!onStartToo)
            Folow();
    }

    private void OnEnable()
    {
        Folow();
    }

    private void Update()
    {
        Folow();
    }

    private void Folow()
    {
        if(_transfrom != null)
            _transfrom.position = toFolow.position;
    }
}
