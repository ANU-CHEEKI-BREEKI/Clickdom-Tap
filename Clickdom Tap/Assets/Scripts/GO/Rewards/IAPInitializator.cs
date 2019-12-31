using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class IAPInitializator : MonoBehaviour
{
    [SerializeField] private UnityEvent onInitSuccess;
    [SerializeField] private UnityEvent onInitFailed;

    private void Awake()
    {
        GIAP.Instance.Init((succes) =>
        {
            if (succes)
                onInitSuccess?.Invoke();
            else
                onInitFailed?.Invoke();
        });
    }
}
