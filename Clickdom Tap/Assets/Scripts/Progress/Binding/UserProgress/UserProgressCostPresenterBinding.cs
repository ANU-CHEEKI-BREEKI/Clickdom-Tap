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

    private UserProgressCostBinding costBinding;

    private void Start()
    {
        costBinding = GetComponent<UserProgressCostBinding>();
        costBinding.CostBindingSource.OnNewCostExpected += CostBindingSource_OnNewCostExpected;

        CostBindingSource_OnNewCostExpected(0);
    }

    private void OnDestroy()
    {
        costBinding.CostBindingSource.OnNewCostExpected -= CostBindingSource_OnNewCostExpected;
    }

    private void CostBindingSource_OnNewCostExpected(float newCost)
    {
        text.text = newCost.ToString();
    }

}
