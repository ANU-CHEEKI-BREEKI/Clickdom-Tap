using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LaunchProjectileToPosition))]
public class ProjectileLauncherEffectUpdater : MonoBehaviour
{
    [SerializeField] private EffectId onParticleStops;
    [SerializeField] private EffectId onParticleRemoves;

    private EffectId lastOnParticleStops;
    private EffectId lastOnParticleRemoves;

    /// <summary>
    /// если пользоватль сам вкачал огненные снаряды, то по жтому полю мы сможем это узнать и не вызывать downgrade
    /// </summary>
    private object lastCaller = null;

    private LaunchProjectileToPosition launcher;

    private void Init()
    {
        if (launcher == null)
            launcher = GetComponent<LaunchProjectileToPosition>();
    }

    public void UpdateParticleEffect()
    {
        UpdateParticleEffect(null);
    }

    public void UpdateParticleEffect(object caller)
    {
        lastCaller = caller;

        Init();

        var data = launcher.LaunchData;

        CacheLastData(data);

        data.effectOnParticleStops = onParticleStops;
        data.effectOnParticleRemoves = onParticleRemoves;

        launcher.LaunchData = data;
    }

    public void DowngradeParticleEffect()
    {
        DowngradeParticleEffect(this);
    }

    public void DowngradeParticleEffect(object caller)
    {
        if (lastCaller != caller)
            return;

        var data = launcher.LaunchData;

        data.effectOnParticleStops = lastOnParticleStops;
        data.effectOnParticleRemoves = lastOnParticleRemoves;
    }

    private void CacheLastData(ProjectileLaunshSetupComponentData data)
    {
        lastOnParticleStops = data.effectOnParticleStops;
        lastOnParticleRemoves = data.effectOnParticleRemoves;
    }
}
