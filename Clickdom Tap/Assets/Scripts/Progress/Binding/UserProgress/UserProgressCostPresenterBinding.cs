using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System;

[RequireComponent(typeof(UserProgressCostBinding))]
public class UserProgressCostPresenterBinding : MonoBehaviour
{
    [Header("presenter")]
    [SerializeField] protected TextMeshProUGUI text;
    [SerializeField] private string format = "";

    private UserProgressCostBinding costBinding;

    private void Start()
    {
        costBinding = GetComponent<UserProgressCostBinding>();
        costBinding.CostBindingSource.OnNewCostExpected += CostBindingSource_OnNewCostExpected;

        CostBindingSource_OnNewCostExpected(costBinding.CostBindingSource.EvaluateRelated);
    }

    private void OnDestroy()
    {
        costBinding.CostBindingSource.OnNewCostExpected -= CostBindingSource_OnNewCostExpected;
    }

    private void CostBindingSource_OnNewCostExpected(float newCost)
    {
        text.text = newCost.ToString(format);
    }

}
