using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class WholeProgress : ASavable
{
    [SerializeField] private Progress wholeProgress = new Progress(0, float.MaxValue);
    [SerializeField] private float money = 1;
    [SerializeField] [Range(0, 2)] private float moneyEarnMultiplier = 1f;
    public float MoneyEarnMultiplier { get => moneyEarnMultiplier; set => moneyEarnMultiplier = Mathf.Clamp(value, 0, 2); }

    public Progress Progress => wholeProgress;
    public float Money
    {
        get { return money; }
        set
        {
            var old = money;
            var add = value - old;
            if (add > 0)
                money += add * MoneyEarnMultiplier;
            else
                money += add;

            if(old != money)
                OnMoneyChanged?.Invoke(money);
        }
    }

    public event Action<float> OnMoneyChanged;

    private void Start()
    {
        wholeProgress.ValueChanged += (newVal, oldVal) =>
        {
            if (newVal > oldVal)
                Money += newVal - oldVal;
        };
    }
}
