using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateAtPosition : MonoBehaviour
{
    [SerializeField] private Transform position;
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform parent;
    [SerializeField] private bool startActivation = true;

    public void InstantiateAtPos()
    {
        InstantiateAtPos(position);
    }

    public void InstantiateAtPos(Transform position)
    {
        var go = Instantiate(prefab, position.position, Quaternion.identity, parent);
        go.SetActive(startActivation);
    }
}
