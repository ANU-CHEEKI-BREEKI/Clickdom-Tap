using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public abstract class AUserReward : MonoBehaviour
{
    [SerializeField] protected float rewardValue;
    [SerializeField] private UnityEvent onRewardEarned;

    public void EarnReward()
    {
        Reward();
        onRewardEarned?.Invoke();
    }

    protected abstract void Reward();

}
