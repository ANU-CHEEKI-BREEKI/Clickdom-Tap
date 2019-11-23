using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct DeathAudioClipComponentData : IComponentData
{
    public AudioClipComponentData audio;
}

[UpdateBefore(typeof(DestroyEntityWhenHealthZeroSystem))]
public class DeathSoundPlayerSystem : ComponentSystem
{
    public struct SoundPosData
    {
        public AudioClipComponentData audio;
        public float3 position;
    }

    [BurstCompile]
    struct TmplateJob : IJobForEach<HealthComponentData, DeathAudioClipComponentData, Translation>
    {
        public NativeQueue<SoundPosData>.ParallelWriter audios;

        public void Execute(ref HealthComponentData health, ref DeathAudioClipComponentData audio, ref Translation translation)
        {
            if (health.value > 0)
                return;
            audios.Enqueue(new SoundPosData()
            {
                position = translation.Value,
                audio = audio.audio
            });
        }
    }
    
    protected override void OnUpdate()
    {
        var audios = new NativeQueue<SoundPosData>(Allocator.TempJob);

        new TmplateJob()
        {
            audios = audios.AsParallelWriter()
        }.Schedule(this).Complete();

        SoundPosData data;
        while(audios.TryDequeue(out data))
        {
            var clipId = data.audio.clipId;
            if (data.audio.randRangeId)
                clipId = UnityEngine.Random.Range(data.audio.clipId, data.audio.maxClipId);
            SpawnerAudioPool.Play(data.audio.audioSourcePoolId, clipId, data.position);
        }

        audios.Dispose();
    }
}