using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeactivateGroup : MonoBehaviour
{
    [SerializeField] private GameObject[] toDeactivate;

    public void SetActivation(bool active)
    {
        foreach (var go in toDeactivate)
            go?.SetActive(active);
    }

    public void SetActivationReverce(bool active)
    {
        SetActivation(!active);
    }
}
