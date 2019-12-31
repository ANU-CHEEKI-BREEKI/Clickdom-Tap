using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class UICircleProgressBar : AProgressBar
{
    [SerializeField] private Image bg;
    [SerializeField] private FloatToText f2t;
    [SerializeField] private ProgressFormat progressFormat;
    [SerializeField] private ProgressType progressType;


    private float progress;

    private bool initiated = false;

    public override bool TextVisibility { get => f2t.TextVisibility; set => f2t.TextVisibility = value; }
    public override ProgressFormat Format { get => progressFormat; set => progressFormat = value; }
    public override ProgressType Type { get => progressType; set => progressType = value; }
    
    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        if (initiated)
            return;

        initiated = true;

        f2t = GetComponent<FloatToText>();
        bg.type = Image.Type.Filled;
        SetProgress(0);
    }

    public override void SetProgress(float progress)
    {
        if (!initiated)
            Init();

        float displayProgress;
        float fill;
        if (Format == ProgressFormat.PERCENT)
        {
            var clampProgress = Mathf.Clamp01(progress);

            if (Type == ProgressType.DECREACE)
                clampProgress = 1 - clampProgress;

            fill = clampProgress;
            displayProgress = clampProgress;
        }
        else
        {
            var clampProgress = (MaxValue != MinValue) ? (progress - MinValue) / (MaxValue - MinValue) : 1;
            clampProgress = Mathf.Clamp01(clampProgress);

            if (Type == ProgressType.DECREACE)
                clampProgress = 1 - clampProgress;

            fill = clampProgress;
            displayProgress = progress;
        }

        bg.fillAmount = fill;
        f2t.Float = displayProgress;
    }
}
