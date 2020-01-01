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
    [SerializeField] private FloatToText rewardValueText;
    [SerializeField] private UnityEvent onRewardEarned;

    private void Start()
    {
        if (rewardValueText != null)
            rewardValueText.Float = rewardValue;
    }

    public void EarnReward()
    {
        Reward();
        onRewardEarned?.Invoke();
    }

    protected abstract void Reward();

}
