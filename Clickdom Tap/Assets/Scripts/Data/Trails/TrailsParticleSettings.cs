using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "TrailsParticleSettings")]
public class TrailsParticleSettings : ScriptableObject
{
    [System.Serializable]
    public class TrailsSpriteData
    {
        public TrailSpriteDataNamedId id;
        public ShaderSpriteUvAnimationSetupData spriteData;
    }

    [SerializeField] private TrailsSpriteData[] datas;
    private Dictionary<TrailSpriteDataNamedId, RenderSharedComponentData> renderData = new Dictionary<TrailSpriteDataNamedId, RenderSharedComponentData>();
    private Dictionary<TrailSpriteDataNamedId, SpriteSheetAnimationComponentData> animationData = new Dictionary<TrailSpriteDataNamedId, SpriteSheetAnimationComponentData>();

    public RenderSharedComponentData GetRenderData(TrailSpriteDataNamedId id)
    {
        if (!renderData.ContainsKey(id))
        {
            var spriteData = datas.Where(d => d.id == id).First();
            renderData.Add(id, new RenderSharedComponentData() { material = spriteData.spriteData.Material, mesh = spriteData.spriteData.Mesh });
        }
        return renderData[id];
    }

    public SpriteSheetAnimationComponentData GetAnimationData(TrailSpriteDataNamedId id)
    {
        if (!animationData.ContainsKey(id))
        {
            var spriteData = datas.Where(d => d.id == id).First();
            animationData.Add(id, DataToComponentData.ToComponentData(spriteData.spriteData));
        }
        return animationData[id];
    }

    public ShaderSpriteUvAnimationSetupData GetSetupData(TrailSpriteDataNamedId id)
    {
         return datas.Where(d => d.id == id).First().spriteData;
    }

}