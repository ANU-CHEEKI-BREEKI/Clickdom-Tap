using UnityEngine;
using System.Collections;
using TMPro;
using System;
using static UserProgressBinding;

[RequireComponent(typeof(UserProgressBinding))]
public class UserProgressCostBinding : MonoBehaviour
{
    [Header("cost binding source")]
    [SerializeField] protected UserProgressCost costSource;

    public ProgressCost CostBindingSource { get; protected set; }

    private UserProgressBinding binding;

    private void Start()
    {
        if (binding == null)
            binding = GetComponent<UserProgressBinding>();

        switch (binding.Path)
        {
            case BindingPath.COUNT:
                CostBindingSource = costSource.Count;
                break;
            case BindingPath.DAMAGE:
                CostBindingSource = costSource.Damage;
                break;
            case BindingPath.FREQUENCY:
                CostBindingSource = costSource.SpawnRate;
                break;
            case BindingPath.SPEED:
                CostBindingSource = costSource.AttackSpeed;
                break;
            default:
                throw new NotImplementedException(binding.Path.ToString());
        }
    }
}
