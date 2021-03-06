﻿using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class EffectSpawner : MonoBehaviour
{
    [SerializeField] private ProjectileEffectSettings effectSettings;
    [SerializeField] private AudioPlayer audioPlayer;

    private static EffectSpawner instance;

    private void Awake()
    {
        instance = this;
    }

    public static void SpawnEffect(float3 position, float scale, quaternion rotation,  EffectId id)
    {
        if (id == EffectId.NONE)
            return;

        if (id == EffectId.ARROW_HIT_SOUND)
        {
            if (instance.audioPlayer != null)
                instance.audioPlayer.PlayRandomClip(position);
        }
        else
        {
            var effect = instance.effectSettings.GetEffect(id);

            if (effect.effect == null)
                return;

            var obj = Instantiate(effect.effect, position, rotation);
            obj.transform.localScale = Vector3.one * scale;
        }
    }
}
