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

    public void InstantiateAtPosMultiple(Transform position, int count, float delay)
    {
        StartCoroutine(MultipleInstanriate(position, count, delay));
    }

    private IEnumerator MultipleInstanriate(Transform position, int count, float delay)
    {
        for (int i = 0; i < count; i++)
        {
            InstantiateAtPos(position);
            yield return new WaitForSeconds(delay);
        }
    }
}
