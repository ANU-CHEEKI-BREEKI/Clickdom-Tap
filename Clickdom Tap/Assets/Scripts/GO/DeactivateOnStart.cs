using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeactivateOnStart : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("DeactivateOnStart");
        gameObject.SetActive(false);
    }
}
