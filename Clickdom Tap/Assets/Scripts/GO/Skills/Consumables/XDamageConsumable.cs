using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;
using System;
using System.Linq;

public class XDamageConsumable : ADurationsConsumable
{
    [SerializeField] private WholeProgressHandle wholeProgress;
    [SerializeField] [Min(0.1f)] private float damageMultiplier = 2;

    private IEnumerable<ArchersProjectileUpdater> archUpdaters;
    private IEnumerable<ProjectileLauncherEffectUpdater> trebEffectsUpdaters;
    private IEnumerable<LaunchProjectileToPosition> trebLaunchers;
    
    protected override void OnStartUse()
    {
        //fire visual effects
        archUpdaters = FindObjectsOfType<ArchersProjectileUpdater>();
        foreach (var apdater in archUpdaters)
            apdater.UpdateProjectiles(this);

        trebEffectsUpdaters = FindObjectsOfType<ProjectileLauncherEffectUpdater>();
        foreach (var apdater in trebEffectsUpdaters)
            apdater.UpdateParticleEffect(this);

        trebLaunchers = FindObjectsOfType<LaunchProjectileToPosition>().Where(l=>l.GetComponent<TrebuchetShooter>()!= null);
        foreach (var launcher in trebLaunchers)
            launcher.DrawTrails = true;

        //actual damage scale
        wholeProgress.DamageMultiplier *= damageMultiplier;
    }

    protected override void OnEndUse()
    {
        //fire visual effects
        foreach (var apdater in archUpdaters)
                apdater.DowngradeProjectiles(this);

        foreach (var apdater in trebEffectsUpdaters)
            apdater.DowngradeParticleEffect(this);

        foreach (var launcher in trebLaunchers)
            launcher.DrawTrails = false;

        //actual damage scale
        wholeProgress.DamageMultiplier /= damageMultiplier;
    }
}
