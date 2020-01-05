using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blinker : MonoBehaviour
{
    [SerializeField] private BlinkColor[] colorBlinks;

    [SerializeField] private AnimationCurve randomBlinkPeriod = AnimationCurve.Linear(0, .2f, 1, .2f);

    private float nextBlinkElapced = 0;

    private void Update()
    {
        if (nextBlinkElapced > 0)
            nextBlinkElapced -= Time.deltaTime;
        else
            Blink();
    }

    private void Blink()
    {
        var t = Random.value;

        foreach (var cb in colorBlinks)
            cb?.Blink(t);

        UpdateTimer();
    }

    private void UpdateTimer()
    {
        var t = Random.value;
        nextBlinkElapced = randomBlinkPeriod.Evaluate(t);
    }
}
