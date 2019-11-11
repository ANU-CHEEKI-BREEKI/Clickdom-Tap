using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Linq;

public class UserProgressEventGroup : MonoBehaviour
{
    [SerializeField] private UnityEvent onAllTriggersEvent;

    private Dictionary<UserProgressEvent, bool> events = new Dictionary<UserProgressEvent, bool>();

    public void TriggerEvent(UserProgressEvent upEvent)
    {
        if (events.ContainsKey(upEvent))
        {
            events[upEvent] = true;

            if (events.Values.All(e => e == true))
                onAllTriggersEvent.Invoke();
        }
    }

    public void AddEvent(UserProgressEvent upEvent)
    {
        if (!events.ContainsKey(upEvent))
            events.Add(upEvent, false);
    }

    public void RemoveEvent(UserProgressEvent upEvent)
    {
        if (events.ContainsKey(upEvent))
            events.Remove(upEvent);
    }

}
