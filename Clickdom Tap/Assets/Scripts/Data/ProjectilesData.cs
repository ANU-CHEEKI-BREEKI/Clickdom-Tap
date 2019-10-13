using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "ProjectilesData")]
public class ProjectilesData : ScriptableObject
{
    [SerializeField] private ShaderSpriteUvAnimationSetupData animation;
    [SerializeField] private ProjectileCollisionComponentData collision;
    [SerializeField] private ProjectileLaunshSetupComponentData launch;

    public ShaderSpriteUvAnimationSetupData Animation => animation;
    public ProjectileCollisionComponentData Collision => collision;
    public ProjectileLaunshSetupComponentData Launch => launch;
}
