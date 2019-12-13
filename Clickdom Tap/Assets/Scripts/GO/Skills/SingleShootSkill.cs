using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LaunchProjectileToPosition))]
public class SingleShootSkill : ATargetedSkill, IDamageSettable
{
    [SerializeField] protected SkillDescription description;

    [SerializeField] protected bool setZByY;
    [SerializeField] protected ZByYSettings settings;
    [SerializeField] protected bool scaleByPos;
    [SerializeField] protected ScaleByPositionSettings scaleSettings;
    [Space]
    [SerializeField] protected Vector3 startPositionoffset = new Vector3(-25, 25, 0);
    [Space]
    [SerializeField] private float damageSettableScaler = 10;
    [Space]
    [Header("Rewrite Trails")]
    [Tooltip("Если это поле установлено, то вместо DrawTrails будет установлена данная анимация")]
    [SerializeField] private ShaderSpriteUvAnimationSetupData animationRewriteTrails;

    private ShaderSpriteUvAnimationSetupData initAnimation;


    private LaunchProjectileToPosition launcher;

    public override void ExecuteAt(Vector3 position)
    {
        CallOnSkillExecutionStartEvent();

        if (setZByY)
            position.z = position.y * settings.Scale;

        var scale = 1f;
        if (scaleByPos)
            scale = scaleSettings.LerpEvaluete(position);

        launcher.Launch(
            from: position + startPositionoffset,
            to: position,
            scale
        );      
    }

    public override SkillDescription Description => description;

    private void Awake()
    {
        launcher = GetComponent<LaunchProjectileToPosition>();
        initAnimation = launcher.ProjectileRenderData;
    }

    protected void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var pos = Utils.GetMouseWorldPosition().ToV3();
            ExecuteAt(pos);
        }
    }

    void IDamageSettable.SetDamage(float damage)
    {
        var realDamage = damage * damageSettableScaler;
        launcher.ForceSetDamage(realDamage);



        if (animationRewriteTrails == null)
            launcher.DrawTrails = damage > 1;
        else
            launcher.ProjectileRenderData = damage > 1 ? animationRewriteTrails : initAnimation;
    }
}
