using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinearProgressBar : MonoBehaviour
{
    public enum Direction { HORISONTAL, VERTICAL }

    [SerializeField] private bool invertDisplayProgress;
    [SerializeField] private Transform fill;
    [SerializeField] private Direction direction;

    public float Prorgess
    {
        set
        {
            var val = Mathf.Clamp01(value);

            var dispProgress = invertDisplayProgress ? 1 - val : val;
            var scale = fill.localScale;

            if (direction == Direction.HORISONTAL)
                scale.x = dispProgress;
            else
                scale.y = dispProgress;

            fill.localScale = scale;
        }
    }

    private void Start()
    {
        Prorgess = 0;
    }

   
}
