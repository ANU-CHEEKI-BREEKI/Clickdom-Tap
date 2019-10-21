using UnityEngine;
using System.Collections;
using System;
using System.Linq;

[CreateAssetMenu(fileName = "AnimationList")]
public class AnimationList : ScriptableObject
{
    [SerializeField] private ShaderSpriteUvAnimationSetupData IDLE_animation;
    [SerializeField] private ShaderSpriteUvAnimationSetupData RUN_animation;
    [SerializeField] private ShaderSpriteUvAnimationSetupData CLIMB_animation;
    [SerializeField] private ShaderSpriteUvAnimationSetupData FIGHT_animation;
    [SerializeField] private ShaderSpriteUvAnimationSetupData SHOOT_animation;
    [SerializeField] private ShaderSpriteUvAnimationSetupData DEATH_animation;
    [SerializeField] private ShaderSpriteUvAnimationSetupData JUMP_animation;
    [SerializeField] private ShaderSpriteUvAnimationSetupData FALL_animation;

    [NonSerialized]
    private ShaderSpriteUvAnimationSetupData[] animationsSource;

    [NonSerialized]
    private WrapSpriteSheetAnimationComponentData[] animations;
    public WrapSpriteSheetAnimationComponentData[] Animations
    {
        get
        {
            if (animations == null)
            {                
                InitAnimationSourceArray();
                animations = new WrapSpriteSheetAnimationComponentData[animationsSource.Length];
                for (int i = 0; i < animationsSource.Length; i++)
                {
                    if (animationsSource[i] != null)
                    {
                        animations[i] = new WrapSpriteSheetAnimationComponentData(
                            DataToComponentData.ToComponentData(animationsSource[i])
                        );
                    }
                }
            }
            return animations;
        }
    }

    [NonSerialized]
    private WrapRefPauseData[] pauseData;
    public WrapRefPauseData[] PausesData
    {
        get
        {
            if (pauseData == null)
            {
                InitAnimationSourceArray();
                pauseData = new WrapRefPauseData[animationsSource.Length];
                for (int i = 0; i < animationsSource.Length; i++)
                {
                    if (animationsSource[i] != null && animationsSource[i].NeedPauseOnSomeFrames)
                    {
                        pauseData[i] = new WrapRefPauseData(
                            animationsSource[i].PauseData
                        );
                    }
                }
            }
            return pauseData;
        }
    }

    [NonSerialized]
    private WrapActionData[] actionsData;
    public WrapActionData[] ActionsData
    {
        get
        {
            if (actionsData == null)
            {
                InitAnimationSourceArray();
                actionsData = new WrapActionData[animationsSource.Length];
                for (int i = 0; i < animationsSource.Length; i++)
                {
                    if (animationsSource[i] != null && animationsSource[i].NeedActionOnSomeFrames)
                    {
                        actionsData[i] = new WrapActionData(
                            animationsSource[i].ActionData
                        );
                    }
                }
            }
            return actionsData;
        }
    }


    public WrapSpriteSheetAnimationComponentData FirstAnimation => Animations.Where(a => a.HasValue).FirstOrDefault();

    private void InitAnimationSourceArray()
    {
        if (animationsSource == null)
        {
            var fields = this.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var indices = Enum.GetValues(typeof(AnimationType));

            animationsSource = new ShaderSpriteUvAnimationSetupData[indices.Length];

            foreach (var index in indices)
            {
                if ((int)index >= animationsSource.Length)
                    continue;

                var field = fields
                    .Where(f => f.Name.StartsWith(index.ToString()))
                    .FirstOrDefault();

                if (field != null)
                {
                    var fieldValue = field.GetValue(this) as ShaderSpriteUvAnimationSetupData;
                    if (fieldValue != null)
                        animationsSource[(int)index] = fieldValue;
                }
            }
        }
    }

}
