using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LaunchProjectileToPosition))]
public class ProjectileLauncherEffectUpdater : MonoBehaviour
{
    [SerializeField] private EffectId onParticleStops;
    [SerializeField] private EffectId onParticleRemoves;

    private LaunchProjectileToPosition launcher;

    private void Init()
    {
        if (launcher == null)
            launcher = GetComponent<LaunchProjectileToPosition>();
    }

    public void UpdateParticleEffect()
    {
        Init();

        var data = launcher.LaunchData;

        data.effectOnParticleStops = onParticleStops;
        data.effectOnParticleRemoves = onParticleRemoves;

        launcher.LaunchData = data;
    }
}
