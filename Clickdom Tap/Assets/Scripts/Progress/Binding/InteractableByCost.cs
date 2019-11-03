using UnityEngine;
using System.Collections;

[RequireComponent(typeof(UserProgressCostBinding))]
public class InteractableByCost : MonoBehaviour
{
    [Header("binding")]
    [SerializeField] private WholeProgress userScore;
    [Header("disable settings")]
    [Tooltip("to disable interactivity")]
    [SerializeField] private InteractableByPriority interact;
    [SerializeField] private int disablePriority = 0;

    private UserProgressCostBinding costBinding;

    private void Start()
    {
        costBinding = GetComponent<UserProgressCostBinding>();

        userScore.OnMoneyChanged += UserScore_OnMoneyChanged;

        UserScore_OnMoneyChanged(userScore.Money);
    }

    private void OnDestroy()
    {
        userScore.OnMoneyChanged -= UserScore_OnMoneyChanged;
    }

    private void UserScore_OnMoneyChanged(float money)
    {
        SetEnabled(money >= costBinding.CostBindingSource.EvaluateRelated);
    }

    private void SetEnabled(bool enabled)
    {
        interact.SetEnabled(enabled, disablePriority);
    }
}
