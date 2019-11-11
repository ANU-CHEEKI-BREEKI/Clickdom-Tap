using UnityEngine;
using System.Collections;
using System;

public abstract class ASpawnerEffect : MonoBehaviour
{
    public event Action OnEffectEnds;

    public abstract void Play();
}
