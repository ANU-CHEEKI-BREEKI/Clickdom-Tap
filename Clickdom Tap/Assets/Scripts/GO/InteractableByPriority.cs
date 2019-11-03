using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(CanvasGroup))]
public class InteractableByPriority : MonoBehaviour
{
    [SerializeField] [Range(0, 1)] float disabledAlpha = 0.6f;

    private CanvasGroup canvasg;

    Dictionary<int, bool> interactable = new Dictionary<int, bool>();

    private void Start()
    {
        canvasg = GetComponent<CanvasGroup>();
    }

    /// <param name="enabled"></param>
    /// <param name="priority">большее значение приоритетнее</param>
    public void SetEnabled(bool enabled, int priority = 0)
    {
        if (!interactable.ContainsKey(priority))
            interactable.Add(priority, enabled);
        else
            interactable[priority] = enabled;

        var keys = interactable.Keys.OrderByDescending(k => k);
        var interact = true;
        foreach (var key in keys)
            interact &= interactable[key];

        SetEnabled(interact);
    }

    private void SetEnabled(bool enabled)
    {
        canvasg.interactable = enabled;
        canvasg.alpha = enabled ? 1 : disabledAlpha;
    }
}
