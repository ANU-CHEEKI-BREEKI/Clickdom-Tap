using UnityEngine;
using System.Collections;

public class SilverPackReward : AUserReward
{
    [SerializeField] private WholeProgress progress;

    protected override void Reward()
    {
        //так как весь доход умножается на MoneyEarnMultiplier, то надо тут разделить
        //тогда в итоге получим нужное значение награды
        var reward = rewardValue;
        if (reward > 0)
            reward /= progress.MoneyEarnMultiplier;
        progress.Money += reward;
    }
}
