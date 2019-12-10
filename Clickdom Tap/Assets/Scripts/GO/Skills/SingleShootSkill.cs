using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LaunchProjectileToPosition))]
public class SingleShootSkill : ATargetedSkill
{
    [SerializeField] protected SkillDescription description;

    [SerializeField] protected bool setZByY;
    [SerializeField] protected ZByYSettings settings;
    [SerializeField] protected bool scaleByPos;
    [SerializeField] protected ScaleByPositionSettings scaleSettings;
    [Space]
    [SerializeField] protected Vector3 startPositionoffset = new Vector3(-25, 25, 0);

    private LaunchProjectileToPosition launcher;

    public override void ExecuteAt(Vector3 position)
    {
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
    }

    protected void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var pos = Utils.GetMouseWorldPosition().ToV3();
            ExecuteAt(pos);
        }
    }
}
