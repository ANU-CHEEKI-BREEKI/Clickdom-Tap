using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class UserProgressEventInitializator : MonoBehaviour
{
    [SerializeField] private UserProgressEvent[] eventsToInit;

    private void Start()
    {
            Init();
    }

    private void Init()
    {
        foreach (var item in eventsToInit)
            if (item != null)
                item.Init();
    }

    private void Reset()
    {
        if(eventsToInit == null || eventsToInit.Length == 0)
        {
            var events = new LinkedList<UserProgressEvent>();

            var stack = new Stack<Transform>();
            stack.Push(transform);
            while(stack.Any())
            {
                var transform = stack.Pop();
                var upe = transform.GetComponent<UserProgressEvent>();
                if (upe != null)
                    events.AddLast(upe);

                var childCount = transform.childCount;
                for (int i = childCount - 1; i >= 0; i--)
                    stack.Push(transform.GetChild(i));
            }

            eventsToInit = events.ToArray();
        }
    }
}
