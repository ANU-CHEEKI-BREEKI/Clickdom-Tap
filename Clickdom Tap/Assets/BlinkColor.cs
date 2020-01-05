using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlinkColor : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] sprites;
    [SerializeField] private Gradient randomColor = new Gradient()
    {
        alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1, 0) },
        colorKeys = new GradientColorKey[] { new GradientColorKey(Color.white, 0) }
    };

    private void Reset()
    {
        ResetSprites();
    }

    [ContextMenu("ResetSprites")]
    private void ResetSprites()
    {
        var sprites = new LinkedList<SpriteRenderer>();

        var stack = new Stack<Transform>();
        stack.Push(transform);
        while (stack.Any())
        {
            var transform = stack.Pop();
            var upe = transform.GetComponent<SpriteRenderer>();
            if (upe != null)
                sprites.AddLast(upe);

            var childCount = transform.childCount;
            for (int i = childCount - 1; i >= 0; i--)
                stack.Push(transform.GetChild(i));
        }
        this.sprites = sprites.ToArray();
    }

    public void Blink(float t)
    {
        foreach (var sprite in sprites)
            sprite.color = randomColor.Evaluate(t);
    }
}
