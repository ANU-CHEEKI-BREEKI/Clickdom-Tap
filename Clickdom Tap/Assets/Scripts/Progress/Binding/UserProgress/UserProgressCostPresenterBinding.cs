using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System;

[RequireComponent(typeof(UserProgressCostBinding))]
[RequireComponent(typeof(FloatToText))]
public class UserProgressCostPresenterBinding : MonoBehaviour
{
    private UserProgressCostBinding costBinding;
    private FloatToText f2t;

    private void Start()
    {
        f2t = GetComponent<FloatToText>();

        costBinding = GetComponent<UserProgressCostBinding>();
        costBinding.CostBindingSource.OnNewCostExpected += CostBindingSource_OnNewCostExpected;

        Init();
    }

    private void Init()
    {
        CostBindingSource_OnNewCostExpected(costBinding.CostBindingSource.EvaluateRelated);
    }

    private void OnDestroy()
    {
        if(costBinding != null)
            costBinding.CostBindingSource.OnNewCostExpected -= CostBindingSource_OnNewCostExpected;
    }

    private void CostBindingSource_OnNewCostExpected(float newCost)
    {
        f2t.Float = newCost;
    }

}
