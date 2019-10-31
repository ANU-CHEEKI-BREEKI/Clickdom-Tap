using UnityEngine;
using System.Collections;

[RequireComponent(typeof(UserProgressCostBinding))]
public class InteractableByCost : MonoBehaviour
{
    [Header("binding")]
    [SerializeField] private WholeProgress userScore;
    [Header("disable settings")]
    [Tooltip("to disable interactivity")]
    [SerializeField] private CanvasGroup canvasg;
    [Space]
    [SerializeField] [Range(0, 1)] private float disabledAlpha = 0.6f;

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
        SetEnadled(money >= costBinding.CostBindingSource.EvaluateRelated);
    }

    private void SetEnadled(bool enabled)
    {
        canvasg.interactable = enabled;

        canvasg.alpha = enabled ? 1 : disabledAlpha;
    }
}
