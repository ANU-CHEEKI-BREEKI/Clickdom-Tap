using UnityEngine;
using System.Collections;

public class UserProgressEventInitializator : MonoBehaviour
{

    [SerializeField] UserProgressEvent[] eventsToInit;

    private void Start()
    {
        foreach (var item in eventsToInit)
            if (item != null)
                item.Init();
    }
}
