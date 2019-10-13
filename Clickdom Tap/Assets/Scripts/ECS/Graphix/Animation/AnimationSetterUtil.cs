using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;

public static class AnimationSetterUtil
{
    public static bool SetAnimation(EntityManager manager, Entity entity, AnimationType animation, int? randomInitFrameRange = null)
    {
        if(manager.HasComponent<AnimationListSharedComponentData>(entity))
        {
            var animList = manager.GetSharedComponentData<AnimationListSharedComponentData>(entity);

            if (animList.animations == null) return false;

            var anim = animList.animations[(int)animation];
            if(anim.HasValue)
            {
                if (!manager.HasComponent<SpriteSheetAnimationComponentData>(entity))
                    manager.AddComponent<SpriteSheetAnimationComponentData>(entity);

                var val = anim.Value;
                if(randomInitFrameRange != null)
                {
                    var range = math.clamp(randomInitFrameRange.Value, 0, val.frameCount);
                    val.currentFrame = UnityEngine.Random.Range(0, range);
                }

                manager.SetComponentData(entity, val);

                if (animList.pauses != null)
                {
                    var pause = animList.pauses[(int)animation];
                    if (pause.HasValue)
                    {
                        if (!manager.HasComponent<AnimationPauseComponentData>(entity))
                            manager.AddComponent<AnimationPauseComponentData>(entity);
                        manager.SetComponentData(entity, new AnimationPauseComponentData()
                        {
                            needPause = true,
                            pauseData = pause.Value
                        });
                    }
                    else if (manager.HasComponent<AnimationPauseComponentData>(entity))
                    {
                        var p = manager.GetComponentData<AnimationPauseComponentData>(entity);
                        p.needPause = false;
                        manager.SetComponentData(entity, p);
                    }
                }

                if (animList.actions != null)
                {
                    var action = animList.actions[(int)animation];
                    if (action.HasValue)
                    {
                        if (!manager.HasComponent<ActionOnAnimationFrameComponentData>(entity))
                            manager.AddComponent<ActionOnAnimationFrameComponentData>(entity);
                        manager.SetComponentData(entity, new ActionOnAnimationFrameComponentData()
                        {
                            needAction = true,
                            frame = action.Value
                        });
                    }
                    else if (manager.HasComponent<ActionOnAnimationFrameComponentData>(entity))
                    {
                        var p = manager.GetComponentData<ActionOnAnimationFrameComponentData>(entity);
                        p.needAction = false;
                        manager.SetComponentData(entity, p);
                    }
                }

                return true;
            }
        }
        return false;
    }
}