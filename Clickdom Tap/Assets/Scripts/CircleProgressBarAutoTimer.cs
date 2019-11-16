using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteCircleProgressBar))]
public class CircleProgressBarAutoTimer : MonoBehaviour
{
    private SpriteCircleProgressBar bar;

    [SerializeField] private float duration = 1;
    public float Duration
    {
        get => duration;
        set
        {
            duration = value;
            if (duration <= 0)
                duration = 1;
        }
    }
    private float timer;

    private void OnValidate()
    {
        if (duration <= 0)
            duration = 1;
    }

    private void Awake()
    {
        bar = GetComponent<SpriteCircleProgressBar>();
    }

    private void OnEnable()
    {
        bar.SetProgress(0);
        timer = 0;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > duration)
            timer = duration;
        bar.SetProgress(timer / duration);
    }
}
