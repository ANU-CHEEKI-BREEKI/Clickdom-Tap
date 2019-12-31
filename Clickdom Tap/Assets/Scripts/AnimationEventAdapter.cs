using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimationEventAdapter : MonoBehaviour
{
    [SerializeField] UnityEvent onExecute;

    [SerializeField] UnityEvent[] onExecuteByIndex;

    public void Execute()
    {
        onExecute.Invoke();
    }

    public void ExecuteByIndex(int index)
    {
        onExecuteByIndex[index].Invoke();
    }
}
