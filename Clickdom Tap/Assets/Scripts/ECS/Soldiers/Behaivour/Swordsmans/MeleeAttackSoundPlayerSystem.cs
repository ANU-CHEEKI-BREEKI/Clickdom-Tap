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

[UpdateAfter(typeof(DetectAnimationActionSystem))]
public class MeleeAttackSoundPlayerSystem : ComponentSystem
{
    public struct SoundPosData
    {
        public AudioClipComponentData audio;
        public float3 position;
    }

    [BurstCompile]
    [RequireComponentTag(typeof(SwordsmanTagComponentData), typeof(MeleeAttackComponentData))]
    struct TmplateJob : IJobForEach<ActionOnAnimationFrameComponentData, AudioClipComponentData, Translation>
    {
        public NativeQueue<SoundPosData>.ParallelWriter audios;

        public void Execute([ReadOnly]ref ActionOnAnimationFrameComponentData action, [ReadOnly] ref AudioClipComponentData audio, [ReadOnly] ref Translation translation)
        {
            if (!action.out_ActionFlag)
                return;
            audios.Enqueue(new SoundPosData()
            {
                audio = audio,
                position = translation.Value
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