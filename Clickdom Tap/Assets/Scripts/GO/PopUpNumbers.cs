using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class PopUpNumbers : MonoBehaviour
{
    [Serializable]
    public struct FloatData
    {
        public Vector2 floatDirection;
        public float floatSpeed;
        [Range(0.01f, 5)] public float lifetime;
        [Range(0.01f, 5)] public float scale;
        public Color color;
        [Range(0, 1)] public float aplhaAtDestroy;
    }

    public static PopUpNumbers Instance { get; private set; }

    [SerializeField] private ShaderSpriteUvAnimationSetupData numbersSpriteSheet;
    [SerializeField] private FloatData defaultData;

    public bool PopUpEnabled { get; set; } = true;

    private RenderSharedComponentData renderData;

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        renderData = new RenderSharedComponentData()
        {
            material = numbersSpriteSheet.Material,
            mesh = numbersSpriteSheet.Mesh,
        };
    }
    
    public void WriteLine(int number, Vector3 worldPosition)
    {
        WriteLine(number, worldPosition, defaultData);
    }    

    public void WriteLine(int number, Vector3 worldPosition, FloatData floatData)
    {
        if (!PopUpEnabled)
            return;

        if (number < 0)
            throw new ArgumentException(nameof(number) + " must be greater or equals zero");

        var manager = World.Active.EntityManager;

        int q = 0;
        foreach (var ch in number.ToString())
        {
            var entity = manager.CreateEntity();
            manager.AddComponentData(entity, new Translation()
            {
                Value = worldPosition + q * new Vector3(1 * floatData.scale, 0, 0)
            });
            manager.AddComponentData(entity, new Scale()
            {
                Value = floatData.scale
            });
            manager.AddComponentData(entity, new DestroyEntityWithDelayComponentData()
            {
                delay = floatData.lifetime
            });
            manager.AddComponentData(entity, new VelocityComponentData()
            {
                value = floatData.floatDirection.normalized * floatData.floatSpeed
            });
            manager.AddComponentData(entity, new SpriteRendererComponentData()
            {
                uv = numbersSpriteSheet.GetUvForFrame(ch - 48)
            });
            manager.AddComponentData(entity, new SpriteTintComponentData()
            {
                color = floatData.color
            });
            manager.AddComponentData(entity, new AlphaAnimationComponentData()
            {
                startAlpha = floatData.color.a,
                endAlpha = floatData.aplhaAtDestroy,
                duration = floatData.lifetime
            });
            manager.AddComponent<UniformMotiontagComponentData>(entity);
            manager.AddComponent<UseOnlyInstancedRendererTagComponentData>(entity);
            manager.AddSharedComponentData(entity, renderData);
            q++;
        }
    }
}
