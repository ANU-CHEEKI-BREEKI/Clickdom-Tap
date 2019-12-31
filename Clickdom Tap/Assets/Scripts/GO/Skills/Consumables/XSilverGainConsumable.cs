using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class XSilverGainConsumable : ADurationsConsumable
{
    [SerializeField] private WholeProgress wholeProgress;
    [SerializeField] [Min(0.1f)] private float silverGainMultiplier = 2;

    protected override void OnStartUse()
    {
        wholeProgress.MoneyEarnMultiplier *= silverGainMultiplier;
    }

    protected override void OnEndUse()
    {
        wholeProgress.MoneyEarnMultiplier /= silverGainMultiplier;
    }
}
