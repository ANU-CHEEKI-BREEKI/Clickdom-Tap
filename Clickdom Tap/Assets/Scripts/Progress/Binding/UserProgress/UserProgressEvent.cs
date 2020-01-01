using UnityEngine;
using System.Collections;
using UnityEngine.Events;

[RequireComponent(typeof(AUserProgressBindingBase))]
public class UserProgressEvent : MonoBehaviour
{
    public enum EventType { LESS, LESS_OR_EQUALS, EQUALS, GREATER_OR_EQUALS, GREATER }

    [SerializeField] private float targetValue;
    [SerializeField] private EventType conditionTrigger;
    [SerializeField] private bool executeOnce;
    [Header("event. не оаботает если установлен eventGtoup.")]
    [SerializeField] private UnityEvent onTriggerEvent;
    [Space]
    [SerializeField] private UserProgressEventGroup eventGtoup;
    [Space]
    [SerializeField] bool initOnStart = true;
    [SerializeField] bool initByInitializator = true;

    private AUserProgressBindingBase binding;

    private void Start()
    {
        if (eventGtoup != null)
            eventGtoup.AddEvent(this);

        binding = GetComponent<AUserProgressBindingBase>();
        Subscribe();

        if(!initByInitializator)
            Init();
    }

    private void OnDestroy()
    {
        if (eventGtoup != null)
            eventGtoup.RemoveEvent(this);

        Unsubscribe();
    }

    private void Subscribe()
    {
        binding.BindingSource.ValueChanged += BindingSource_ValueChanged;
    }

    private void Unsubscribe()
    {
        if (binding != null)
            binding.BindingSource.ValueChanged -= BindingSource_ValueChanged;
    }

    public void Init()
    {
        if (initOnStart)
            BindingSource_ValueChanged(binding.BindingSource.Value, binding.BindingSource.Value);
    }

    private void BindingSource_ValueChanged(float newValue, float oldValue)
    {
        switch (conditionTrigger)
        {
            case EventType.LESS:
                if (newValue < targetValue)
                    TriggerEvent();
                break;
            case EventType.LESS_OR_EQUALS:
                if (newValue <= targetValue)
                    TriggerEvent();
                break;
            case EventType.EQUALS:
                if (newValue == targetValue)
                    TriggerEvent();
                break;
            case EventType.GREATER_OR_EQUALS:
                if (newValue >= targetValue)
                    TriggerEvent();
                break;
            case EventType.GREATER:
                if (newValue > targetValue)
                    TriggerEvent();
                break;
            default:
                break;
        }
    }

    private void TriggerEvent()
    {
        if (eventGtoup != null)
        {
            eventGtoup.TriggerEvent(this);
        }
        else
        {
            onTriggerEvent.Invoke();
        }
        if (executeOnce)
            Unsubscribe();
    }


}
