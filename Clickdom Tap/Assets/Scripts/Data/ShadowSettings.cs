using UnityEngine;
using System.Collections;
using Unity.Mathematics;

[CreateAssetMenu(fileName = "ShadowSettings")]
public class ShadowSettings : ScriptableObject
{
    [SerializeField] private CastSpritesShadowComponentData shadowsData = new CastSpritesShadowComponentData()
    {
        color = Color.black,
        positionPercentOffset = new float3(0,-1,0),
        scale = new float2(1, -0.581f)
    };

    public CastSpritesShadowComponentData ShadowsData => shadowsData;
}
