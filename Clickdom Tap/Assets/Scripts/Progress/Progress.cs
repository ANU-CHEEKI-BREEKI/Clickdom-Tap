using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class Progress
{
    [SerializeField] private float maxProgress;
    [SerializeField] private float progressValue;

    public float MaxValue => maxProgress;
    public float Value
    {
        get { return progressValue; }
        set
        {
            var old = progressValue;
            progressValue = Mathf.Clamp(value, 0, maxProgress);
            if (old != progressValue)
                ValueChanged?.Invoke(progressValue, old);
        }
    }
    public float PercentValue => progressValue / maxProgress;

    public delegate void ValueChangedEventHandler(float newValue, float oldValue);
    public event ValueChangedEventHandler ValueChanged;

    public Progress() : this(0, 10_000)
    {
    }

    public Progress(float progress, float maxProgress)
    {
        this.maxProgress = maxProgress;
        this.progressValue = progress;
    }
}