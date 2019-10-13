using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

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
public struct AnimationListSharedComponentData : ISharedComponentData, IEquatable<AnimationListSharedComponentData>
{
    public SpriteSheetAnimationComponentData?[] animations;
    public PauseData?[] pauses;
    public int?[] actions;

    public bool Equals(AnimationListSharedComponentData other)
    {
        return ReferenceEquals(animations, other.animations) && ReferenceEquals(pauses, other.pauses);
    }

    public override int GetHashCode()
    {
        return animations.GetHashCode() * pauses.GetHashCode();
    }
}

public struct AnimatorStatesComponentData :IComponentData
{
    public bool running;
    public bool shooting;
    public bool climbing;
    public bool fighting;
    public bool death;
    public bool jumping;
    public bool falling;

    public bool stateChangedEventFlag;
}