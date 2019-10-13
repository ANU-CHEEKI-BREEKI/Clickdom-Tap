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
    
    private SpriteSheetAnimationComponentData?[] animations;
    public SpriteSheetAnimationComponentData?[] Animations
    {
        get
        {
            if (animations == null)
            {                
                InitAnimationSourceArray();
                animations = new SpriteSheetAnimationComponentData?[animationsSource.Length];
                for (int i = 0; i < animationsSource.Length; i++)
                {
                    if(animationsSource[i] != null)
                        animations[i] = DataToComponentData.ToComponentData(animationsSource[i]);
                }
            }
            return animations;
        }
    }

    private PauseData?[] pauseData;
    public PauseData?[] PausesData
    {
        get
        {
            if (pauseData == null)
            {
                InitAnimationSourceArray();
                pauseData = new PauseData?[animationsSource.Length];
                for (int i = 0; i < animationsSource.Length; i++)
                {
                    if (animationsSource[i] != null && animationsSource[i].NeedPauseOnSomeFrames)
                        pauseData[i] = animationsSource[i].PauseData;
                }
            }
            return pauseData;
        }
    }

    private int?[] actionsData;
    public int?[] ActionsData
    {
        get
        {
            if (actionsData == null)
            {
                InitAnimationSourceArray();
                actionsData = new int?[animationsSource.Length];
                for (int i = 0; i < animationsSource.Length; i++)
                {
                    if (animationsSource[i] != null && animationsSource[i].NeedActionOnSomeFrames)
                        actionsData[i] = animationsSource[i].ActionFrame;
                }
            }
            return actionsData;
        }
    }


    public SpriteSheetAnimationComponentData? FirstAnimation => Animations.Where(a => a.HasValue).FirstOrDefault();

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
