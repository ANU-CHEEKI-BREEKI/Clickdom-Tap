using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class ProgressCost
{
    [SerializeField] float minCost;
    [SerializeField] float maxCost;
    [SerializeField] AnimationCurve costCurve;

    private Progress relatedProgress;
    public Progress RelatedProgress
    {
        get { return relatedProgress; }
        set
        {
            if(relatedProgress != null)
                relatedProgress.ValueChanged -= RelatedProgress_ValueChanged;
            relatedProgress = value;
            if (relatedProgress != null)
                relatedProgress.ValueChanged += RelatedProgress_ValueChanged;
        }
    }

    private void RelatedProgress_ValueChanged(float newValue, float oldValue)
    {
        OnNewCostExpected?.Invoke(EvaluateRelated);
    }

    public float EvaluateRelated => Evaluate(RelatedProgress.PercentValue);

    public event Action<float> OnNewCostExpected;

    public float MinCost => minCost;
    public float MaxCost => maxCost;

    public ProgressCost(float minCost, float maxCost)
    {
        this.minCost = minCost;
        this.maxCost = maxCost;

        if(minCost > maxCost)
        {
            var t = minCost;
            minCost = maxCost;
            maxCost = t;
        }
    }

    public float Evaluate(float t) => Mathf.Lerp(minCost, maxCost, costCurve.Evaluate(t));
}
