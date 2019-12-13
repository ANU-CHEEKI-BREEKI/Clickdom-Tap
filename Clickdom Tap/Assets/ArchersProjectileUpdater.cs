using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ArcherSpawner))]
public class ArchersProjectileUpdater : MonoBehaviour
{
    [SerializeField] private ShaderSpriteUvAnimationSetupData updateAnimation;

    private ArcherSpawner spawner;

    private void Init()
    {
        if(spawner == null)
            spawner = GetComponent<ArcherSpawner>();
    }

    public void UpdateProjectiles()
    {
        Init();

        if (updateAnimation != null)
            spawner.LaunchArrowData.data.animaionData = DataToComponentData.ToComponentData(updateAnimation);
    }

}
