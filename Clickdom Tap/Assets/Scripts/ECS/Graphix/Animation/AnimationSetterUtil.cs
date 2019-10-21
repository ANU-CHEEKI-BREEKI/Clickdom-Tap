using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public enum AnimationType
{
    IDLE,
    RUN,
    CLIMB,
    FIGHT,
    SHOOT,
    DEATH,
    JUMP,
    FALL
}

[Serializable]
public struct WrapSpriteSheetAnimationComponentData
{
    public WrapSpriteSheetAnimationComponentData(SpriteSheetAnimationComponentData value)
    {
        this.value = value;
        HasValue = true;
    }

    public readonly bool HasValue;

    public SpriteSheetAnimationComponentData value;
}
[Serializable]
public struct WrapRefPauseData
{
    public WrapRefPauseData(PauseData value)
    {
        this.value = value;
        HasValue = true;
    }

    public readonly bool HasValue;

    public PauseData value;
}
[Serializable]
public struct WrapActionData
{
    public WrapActionData(ActionData value)
    {
        this.value = value;
        HasValue = true;
    }

    public readonly bool HasValue;

    public ActionData value;
}

[Serializable]
public struct AnimationListSharedComponentData : ISharedComponentData, IEquatable<AnimationListSharedComponentData>
{
    public WrapSpriteSheetAnimationComponentData[] animations;
    [Space]
    public WrapRefPauseData[] pauses;
    [Space]
    public WrapActionData[] actions;

    public bool Equals(AnimationListSharedComponentData other)
    {
        return     ReferenceEquals(animations, other.animations) 
                && ReferenceEquals(pauses, other.pauses)
                && ReferenceEquals(actions, other.actions);
    }

    public override int GetHashCode()
    {
        return animations.GetHashCode() * pauses.GetHashCode() * actions.GetHashCode();
    }
}

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

                var val = anim.value;
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
                            pauseData = pause.value
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
                            actionData = action.value
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