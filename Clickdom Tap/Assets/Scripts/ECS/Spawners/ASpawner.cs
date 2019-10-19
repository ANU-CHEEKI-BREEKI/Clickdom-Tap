using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public abstract class ASpawner : MonoBehaviour
{
    [SerializeField] protected float spawnRandRange = 0.2f;
    [SerializeField] protected float velocity = 2;
    [Space]
    [SerializeField] ScaleByPositionSettings scaleByPosSettings;
    [Space]
    [SerializeField] protected bool flipHorisontalScale = false;
    [Space]
    [SerializeField] protected AnimationList animationProvider;
    [Space]
    [SerializeField] protected int squadId = 0;
    [SerializeField] protected Transform squadPosition;
    [SerializeField] protected SquadData squadData;
    [SerializeField] protected FactionComponentData.Faction faction;
    [Space]
    [SerializeField] private float health = 1;
    [Space]

    private EntityArchetype archetype;
    private EntityManager manager;

    private SquadTagSharedComponentData squadTag;
    private RenderSharedComponentData renderData;
    private AnimationListSharedComponentData animationData;

    private float timer = 0;
    [Space]
    [SerializeField] [Range(0.001f, 60)] private float spavnFrequency = 1;
    [SerializeField] private int maxEntityCoun = 0;

    protected virtual void Start()
    {
        manager = World.Active.EntityManager;
        archetype = manager.CreateArchetype(
            typeof(Translation),
            typeof(LinearMovementComponentData),
            typeof(VelocityAbsoluteComponentData),
            typeof(SpriteSheetAnimationComponentData),
            typeof(Scale),
            typeof(RenderScaleComponentdata),
            typeof(ScaleByPositionComponentData),
            typeof(SquadTagSharedComponentData),
            typeof(SquadComponentData),
            typeof(RenderSharedComponentData),
            typeof(FactionComponentData),
            typeof(CacheByQuadrandSystemTag),
            typeof(ProcessProjectileCollisionTag),
            typeof(AnimationListSharedComponentData),
            typeof(AnimationPauseComponentData),
            typeof(ActionOnAnimationFrameComponentData),
            typeof(DestroyWithHealthComponentData),
            typeof(HealthComponentData),
            typeof(AnimatorStatesComponentData), 
            typeof(AnimatorStateLastTriggeredAnimationComponentData), 
            typeof(FlibHorisontalByMoveDirTagComponentData),
            typeof(FlibHorisontalByTargetTagComponentData)
        );

        squadTag = DataToComponentData.ToComponentData(squadData, squadId, squadPosition.position);
        renderData = new RenderSharedComponentData()
        {
            mesh = EntitySpavner.Instance.quadMesh,
            material = EntitySpavner.Instance.animatedMeterial
        };
        animationData = new AnimationListSharedComponentData()
        {
            animations = animationProvider.Animations,
            pauses = animationProvider.PausesData,
            actions = animationProvider.ActionsData
        };
    }

    protected void Update()
    {
        var cnt = squadTag.unitCount == null ? 0 : squadTag.unitCount.value;
        if (cnt < maxEntityCoun)
        {
            timer += Time.deltaTime;
            while (timer > spavnFrequency && cnt < maxEntityCoun)
            {
                timer -= spavnFrequency;
                Spawn(1);
                cnt++;
            }
        }
    }

#if UNITY_EDITOR
    protected virtual void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 0.3f);

        if (squadPosition != null)
        {
            Gizmos.DrawLine(transform.position, squadPosition.position);
            Gizmos.DrawWireSphere(squadPosition.position, 0.2f);

            if (squadData != null)
            {
                float radius = 5;
                int horisonlalFlip = squadData.Data.directionLeftToRight ? 1 : -1;
                int verticalFlip = squadData.Data.directionBottomToTop ? 1 : -1;

                var pos = new Vector3(
                        squadPosition.position.x + Mathf.Cos((squadData.Data.rotationDegrees + 90) * Mathf.Deg2Rad) * radius * horisonlalFlip,
                        squadPosition.position.y + Mathf.Sin((squadData.Data.rotationDegrees + 90) * Mathf.Deg2Rad) * radius * verticalFlip,
                        squadPosition.position.z
                );

                Gizmos.DrawLine(squadPosition.position, pos);
            }
        }
    }
#endif

    public void Spawn(int entityCount)
    {
        var entities = new NativeArray<Entity>(entityCount, Allocator.Temp);
        manager.CreateEntity(archetype, entities);
        
        for (int i = 0; i < entities.Length; i++)
        {
            var entity = entities[i];
            SetEntityComponentsData(entity, manager);
            SetEntitySharedComponentsData(entity, manager);
            EndInitEntityData(entity, manager);
#if UNITY_EDITOR
            manager.SetName(entity, GenerateEntityName());
#endif
        }

        entities.Dispose();
    }

    protected virtual void SetEntityComponentsData(Entity entity, EntityManager manager)
    {
        var translation = this.transform.position;
        manager.SetComponentData(entity, new Translation()
        {
            Value = new float3(
                    translation.x + UnityEngine.Random.Range(-spawnRandRange, spawnRandRange),
                    translation.y + UnityEngine.Random.Range(-spawnRandRange, spawnRandRange),
                    0f
                )
        });
        manager.SetComponentData(entity, new VelocityAbsoluteComponentData()
        {
            value = velocity
        });
        manager.SetComponentData(entity, new Scale()
        {
            Value = 1f
        });
        manager.SetComponentData(entity, new RenderScaleComponentdata()
        {
            value = new float2(flipHorisontalScale ? -1 : 1, 1)
        });
        manager.SetComponentData(entity, new ScaleByPositionComponentData()
        {
            minScale = scaleByPosSettings.MinScale,
            maxScale = scaleByPosSettings.MaxScale,
            minY = scaleByPosSettings.MinY,
            maxY = scaleByPosSettings.MaxY
        });
        manager.SetComponentData(entity, new FactionComponentData() { value = faction });
        manager.SetComponentData(entity, new LinearMovementComponentData()
        {
            doMoving = true
        });
        manager.SetComponentData(entity, new HealthComponentData()
        {
            value = health
        });
        manager.SetComponentData(entity, new DestroyWithHealthComponentData()
        {
            delay = 1f
        });

        
    }

    protected virtual void SetEntitySharedComponentsData(Entity entity, EntityManager manager)
    {
        manager.SetSharedComponentData(entity, squadTag);
        manager.SetSharedComponentData(entity, renderData);
        manager.SetSharedComponentData(entity, animationData);
    }

    protected virtual void EndInitEntityData(Entity entity, EntityManager manager)
    {
        AnimationSetterUtil.SetAnimation(manager, entity, AnimationType.IDLE, 4);
    }
    
    protected virtual string GenerateEntityName()
    {
        return "Soldier";
    }

    public void IncreaceMaxEntityCount(int addCount)
    {
        if (addCount < 0) throw new ArgumentException(nameof(addCount));
        maxEntityCoun += addCount;
    }

    public void SetSpawnFrequency(float newFrequency)
    {
        spavnFrequency = newFrequency;
    }
}