using UnityEngine;
using System.Collections;
using System;

public abstract class AProgressBar : MonoBehaviour
{
    public enum ProgressFormat { PERCENT, ACTUAL_VALUE }
    public enum ProgressType { INCREACE, DECREACE }

    public abstract ProgressFormat Format { get; set; }
    public abstract ProgressType Type { get; set; }
    public abstract bool TextVisibility { get; set; }

    public abstract void SetProgress(float progress);

    public float MaxValue { get; set; } = 1;
    public float MinValue { get; set; } = 0;
}
