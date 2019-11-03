using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(UserProgressBinding), typeof(UserProgressCostBinding))]
public class UserProgressButtonBinding : MonoBehaviour
{
    [Header("binding value")]
    [SerializeField] protected float value;
    [SerializeField] protected UserProgressBinding[] additionalBindings;

    [Header("interactable bindins")]
    [SerializeField] private bool bindingIntecactable = true;
    [SerializeField] private InteractableByPriority interact;
    [SerializeField] private int disablePriority = 0;
    [SerializeField] private GameObject[] toDeactivate;
    [Header("button")]
    [SerializeField] private Button button;
    [SerializeField] private CanvasGroup canvasg;
    [Header("user score")]
    [SerializeField] private WholeProgress score;

    private UserProgressBinding binding;
    private UserProgressCostBinding costBinding;

    private void Start()
    {
        binding = GetComponent<UserProgressBinding>();
        costBinding = GetComponent<UserProgressCostBinding>();

        button.onClick.AddListener(() =>
        {
            var cost = costBinding.CostBindingSource.EvaluateRelated;
            if (score.Money < cost)
                return;

            binding.BindingSource.Value += value;
            foreach (var addbinding in additionalBindings)
                addbinding.BindingSource.Value += value;

            if (bindingIntecactable)
                SetEnabled(binding.BindingSource.PercentValue < 1);

            score.Money -= cost;
        });

        if (bindingIntecactable)
            SetEnabled(binding.BindingSource.PercentValue < 1);
    }

    private void SetEnabled(bool enabled)
    {
        interact.SetEnabled(enabled, disablePriority);

        foreach (var go in toDeactivate)
            go?.SetActive(enabled);
    }
}
