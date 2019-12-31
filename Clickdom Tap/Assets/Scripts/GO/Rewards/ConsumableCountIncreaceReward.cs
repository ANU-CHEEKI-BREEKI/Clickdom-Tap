using UnityEngine;
using System.Collections;

public class ConsumableCountIncreaceReward : AUserReward
{
    [SerializeField] protected ADurationsConsumable consumable;

    protected override void Reward()
    {
        consumable.IncreaceCount((int)rewardValue);
    }
}
