using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ArcherSpawner))]
public class ArchersProjectileUpdater : MonoBehaviour
{
    [SerializeField] private ShaderSpriteUvAnimationSetupData updateAnimation;

    private ArcherSpawner spawner;

    protected SpriteSheetAnimationComponentData oldAnimation;

    private object lastCaller;

    private void Init()
    {
        if (spawner == null)
            spawner = GetComponent<ArcherSpawner>();
        CacheAnimationData();
    }

    private void CacheAnimationData()
    {
        oldAnimation = spawner.LaunchArrowData.data.animaionData;
    }

    public void UpdateProjectiles()
    {
        UpdateProjectiles(null);
    }

    public void UpdateProjectiles(object caller)
    {
        lastCaller = caller;

        Init();

        if (updateAnimation != null)
            spawner.LaunchArrowData.data.animaionData = DataToComponentData.ToComponentData(updateAnimation);
    }

    public void DowngradeProjectiles()
    {
        DowngradeProjectiles(null);
    }

    public void DowngradeProjectiles(object caller)
    {
        if (lastCaller != caller)
            return;

        spawner.LaunchArrowData.data.animaionData = oldAnimation;
    }
}
