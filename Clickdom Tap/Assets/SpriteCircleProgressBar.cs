using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteCircleProgressBar : MonoBehaviour
{
    [SerializeField] Transform bg;
    [SerializeField] Transform fg;

    private float progress;
    private Vector3 startScale;

    private bool initiated = false;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        if (initiated)
            return;

        initiated = true;
        startScale = fg.localScale;
        SetProgress(0);
    }

    public void SetProgress(float progress)
    {
        if (!initiated)
            Init();

        progress = Mathf.Clamp01(progress);
        fg.localScale = Vector3.Lerp(Vector3.zero, startScale, progress);
    }
}

