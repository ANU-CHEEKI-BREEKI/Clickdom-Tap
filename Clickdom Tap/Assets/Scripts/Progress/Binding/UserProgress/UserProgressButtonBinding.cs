using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(UserProgressBinding), typeof(UserProgressCostBinding))]
public class UserProgressButtonBinding : MonoBehaviour
{
    [Header("binding value")]
    [SerializeField] protected float value;
    [Header("interactable bindins")]
    [SerializeField] [Range(0, 1)] private float disabledAlpha = 0.6f;
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
            SetEnabled(binding.BindingSource.PercentValue < 1);

            score.Money -= cost;
        });

        SetEnabled(binding.BindingSource.PercentValue < 1);
    }

    private void SetEnabled(bool enabled)
    {
        canvasg.interactable = enabled;

        foreach (var go in toDeactivate)
            go?.SetActive(enabled);

        canvasg.alpha = enabled ? 1 : disabledAlpha;
    }
}
