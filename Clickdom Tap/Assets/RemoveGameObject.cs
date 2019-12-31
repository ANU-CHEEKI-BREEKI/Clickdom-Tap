using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveGameObject : MonoBehaviour
{
    [SerializeField] private GameObject toDestroy;

    private void Reset()
    {
        toDestroy = gameObject;
    }

    public void DestroyObject()
    {
        Destroy(gameObject);
    }
}
