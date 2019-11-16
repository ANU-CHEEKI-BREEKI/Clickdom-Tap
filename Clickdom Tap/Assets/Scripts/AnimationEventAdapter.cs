using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimationEventAdapter : MonoBehaviour
{
    [SerializeField] UnityEvent onExecute;

    public void Execute()
    {
        onExecute.Invoke();
    }
}
