using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPositionToMouse : MonoBehaviour
{
    public void SetPosToMouseWorld()
    {
        transform.position = Utils.GetMouseWorldPosition().ToV3(transform.position.z);
    }
}
